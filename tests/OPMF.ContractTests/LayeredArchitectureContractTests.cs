using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Xunit;

namespace OPMF.ContractTests
{
    public class LayeredArchitectureContractTests
    {
        private const string SolutionFileName = "OffPeakMediaFetcher.sln";

        private static string FindSolutionRootFolderByWalkingUpFromTheTestAssembly()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder != null)
            {
                if (File.Exists(Path.Combine(folder.FullName, SolutionFileName)))
                {
                    return folder.FullName;
                }
                folder = folder.Parent;
            }
            throw new InvalidOperationException($"{SolutionFileName} was not found in any folder above {AppContext.BaseDirectory}.");
        }

        private static List<string> ListProductionProjectNamesInTheSolution(string solutionRootFolder)
        {
            var projectLinePattern = new Regex("^Project\\(\"[^\"]+\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\"", RegexOptions.Multiline);
            var solutionFileText = File.ReadAllText(Path.Combine(solutionRootFolder, SolutionFileName));

            return projectLinePattern.Matches(solutionFileText)
                .Select(match => new { Name = match.Groups["name"].Value, ProjectPath = match.Groups["path"].Value })
                .Where(project => project.ProjectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .Where(project => !project.ProjectPath.StartsWith("tests\\", StringComparison.OrdinalIgnoreCase))
                .Select(project => project.Name)
                .ToList();
        }

        private static string ReadBuildConfigurationBakedIntoThisTestAssembly()
        {
            return typeof(LayeredArchitectureContractTests).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(metadata => metadata.Key == "BuildConfiguration")
                .Value;
        }

        private static string FindBuiltAssemblyOfProductionProject(string solutionRootFolder, string projectName)
        {
            var assemblyFileName = projectName + ".dll";

            var copiedNextToTheTestAssembly = Path.Combine(AppContext.BaseDirectory, assemblyFileName);
            if (File.Exists(copiedNextToTheTestAssembly))
            {
                return copiedNextToTheTestAssembly;
            }

            var buildOutputFolderOfProject = Path.Combine(solutionRootFolder, projectName, "bin", ReadBuildConfigurationBakedIntoThisTestAssembly());
            var builtAssemblyInAnyTargetFramework = Directory.Exists(buildOutputFolderOfProject)
                ? Directory.GetFiles(buildOutputFolderOfProject, assemblyFileName, SearchOption.AllDirectories).FirstOrDefault()
                : null;

            return builtAssemblyInAnyTargetFramework
                ?? throw new FileNotFoundException(
                    $"The built assembly of production project {projectName} was not found next to the contract tests or under {buildOutputFolderOfProject}. " +
                    $"Reference {projectName} from OPMF.ContractTests, or build the solution before running the contract tests.");
        }

        private static List<string> ListBuiltAssembliesOfEveryProductionProject()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();
            return ListProductionProjectNamesInTheSolution(solutionRootFolder)
                .Select(projectName => FindBuiltAssemblyOfProductionProject(solutionRootFolder, projectName))
                .ToList();
        }

        private static bool TypeOrAnyTypeEnclosingItIsCompilerGenerated(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            while (true)
            {
                var hasCompilerGeneratedAttribute = typeDefinition.GetCustomAttributes()
                    .Select(metadataReader.GetCustomAttribute)
                    .Any(attribute => NameOfAttributeType(metadataReader, attribute) == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                if (hasCompilerGeneratedAttribute)
                {
                    return true;
                }

                var enclosingTypeHandle = typeDefinition.GetDeclaringType();
                if (enclosingTypeHandle.IsNil)
                {
                    return false;
                }
                typeDefinition = metadataReader.GetTypeDefinition(enclosingTypeHandle);
            }
        }

        private static string NameOfAttributeType(MetadataReader metadataReader, CustomAttribute attribute)
        {
            if (attribute.Constructor.Kind != HandleKind.MemberReference)
            {
                return null;
            }
            var constructorReference = metadataReader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
            if (constructorReference.Parent.Kind != HandleKind.TypeReference)
            {
                return null;
            }
            var attributeTypeReference = metadataReader.GetTypeReference((TypeReferenceHandle)constructorReference.Parent);
            return metadataReader.GetString(attributeTypeReference.Namespace) + "." + metadataReader.GetString(attributeTypeReference.Name);
        }

        private static string FullNameOfType(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            var enclosingTypeHandle = typeDefinition.GetDeclaringType();
            if (!enclosingTypeHandle.IsNil)
            {
                var enclosingTypeName = FullNameOfType(metadataReader, metadataReader.GetTypeDefinition(enclosingTypeHandle));
                return enclosingTypeName + "+" + metadataReader.GetString(typeDefinition.Name);
            }
            var typeNamespace = metadataReader.GetString(typeDefinition.Namespace);
            var typeName = metadataReader.GetString(typeDefinition.Name);
            return typeNamespace.Length == 0 ? typeName : typeNamespace + "." + typeName;
        }

        private static List<string> ListStaticFieldsThatAreNeitherConstNorReadonly(string assemblyFilePath)
        {
            var offendingFields = new List<string>();
            using var assemblyStream = File.OpenRead(assemblyFilePath);
            using var portableExecutableReader = new PEReader(assemblyStream);
            var metadataReader = portableExecutableReader.GetMetadataReader();

            foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions)
            {
                var typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
                if (TypeOrAnyTypeEnclosingItIsCompilerGenerated(metadataReader, typeDefinition))
                {
                    continue;
                }

                foreach (var fieldDefinitionHandle in typeDefinition.GetFields())
                {
                    var fieldDefinition = metadataReader.GetFieldDefinition(fieldDefinitionHandle);
                    var isStatic = (fieldDefinition.Attributes & FieldAttributes.Static) != 0;
                    var isConstOrReadonly = (fieldDefinition.Attributes & (FieldAttributes.Literal | FieldAttributes.InitOnly)) != 0;
                    if (isStatic && !isConstOrReadonly)
                    {
                        offendingFields.Add(FullNameOfType(metadataReader, typeDefinition) + "." + metadataReader.GetString(fieldDefinition.Name));
                    }
                }
            }
            return offendingFields;
        }

        private enum Layer
        {
            Api,
            Application,
            Service,
            Repository,
            Store,
            Domain,
            Dto,
            CrossCutting,
        }

        private static readonly Dictionary<string, Layer> LayerOfEachProductionProject = new Dictionary<string, Layer>
        {
            ["OffPeakMediaFetcher"] = Layer.Api,
            ["MediaManagerUI"] = Layer.Api,
            ["MediaManagerUI.Components"] = Layer.Api,
            ["MediaManager.Initializations"] = Layer.Application,
            ["MediaManager.Services2"] = Layer.Service,
            ["MediaManager.Repositories"] = Layer.Repository,
            ["OPMF.Database"] = Layer.Store,
            ["OPMF.Logging"] = Layer.Store,
            ["OPMF.SiteAdapter"] = Layer.Store,
            ["OPMF.SiteAdapter.Youtube"] = Layer.Store,
            ["OPMF.Downloader"] = Layer.Store,
            ["OPMF.Downloader.YTDownloader"] = Layer.Store,
            ["OPMF.Entities"] = Layer.Domain,
            ["MediaManager.Dtos"] = Layer.Dto,
            ["OPMF.Settings"] = Layer.CrossCutting,
            ["OPMF.Filesystem"] = Layer.CrossCutting,
            ["OPMF.TextLogging"] = Layer.CrossCutting,
        };

        private static readonly Dictionary<Layer, Layer[]> LayersEachLayerMayDependOn = new Dictionary<Layer, Layer[]>
        {
            [Layer.Api] = new[] { Layer.Api, Layer.Application, Layer.Dto, Layer.CrossCutting },
            [Layer.Application] = new[] { Layer.Application, Layer.Service, Layer.Dto, Layer.CrossCutting },
            [Layer.Service] = new[] { Layer.Service, Layer.Repository, Layer.Dto, Layer.CrossCutting },
            [Layer.Repository] = new[] { Layer.Repository, Layer.Store, Layer.Domain, Layer.Dto, Layer.CrossCutting },
            [Layer.Store] = new[] { Layer.Store, Layer.Domain, Layer.CrossCutting },
            [Layer.Domain] = new[] { Layer.Domain },
            [Layer.Dto] = new[] { Layer.Dto },
            [Layer.CrossCutting] = new[] { Layer.CrossCutting },
        };

        private static readonly Dictionary<Layer, string[]> DtoNamespacesEachLayerMayUse = new Dictionary<Layer, string[]>
        {
            [Layer.Api] = new[] { "MediaManager.Dtos.Application" },
            [Layer.Application] = new[] { "MediaManager.Dtos.Application", "MediaManager.Dtos.Services" },
            [Layer.Service] = new[] { "MediaManager.Dtos.Services", "MediaManager.Dtos.Repositories" },
            [Layer.Repository] = new[] { "MediaManager.Dtos.Repositories" },
        };

        private const string DtoAssemblyName = "MediaManager.Dtos";
        private const string DatabaseModelAssemblyName = "OPMF.Entities";

        private static List<(string ProjectName, string ProjectFilePath)> ListProductionProjectsInTheSolution(string solutionRootFolder)
        {
            var projectLinePattern = new Regex("^Project\\(\"[^\"]+\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\"", RegexOptions.Multiline);
            var solutionFileText = File.ReadAllText(Path.Combine(solutionRootFolder, SolutionFileName));

            return projectLinePattern.Matches(solutionFileText)
                .Select(match => (ProjectName: match.Groups["name"].Value, RelativePath: match.Groups["path"].Value))
                .Where(project => project.RelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .Where(project => !project.RelativePath.StartsWith("tests\\", StringComparison.OrdinalIgnoreCase))
                .Select(project => (project.ProjectName, Path.Combine(solutionRootFolder, project.RelativePath.Replace('\\', Path.DirectorySeparatorChar))))
                .ToList();
        }

        private static List<string> NamesOfProjectsDeclaredInProjectReferencesOf(string projectFilePath)
        {
            var projectReferencePattern = new Regex("<ProjectReference\\s+Include=\"(?<path>[^\"]+)\"");
            return projectReferencePattern.Matches(File.ReadAllText(projectFilePath))
                .Select(match => Path.GetFileNameWithoutExtension(match.Groups["path"].Value.Replace('\\', '/')))
                .ToList();
        }

        private static MetadataReader OpenMetadataReader(string assemblyFilePath, out PEReader portableExecutableReader)
        {
            portableExecutableReader = new PEReader(File.OpenRead(assemblyFilePath));
            return portableExecutableReader.GetMetadataReader();
        }

        private static List<string> NamesOfAssembliesReferencedBy(string assemblyFilePath)
        {
            var metadataReader = OpenMetadataReader(assemblyFilePath, out var portableExecutableReader);
            using (portableExecutableReader)
            {
                return metadataReader.AssemblyReferences
                    .Select(handle => metadataReader.GetString(metadataReader.GetAssemblyReference(handle).Name))
                    .ToList();
            }
        }

        private static string NameOfAssemblyThatDefines(MetadataReader metadataReader, TypeReference typeReference)
        {
            switch (typeReference.ResolutionScope.Kind)
            {
                case HandleKind.AssemblyReference:
                    return metadataReader.GetString(metadataReader.GetAssemblyReference((AssemblyReferenceHandle)typeReference.ResolutionScope).Name);
                case HandleKind.TypeReference:
                    return NameOfAssemblyThatDefines(metadataReader, metadataReader.GetTypeReference((TypeReferenceHandle)typeReference.ResolutionScope));
                default:
                    return null;
            }
        }

        private static string NamespaceOf(MetadataReader metadataReader, TypeReference typeReference)
        {
            if (typeReference.ResolutionScope.Kind == HandleKind.TypeReference)
            {
                return NamespaceOf(metadataReader, metadataReader.GetTypeReference((TypeReferenceHandle)typeReference.ResolutionScope));
            }
            return metadataReader.GetString(typeReference.Namespace);
        }

        private static List<(string AssemblyName, string Namespace)> TypesReferencedBy(string assemblyFilePath)
        {
            var metadataReader = OpenMetadataReader(assemblyFilePath, out var portableExecutableReader);
            using (portableExecutableReader)
            {
                return metadataReader.TypeReferences
                    .Select(handle => metadataReader.GetTypeReference(handle))
                    .Select(typeReference => (AssemblyName: NameOfAssemblyThatDefines(metadataReader, typeReference), Namespace: NamespaceOf(metadataReader, typeReference)))
                    .Where(type => type.AssemblyName != null)
                    .Distinct()
                    .ToList();
            }
        }

        [Fact]
        public void EveryProductionProjectInTheSolutionHasALayerAssigned()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();

            var projectsWithNoLayer = ListProductionProjectsInTheSolution(solutionRootFolder)
                .Select(project => project.ProjectName)
                .Where(projectName => !LayerOfEachProductionProject.ContainsKey(projectName))
                .ToList();

            Assert.True(projectsWithNoLayer.Count == 0, "Add these projects to the layer map: " + string.Join(", ", projectsWithNoLayer));
        }

        [Fact]
        public void EveryProductionProjectDeclaresOnlyProjectReferencesItsLayerAllows()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();
            var violations = new List<string>();

            foreach (var (projectName, projectFilePath) in ListProductionProjectsInTheSolution(solutionRootFolder))
            {
                var layerOfProject = LayerOfEachProductionProject[projectName];
                foreach (var referencedProjectName in NamesOfProjectsDeclaredInProjectReferencesOf(projectFilePath))
                {
                    var layerOfReferencedProject = LayerOfEachProductionProject[referencedProjectName];
                    if (!LayersEachLayerMayDependOn[layerOfProject].Contains(layerOfReferencedProject))
                    {
                        violations.Add($"{projectName} ({layerOfProject}) declares a reference to {referencedProjectName} ({layerOfReferencedProject})");
                    }
                }
            }

            Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
        }

        [Fact]
        public void EveryProductionAssemblyUsesOnlyAssembliesItsLayerAllows()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();
            var violations = new List<string>();

            foreach (var (projectName, _) in ListProductionProjectsInTheSolution(solutionRootFolder))
            {
                var layerOfProject = LayerOfEachProductionProject[projectName];
                var assemblyFilePath = FindBuiltAssemblyOfProductionProject(solutionRootFolder, projectName);
                foreach (var usedAssemblyName in NamesOfAssembliesReferencedBy(assemblyFilePath).Where(LayerOfEachProductionProject.ContainsKey))
                {
                    var layerOfUsedAssembly = LayerOfEachProductionProject[usedAssemblyName];
                    if (!LayersEachLayerMayDependOn[layerOfProject].Contains(layerOfUsedAssembly))
                    {
                        violations.Add($"{projectName} ({layerOfProject}) uses {usedAssemblyName} ({layerOfUsedAssembly})");
                    }
                }
            }

            Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
        }

        [Fact]
        public void EveryProductionAssemblyUsesOnlyTheDtoNamespacesOfTheBoundariesDirectlyAboveAndBelowIt()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();
            var violations = new List<string>();

            foreach (var (projectName, _) in ListProductionProjectsInTheSolution(solutionRootFolder))
            {
                var layerOfProject = LayerOfEachProductionProject[projectName];
                var dtoNamespacesTheLayerMayUse = DtoNamespacesEachLayerMayUse.TryGetValue(layerOfProject, out var allowedNamespaces) ? allowedNamespaces : new string[0];
                var assemblyFilePath = FindBuiltAssemblyOfProductionProject(solutionRootFolder, projectName);
                var dtoNamespacesUsed = TypesReferencedBy(assemblyFilePath)
                    .Where(type => type.AssemblyName == DtoAssemblyName)
                    .Select(type => type.Namespace)
                    .Distinct();
                foreach (var dtoNamespaceUsed in dtoNamespacesUsed.Where(dtoNamespace => !dtoNamespacesTheLayerMayUse.Contains(dtoNamespace)))
                {
                    violations.Add($"{projectName} ({layerOfProject}) uses types of {dtoNamespaceUsed}");
                }
            }

            Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
        }

        [Fact]
        public void OnlyRepositoryAndStoreAssembliesUseDatabaseModelTypes()
        {
            var solutionRootFolder = FindSolutionRootFolderByWalkingUpFromTheTestAssembly();
            var layersThatMayUseDatabaseModelTypes = new[] { Layer.Repository, Layer.Store, Layer.Domain };
            var violations = new List<string>();

            foreach (var (projectName, _) in ListProductionProjectsInTheSolution(solutionRootFolder))
            {
                var layerOfProject = LayerOfEachProductionProject[projectName];
                var assemblyFilePath = FindBuiltAssemblyOfProductionProject(solutionRootFolder, projectName);
                var usesDatabaseModelTypes = TypesReferencedBy(assemblyFilePath).Any(type => type.AssemblyName == DatabaseModelAssemblyName);
                if (usesDatabaseModelTypes && !layersThatMayUseDatabaseModelTypes.Contains(layerOfProject))
                {
                    violations.Add($"{projectName} ({layerOfProject}) uses database model types of {DatabaseModelAssemblyName}");
                }
            }

            Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
        }

        [Fact]
        public void NoProductionTypeHasAStaticFieldThatIsNotReadonly()
        {
            var staticFieldsThatAreNeitherConstNorReadonly = ListBuiltAssembliesOfEveryProductionProject()
                .SelectMany(ListStaticFieldsThatAreNeitherConstNorReadonly)
                .OrderBy(fieldName => fieldName, StringComparer.Ordinal)
                .ToList();

            Assert.True(
                staticFieldsThatAreNeitherConstNorReadonly.Count == 0,
                "Process-wide state is not allowed. Static fields that are neither const nor readonly:" + Environment.NewLine +
                string.Join(Environment.NewLine, staticFieldsThatAreNeitherConstNorReadonly));
        }
    }
}
