using System.IO;
using OPMF.Filesystem;
using OPMF.IntegrationTests.Support;
using Xunit;

namespace OPMF.IntegrationTests.Filesystem
{
    public class MissingFolderCreatorTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly MissingFolderCreator _missingFolderCreator = new MissingFolderCreator();

        public MissingFolderCreatorTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        [Fact]
        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing_CreatesAllExpectedDirectories()
        {
            _missingFolderCreator.CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(_fixture.ReadonlySettings);

            Assert.True(Directory.Exists(_fixture.ReadonlySettings.GetLocalAppFolderPath()));
            Assert.True(Directory.Exists(_fixture.ReadonlySettings.GetDownloadFolderPath()));
            Assert.True(Directory.Exists(_fixture.ReadonlySettings.GetDatabaseFolderPath()));
            Assert.True(Directory.Exists(_fixture.ReadonlySettings.GetBinFolderPath()));
        }

        [Fact]
        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing_IsIdempotent()
        {
            _missingFolderCreator.CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(_fixture.ReadonlySettings);
            _missingFolderCreator.CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(_fixture.ReadonlySettings);
            Assert.True(Directory.Exists(_fixture.ReadonlySettings.GetLocalAppFolderPath()));
        }

        [Fact]
        public void CreateFolderWhenMissing_CreatesConfiguredVideoOutputFolder()
        {
            var sentinel = Path.Combine(Path.GetTempPath(), "OPMF_VidOut_" + System.Guid.NewGuid().ToString("N"));
            _fixture.AppConfig.VideoOutputFolderPath = sentinel;
            try
            {
                _missingFolderCreator.CreateFolderWhenMissing(_fixture.AppConfig.VideoOutputFolderPath);
                Assert.True(Directory.Exists(sentinel));
            }
            finally
            {
                if (Directory.Exists(sentinel)) Directory.Delete(sentinel, true);
            }
        }
    }
}
