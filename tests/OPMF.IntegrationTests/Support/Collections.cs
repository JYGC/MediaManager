using Xunit;

namespace OPMF.IntegrationTests.Support
{
    [CollectionDefinition("IsolatedAppFolder", DisableParallelization = true)]
    public class IsolatedAppFolderCollection : ICollectionFixture<IsolatedAppFolderFixture>
    {
    }
}
