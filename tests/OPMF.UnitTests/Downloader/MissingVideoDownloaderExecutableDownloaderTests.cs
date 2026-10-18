using System;
using System.IO;
using System.Linq;
using OPMF.Downloader.YTDownloader;
using OPMF.Entities;
using OPMF.Logging;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class MissingVideoDownloaderExecutableDownloaderTests : IDisposable
    {
        private const string UrlYoutubeDlIsDownloadedFromToday = "https://yt-dl.org/latest/youtube-dl.exe";

        private readonly TemporaryFolderForOneTest _temporaryFolder = new TemporaryFolderForOneTest();
        private readonly StringWriter _standardOutput = new StringWriter();
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly RecordingUrlToFileDownloader _urlToFileDownloader;
        private readonly MissingVideoDownloaderExecutableDownloader _missingVideoDownloaderExecutableDownloader;
        private readonly VideoDownloaderExecutableLocation _youtubeDlLocation;
        private readonly VideoDownloaderExecutableLocation _ytDlpLocation;

        public MissingVideoDownloaderExecutableDownloaderTests()
        {
            _urlToFileDownloader = new RecordingUrlToFileDownloader(_standardOutput);
            _missingVideoDownloaderExecutableDownloader = new MissingVideoDownloaderExecutableDownloader(_urlToFileDownloader, new Logger(_databaseConnectionOpener), _standardOutput);
            var readonlySettings = new TemporaryFolderReadonlySettings(_temporaryFolder.FolderPath);
            _youtubeDlLocation = new VideoDownloaderExecutableLocation("youtube-dl", readonlySettings.GetYoutubeDLPath(), UrlYoutubeDlIsDownloadedFromToday);
            _ytDlpLocation = new VideoDownloaderExecutableLocation("yt-dlp", readonlySettings.GetYtDlpExecutablePath(), "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe");
        }

        public void Dispose()
        {
            _databaseConnectionOpener.Dispose();
            _temporaryFolder.Dispose();
        }

        private static void CreateTheExecutableFile(VideoDownloaderExecutableLocation location)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(location.ExecutableFilePath));
            File.WriteAllText(location.ExecutableFilePath, "already downloaded");
        }

        [Fact]
        public void NothingIsDownloadedOrPrintedWhenTheExecutableFileExists()
        {
            CreateTheExecutableFile(_youtubeDlLocation);

            _missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(_youtubeDlLocation);

            Assert.Empty(_urlToFileDownloader.Downloads);
            Assert.Equal(string.Empty, _standardOutput.ToString());
        }

        [Fact]
        public void WhenTheExecutableFileIsMissingItIsDownloadedFromTheUrlToTheFilePath()
        {
            _missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(_youtubeDlLocation);

            var download = Assert.Single(_urlToFileDownloader.Downloads);
            Assert.Equal(UrlYoutubeDlIsDownloadedFromToday, download.Url);
            Assert.Equal(_youtubeDlLocation.ExecutableFilePath, download.FilePath);
            Assert.EndsWith(Path.Combine("Bin", "youtube-dl.exe"), download.FilePath);
        }

        [Fact]
        public void TheDownloadingMessageIsWrittenBeforeTheDownloadStarts()
        {
            _missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(_youtubeDlLocation);

            var download = Assert.Single(_urlToFileDownloader.Downloads);
            Assert.Equal("downloading youtube-dl binary" + Environment.NewLine, download.StandardOutputWhenTheDownloadWasRequested);
        }

        [Fact]
        public void TheExceptionOfTheUrlToFileDownloaderPropagates()
        {
            var exceptionOfTheDownload = new InvalidOperationException("the download failed");
            _urlToFileDownloader.ExceptionToThrowOnEveryDownload = exceptionOfTheDownload;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(() => _missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(_youtubeDlLocation));

            Assert.Same(exceptionOfTheDownload, exceptionThatPropagated);
        }

        [Fact]
        public void WithTheYtDlpLocationTheYtDlpMessageIsWrittenAndYtDlpIsDownloadedFromItsUrlToItsPath()
        {
            _missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(_ytDlpLocation);

            var download = Assert.Single(_urlToFileDownloader.Downloads);
            Assert.Equal("downloading yt-dlp binary" + Environment.NewLine, download.StandardOutputWhenTheDownloadWasRequested);
            Assert.Equal("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", download.Url);
            Assert.EndsWith(Path.Combine("Bin", "yt-dlp.exe"), download.FilePath);
        }

        [Fact]
        public void TheFallbackIsAvailableWithoutADownloadWhenItsFileExists()
        {
            CreateTheExecutableFile(_ytDlpLocation);

            var fallbackIsAvailable = _missingVideoDownloaderExecutableDownloader.DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(_ytDlpLocation);

            Assert.True(fallbackIsAvailable);
            Assert.Empty(_urlToFileDownloader.Downloads);
        }

        [Fact]
        public void TheFallbackIsAvailableWhenItsMissingFileIsDownloadedSuccessfully()
        {
            var fallbackIsAvailable = _missingVideoDownloaderExecutableDownloader.DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(_ytDlpLocation);

            Assert.True(fallbackIsAvailable);
            Assert.Single(_urlToFileDownloader.Downloads);
        }

        [Fact]
        public void TheFallbackIsNotAvailableWhenItsDownloadThrows()
        {
            _urlToFileDownloader.ExceptionToThrowOnEveryDownload = new InvalidOperationException("the download failed");

            var fallbackIsAvailable = _missingVideoDownloaderExecutableDownloader.DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(_ytDlpLocation);

            Assert.False(fallbackIsAvailable);
        }

        [Fact]
        public void AFailedDownloadOfTheFallbackLogsAWarningNamingTheExecutable()
        {
            _urlToFileDownloader.ExceptionToThrowOnEveryDownload = new InvalidOperationException("the download failed");

            _missingVideoDownloaderExecutableDownloader.DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(_ytDlpLocation);

            OPMFLog loggedEntry = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => loggedEntry = databaseAdapter.OPMFLogDbCollection.GetAll().Single());
            Assert.Equal(OPMFLogType.Warning, loggedEntry.Type);
            Assert.Contains("yt-dlp", loggedEntry.Message);
            Assert.Contains("the download failed", loggedEntry.Message);
            Assert.Contains("InvalidOperationException", loggedEntry.ExceptionObject);
        }
    }
}
