using System.IO;
using OPMF.IntegrationTests.Support;
using OPMF.Settings;
using Xunit;

namespace OPMF.IntegrationTests.Filesystem
{
    public class FolderSetupTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public FolderSetupTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void EstablishFolders_CreatesAllExpectedDirectories()
        {
            OPMF.Filesystem.FolderSetup.EstablishFolders();

            Assert.True(Directory.Exists(ConfigHelper.ReadonlySettings.GetLocalAppFolderPath()));
            Assert.True(Directory.Exists(ConfigHelper.ReadonlySettings.GetDownloadFolderPath()));
            Assert.True(Directory.Exists(ConfigHelper.ReadonlySettings.GetDatabaseFolderPath()));
            Assert.True(Directory.Exists(ConfigHelper.ReadonlySettings.GetBinFolderPath()));
        }

        [Fact]
        public void EstablishFolders_IsIdempotent()
        {
            OPMF.Filesystem.FolderSetup.EstablishFolders();
            OPMF.Filesystem.FolderSetup.EstablishFolders();
            Assert.True(Directory.Exists(ConfigHelper.ReadonlySettings.GetLocalAppFolderPath()));
        }

        [Fact]
        public void EstablishVideoOutputFolder_CreatesConfiguredFolder()
        {
            var sentinel = Path.Combine(Path.GetTempPath(), "OPMF_VidOut_" + System.Guid.NewGuid().ToString("N"));
            ConfigHelper.Config.VideoOutputFolderPath = sentinel;
            try
            {
                OPMF.Filesystem.FolderSetup.EstablishVideoOutputFolder();
                Assert.True(Directory.Exists(sentinel));
            }
            finally
            {
                if (Directory.Exists(sentinel)) Directory.Delete(sentinel, true);
            }
        }
    }
}
