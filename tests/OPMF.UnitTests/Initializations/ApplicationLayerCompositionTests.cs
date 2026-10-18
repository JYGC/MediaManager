using System;
using System.IO;
using MediaManager.Initializations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Initializations
{
    public class ApplicationLayerCompositionTests : IDisposable
    {
        private static readonly Type[] EveryApplicationInterface =
        {
            typeof(IApplicationStartupUseCase),
            typeof(IUnhandledErrorRecordingUseCase),
            typeof(IApplicationEntryPointUseCase),
            typeof(ICommandLineTaskUseCases),
            typeof(IChannelBrowsingUseCases),
            typeof(IVideoBrowsingUseCases),
            typeof(ILogBrowsingUseCases),
        };

        private readonly TemporaryFolderForOneTest _temporaryFolder = new TemporaryFolderForOneTest();

        public void Dispose() => _temporaryFolder.Dispose();

        private ServiceProvider BuildTheContainerTheCompositionRootsBuild()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero)));
            services.AddSingleton<TextWriter>(new StringWriter());
            services.AddMediaManagerApplicationLayer();
            return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        }

        private void FillTheSettingsStartupKeeps(ServiceProvider serviceProvider)
        {
            var settingsEstablishedAtStartup = serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>();
            settingsEstablishedAtStartup.KeepReadonlySettingsInitializedAtStartup(new TemporaryFolderReadonlySettings(_temporaryFolder.FolderPath));
            settingsEstablishedAtStartup.KeepAppConfigEstablishedAtStartup(new AppConfig());
        }

        [Fact]
        public void TheContainerValidatesWhenItIsBuilt()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();

            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void EveryApplicationInterfaceCanBeResolved()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();
            FillTheSettingsStartupKeeps(serviceProvider);

            foreach (var applicationInterface in EveryApplicationInterface)
            {
                Assert.NotNull(serviceProvider.GetRequiredService(applicationInterface));
            }
        }

        [Fact]
        public void ResolvingEveryApplicationInterfaceCreatesNoStoreClientSoNothingIsDownloadedOrWrittenToDisk()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();
            FillTheSettingsStartupKeeps(serviceProvider);

            foreach (var applicationInterface in EveryApplicationInterface)
            {
                serviceProvider.GetRequiredService(applicationInterface);
            }

            Assert.Empty(Directory.GetFiles(_temporaryFolder.FolderPath, "*", SearchOption.AllDirectories));
        }

        [Fact]
        public void TheEntryPointCanBeResolvedBeforeStartupKeptTheSettingsBecauseStartupIsWhatKeepsThem()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();

            var applicationEntryPointUseCase = serviceProvider.GetRequiredService<IApplicationEntryPointUseCase>();

            Assert.NotNull(applicationEntryPointUseCase);
        }

        [Fact]
        public void TheSettingsEstablishedAtStartupAreOneObjectPerContainer()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();

            Assert.Same(serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>(), serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>());
        }

        [Fact]
        public void TheReadonlySettingsAndTheConfigAreTheOnesStartupKept()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();
            var readonlySettingsStartupKeeps = new TemporaryFolderReadonlySettings(_temporaryFolder.FolderPath);
            var appConfigStartupKeeps = new AppConfig();
            var settingsEstablishedAtStartup = serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>();
            settingsEstablishedAtStartup.KeepReadonlySettingsInitializedAtStartup(readonlySettingsStartupKeeps);
            settingsEstablishedAtStartup.KeepAppConfigEstablishedAtStartup(appConfigStartupKeeps);

            Assert.Same(readonlySettingsStartupKeeps, serviceProvider.GetRequiredService<IReadonlySettings>());
            Assert.Same(appConfigStartupKeeps, serviceProvider.GetRequiredService<AppConfig>());
        }

        [Fact]
        public void TheReadonlySettingsCannotBeResolvedBeforeStartupKeptThem()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();

            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IReadonlySettings>());
        }

        [Fact]
        public void TheTextLogIsOneObjectPerContainer()
        {
            using var serviceProvider = BuildTheContainerTheCompositionRootsBuild();
            FillTheSettingsStartupKeeps(serviceProvider);

            Assert.Same(
                serviceProvider.GetRequiredService<OPMF.TextLogging.TextLog>(),
                serviceProvider.GetRequiredService<OPMF.TextLogging.TextLog>());
        }
    }
}
