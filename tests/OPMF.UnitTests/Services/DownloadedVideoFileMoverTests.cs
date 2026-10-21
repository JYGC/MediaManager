using System;
using System.IO;
using System.Linq;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class DownloadedVideoFileMoverTests : IDisposable
    {
        private readonly TemporaryFolderForOneTest _temporaryFolder = new TemporaryFolderForOneTest();
        private readonly string _sourceFolderPath;
        private readonly string _destinationFolderPath;
        private readonly DownloadedVideoFileMover _downloadedVideoFileMover = new DownloadedVideoFileMover();

        public DownloadedVideoFileMoverTests()
        {
            _sourceFolderPath = Path.Combine(_temporaryFolder.FolderPath, "Downloads");
            _destinationFolderPath = Path.Combine(_temporaryFolder.FolderPath, "VideoOutput");
            Directory.CreateDirectory(_sourceFolderPath);
            Directory.CreateDirectory(_destinationFolderPath);
        }

        public void Dispose() => _temporaryFolder.Dispose();

        private void CreateFileInSourceFolder(string fileName, string content = "content") => File.WriteAllText(Path.Combine(_sourceFolderPath, fileName), content);

        private static string[] FileNamesIn(string folderPath) => Directory.GetFiles(folderPath).Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        [Fact]
        public void OnlyFilesWithTheGivenExtensionsAreMoved()
        {
            CreateFileInSourceFolder("video.mp4");
            CreateFileInSourceFolder("subtitles.vtt");
            CreateFileInSourceFolder("notes.txt");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4", "vtt" });

            Assert.Equal(new[] { "subtitles.vtt", "video.mp4" }, FileNamesIn(_destinationFolderPath));
            Assert.Equal(new[] { "notes.txt" }, FileNamesIn(_sourceFolderPath));
        }

        [Fact]
        public void AMovedFileKeepsItsContent()
        {
            CreateFileInSourceFolder("video.mp4", "the video bytes");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4" });

            Assert.Equal("the video bytes", File.ReadAllText(Path.Combine(_destinationFolderPath, "video.mp4")));
        }

        [Fact]
        public void AFileWhoseNameAlreadyExistsInTheDestinationIsSkippedAndLeftInTheSource()
        {
            CreateFileInSourceFolder("video.mp4", "new content");
            File.WriteAllText(Path.Combine(_destinationFolderPath, "video.mp4"), "old content");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4" });

            Assert.Equal("old content", File.ReadAllText(Path.Combine(_destinationFolderPath, "video.mp4")));
            Assert.Equal("new content", File.ReadAllText(Path.Combine(_sourceFolderPath, "video.mp4")));
        }

        [Fact]
        public void ASkippedFileDoesNotStopTheOtherFilesFromMoving()
        {
            CreateFileInSourceFolder("a.mp4");
            CreateFileInSourceFolder("b.mp4");
            File.WriteAllText(Path.Combine(_destinationFolderPath, "a.mp4"), "old content");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4" });

            Assert.Equal(new[] { "a.mp4", "b.mp4" }, FileNamesIn(_destinationFolderPath));
            Assert.Equal(new[] { "a.mp4" }, FileNamesIn(_sourceFolderPath));
        }

        [Fact]
        public void AFileWhoseNameOnlyEndsWithTheExtensionTextButNotAfterADotIsNotMoved()
        {
            CreateFileInSourceFolder("not-a-video-mp4");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4" });

            Assert.Empty(FileNamesIn(_destinationFolderPath));
        }

        [Fact]
        public void NoExtensionsMoveNothing()
        {
            CreateFileInSourceFolder("video.mp4");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, Array.Empty<string>());

            Assert.Empty(FileNamesIn(_destinationFolderPath));
        }

        [Fact]
        public void AStreamFileLeftBeforeMergingStaysInTheSourceFolderWhileTheMergedFileMoves()
        {
            CreateFileInSourceFolder("Title.f137.mp4");
            CreateFileInSourceFolder("Title.f140.m4a");
            CreateFileInSourceFolder("Title.mp4");

            _downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(_sourceFolderPath, _destinationFolderPath, new[] { "mp4", "m4a" });

            Assert.Equal(new[] { "Title.mp4" }, FileNamesIn(_destinationFolderPath));
            Assert.Equal(new[] { "Title.f137.mp4", "Title.f140.m4a" }, FileNamesIn(_sourceFolderPath));
        }
    }
}
