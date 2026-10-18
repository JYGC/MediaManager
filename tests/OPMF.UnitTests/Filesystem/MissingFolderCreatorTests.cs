using System.IO;
using OPMF.Filesystem;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Filesystem
{
    public class MissingFolderCreatorTests
    {
        private readonly MissingFolderCreator _missingFolderCreator = new MissingFolderCreator();

        [Fact]
        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissingCreatesAllFourFolders()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var readonlySettings = new TemporaryFolderReadonlySettings(temporaryFolder.FolderPath);

            _missingFolderCreator.CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(readonlySettings);

            Assert.True(Directory.Exists(readonlySettings.GetLocalAppFolderPath()));
            Assert.True(Directory.Exists(readonlySettings.GetDownloadFolderPath()));
            Assert.True(Directory.Exists(readonlySettings.GetDatabaseFolderPath()));
            Assert.True(Directory.Exists(readonlySettings.GetBinFolderPath()));
        }

        [Fact]
        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissingLeavesFoldersThatExistAloneContentsIncluded()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var readonlySettings = new TemporaryFolderReadonlySettings(temporaryFolder.FolderPath);
            Directory.CreateDirectory(readonlySettings.GetBinFolderPath());
            var fileAlreadyInTheBinFolder = Path.Combine(readonlySettings.GetBinFolderPath(), "youtube-dl.exe");
            File.WriteAllText(fileAlreadyInTheBinFolder, "already here");

            _missingFolderCreator.CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(readonlySettings);

            Assert.Equal("already here", File.ReadAllText(fileAlreadyInTheBinFolder));
        }

        [Fact]
        public void CreateFolderWhenMissingCreatesTheGivenFolderIncludingItsParents()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var folderPath = Path.Combine(temporaryFolder.FolderPath, "parent", "VideoOutput");

            _missingFolderCreator.CreateFolderWhenMissing(folderPath);

            Assert.True(Directory.Exists(folderPath));
        }

        [Fact]
        public void CreateFolderWhenMissingLeavesAFolderThatExistsAloneContentsIncluded()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var folderPath = Path.Combine(temporaryFolder.FolderPath, "VideoOutput");
            Directory.CreateDirectory(folderPath);
            var fileAlreadyInTheFolder = Path.Combine(folderPath, "video.mp4");
            File.WriteAllText(fileAlreadyInTheFolder, "already here");

            _missingFolderCreator.CreateFolderWhenMissing(folderPath);

            Assert.Equal("already here", File.ReadAllText(fileAlreadyInTheFolder));
        }
    }
}
