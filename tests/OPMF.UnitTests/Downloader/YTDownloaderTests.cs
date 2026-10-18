using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Time.Testing;
using OPMF.Downloader.YTDownloader;
using OPMF.Entities;
using OPMF.Logging;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class YTDownloaderTests : IDisposable
    {
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private readonly TemporaryFolderForOneTest _temporaryFolder = new TemporaryFolderForOneTest();
        private readonly FakeVideoDownloaderExecutableRunner _fakeVideoDownloaderExecutableRunner = new FakeVideoDownloaderExecutableRunner();
        private readonly RecordingMissingVideoDownloaderExecutableDownloader _missingVideoDownloaderExecutableDownloader = new RecordingMissingVideoDownloaderExecutableDownloader();
        private readonly FakeInterruptibleDownloadThreadStarter _fakeThreadStarter = new FakeInterruptibleDownloadThreadStarter();
        private readonly RecordingConsoleRowOverwriter _consoleRowOverwriter = new RecordingConsoleRowOverwriter();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
        private readonly FakeCurrentThreadWaiter _fakeCurrentThreadWaiter;
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly TemporaryFolderReadonlySettings _readonlySettings;
        private readonly AppConfig _appConfig = new AppConfig();

        public YTDownloaderTests()
        {
            _readonlySettings = new TemporaryFolderReadonlySettings(_temporaryFolder.FolderPath);
            _fakeCurrentThreadWaiter = new FakeCurrentThreadWaiter(_fakeTimeProvider)
            {
                RunWhileWaiting = _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet,
            };
        }

        public void Dispose()
        {
            _databaseConnectionOpener.Dispose();
            _temporaryFolder.Dispose();
        }

        private YTDownloader CreateYTDownloader()
        {
            return new YTDownloader(
                _readonlySettings,
                _appConfig,
                _fakeVideoDownloaderExecutableRunner,
                _missingVideoDownloaderExecutableDownloader,
                _fakeThreadStarter,
                _fakeCurrentThreadWaiter,
                _consoleRowOverwriter,
                _fakeTimeProvider,
                new Logger(_databaseConnectionOpener));
        }

        private static List<Metadata> QueueOfVideosNumbered(int firstNumber, int lastNumber)
        {
            return Enumerable.Range(firstNumber, lastNumber - firstNumber + 1)
                .Select(number => (Metadata)new YoutubeMetadata { SiteId = "video-" + number, Url = "https://www.youtube.com/watch?v=video-" + number, Title = "Video " + number })
                .ToList();
        }

        private YTDownloader CreateYTDownloaderToDownload(List<Metadata> queue)
        {
            var downloader = CreateYTDownloader();
            downloader.DownloadQueue = queue;
            return downloader;
        }

        [Fact]
        public void TheConstructorAsksForYoutubeDlWithItsNamePathAndGithubUrlBeforeAnythingElse()
        {
            CreateYTDownloader();

            var locationAskedFor = Assert.Single(_missingVideoDownloaderExecutableDownloader.LocationsAskedToDownloadWhenMissing);
            Assert.Equal("youtube-dl", locationAskedFor.ExecutableName);
            Assert.Equal(_readonlySettings.GetYoutubeDLPath(), locationAskedFor.ExecutableFilePath);
            Assert.Equal("https://github.com/ytdl-org/youtube-dl/releases/latest/download/youtube-dl.exe", locationAskedFor.DownloadUrlUsedWhenExecutableIsMissing);
            Assert.Equal(0, _fakeThreadStarter.ThreadsStarted);
            Assert.Empty(_fakeVideoDownloaderExecutableRunner.Runs);
        }

        [Fact]
        public void WithTheDefaultSettingYoutubeDlIsTheExecutableTriedFirstAndYtDlpIsTheFallback()
        {
            CreateYTDownloader();

            var firstLocation = Assert.Single(_missingVideoDownloaderExecutableDownloader.LocationsAskedToDownloadWhenMissing);
            var fallbackLocation = Assert.Single(_missingVideoDownloaderExecutableDownloader.FallbackLocationsAskedToDownloadWhenMissing);
            Assert.Equal("youtube-dl", firstLocation.ExecutableName);
            Assert.Equal("yt-dlp", fallbackLocation.ExecutableName);
            Assert.Equal(_readonlySettings.GetYtDlpExecutablePath(), fallbackLocation.ExecutableFilePath);
            Assert.Equal("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", fallbackLocation.DownloadUrlUsedWhenExecutableIsMissing);
        }

        [Fact]
        public void WithTheYtDlpSettingYtDlpIsTheExecutableTriedFirstAndYoutubeDlIsTheFallback()
        {
            _appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos = VideoDownloaderExecutable.YtDlp;

            CreateYTDownloader();

            var firstLocation = Assert.Single(_missingVideoDownloaderExecutableDownloader.LocationsAskedToDownloadWhenMissing);
            var fallbackLocation = Assert.Single(_missingVideoDownloaderExecutableDownloader.FallbackLocationsAskedToDownloadWhenMissing);
            Assert.Equal("yt-dlp", firstLocation.ExecutableName);
            Assert.Equal("youtube-dl", fallbackLocation.ExecutableName);
        }

        [Fact]
        public void AFailedDownloadOfTheExecutableTriedFirstPropagatesBeforeTheFallbackIsAskedFor()
        {
            _missingVideoDownloaderExecutableDownloader.ExceptionToThrowWhenAskedToDownload = new InvalidOperationException("the download failed");

            Assert.Throws<InvalidOperationException>(CreateYTDownloader);

            Assert.Empty(_missingVideoDownloaderExecutableDownloader.FallbackLocationsAskedToDownloadWhenMissing);
        }

        [Fact]
        public void ANumberWithNoExecutableThrowsAnArgumentOutOfRangeExceptionBeforeAnythingIsDownloaded()
        {
            _appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos = (VideoDownloaderExecutable)99;

            Assert.Throws<ArgumentOutOfRangeException>(CreateYTDownloader);

            Assert.Empty(_missingVideoDownloaderExecutableDownloader.LocationsAskedToDownloadWhenMissing);
            Assert.Empty(_missingVideoDownloaderExecutableDownloader.FallbackLocationsAskedToDownloadWhenMissing);
        }

        [Fact]
        public void TheInstancesAreGivenTheFallbackSoAFailedVideoIsRetriedWithIt()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "ERROR: unable to download");
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 1));

            downloader.StartDownloadingQueue();

            Assert.Equal(
                new[] { _readonlySettings.GetYoutubeDLPath(), _readonlySettings.GetYtDlpExecutablePath() },
                _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void WhenTheFallbackIsReportedUnavailableTheInstancesGetNoFallbackSoAFailedVideoIsNotRetried()
        {
            _missingVideoDownloaderExecutableDownloader.FallbackIsAvailable = false;
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "ERROR: unable to download");
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 1));

            downloader.StartDownloadingQueue();

            Assert.Equal(new[] { _readonlySettings.GetYoutubeDLPath() }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void AFailureToDownloadYoutubeDlPropagatesFromTheConstructor()
        {
            _missingVideoDownloaderExecutableDownloader.ExceptionToThrowWhenAskedToDownload = new InvalidOperationException("the download failed");

            Assert.Throws<InvalidOperationException>(CreateYTDownloader);
        }

        [Fact]
        public void AnEmptyQueueDownloadsNothingAndNeverWaits()
        {
            var downloader = CreateYTDownloaderToDownload(new List<Metadata>());

            downloader.StartDownloadingQueue();

            Assert.Empty(_fakeVideoDownloaderExecutableRunner.Runs);
            Assert.Equal(0, _fakeThreadStarter.ThreadsStarted);
            Assert.Empty(_fakeCurrentThreadWaiter.Waits);
        }

        [Fact]
        public void EachVideoIsDownloadedOnceInQueueOrder()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 5;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 7));

            downloader.StartDownloadingQueue();

            Assert.Equal(
                QueueOfVideosNumbered(1, 7).Select(video => video.Url),
                _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.VideoUrl));
        }

        [Fact]
        public void AllVideosOfTheQueueEndUpDownloaded()
        {
            var queue = QueueOfVideosNumbered(1, 7);
            var downloader = CreateYTDownloaderToDownload(queue);

            downloader.StartDownloadingQueue();

            Assert.All(queue, video => Assert.Equal(MetadataStatus.Downloaded, video.Status));
        }

        [Fact]
        public void AsManyInstancesAsThereAreVideosAreCreatedWhenThereAreFewerVideosThanTheParallelLimit()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 5;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 2));

            downloader.StartDownloadingQueue();

            Assert.Equal(2, _fakeThreadStarter.ThreadsStarted);
            var consoleRowsOfTheInstancesThatWereCleanedUp = _consoleRowOverwriter.Overwrites
                .Where(overwrite => overwrite.Message == "download instance not needed. removed...")
                .Select(overwrite => overwrite.ConsoleRow)
                .OrderBy(consoleRow => consoleRow);
            Assert.Equal(new[] { 0, 1 }, consoleRowsOfTheInstancesThatWereCleanedUp);
        }

        [Fact]
        public void AtMostTheParallelLimitOfInstancesAreCreatedOnConsoleRowsZeroToLimitMinusOne()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 3;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 7));
            var threadsStartedAtEachWait = new List<int>();
            _fakeCurrentThreadWaiter.RunWhileWaiting = () =>
            {
                threadsStartedAtEachWait.Add(_fakeThreadStarter.ThreadsStarted);
                _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();
            };

            downloader.StartDownloadingQueue();

            Assert.Equal(3, threadsStartedAtEachWait.First());
            var consoleRowsOfTheInstancesThatWereCleanedUp = _consoleRowOverwriter.Overwrites
                .Where(overwrite => overwrite.Message == "download instance not needed. removed...")
                .Select(overwrite => overwrite.ConsoleRow)
                .OrderBy(consoleRow => consoleRow);
            Assert.Equal(new[] { 0, 1, 2 }, consoleRowsOfTheInstancesThatWereCleanedUp);
        }

        [Fact]
        public void FiveSecondsAreWaitedAfterEachPassOverTheInstances()
        {
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 3));

            downloader.StartDownloadingQueue();

            Assert.Equal(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5) }, _fakeCurrentThreadWaiter.Waits);
        }

        [Fact]
        public void OnceTheQueueIsEmptyEachInstanceLeavesWithItsMessage()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 2;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 2));

            downloader.StartDownloadingQueue();

            Assert.Contains("download instance not needed. removed...", _consoleRowOverwriter.MessagesWrittenOnConsoleRow(0));
            Assert.Contains("download instance not needed. removed...", _consoleRowOverwriter.MessagesWrittenOnConsoleRow(1));
        }

        [Fact]
        public void AnInstanceOlderThanTheStopLimitCountedFromItsCreationIsStoppedAndGivenTheNextVideo()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 1;
            _appConfig.StopDownloadInstanceAfterSeconds = 100;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 2));
            var numberOfWaits = 0;
            _fakeCurrentThreadWaiter.RunWhileWaiting = () =>
            {
                numberOfWaits++;
                if (numberOfWaits == 1)
                {
                    _fakeTimeProvider.Advance(TimeSpan.FromSeconds(200));
                }
                else
                {
                    _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();
                }
            };

            downloader.StartDownloadingQueue();

            Assert.Equal(2, _fakeThreadStarter.ThreadsStarted);
            Assert.True(_fakeThreadStarter.InterruptActionsInvoked >= 1);
            Assert.Contains("https://www.youtube.com/watch?v=video-2", _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.VideoUrl));
        }

        [Fact]
        public void AnInstanceThatIsStillDownloadingAndIsNotOldEnoughIsLeftAlone()
        {
            _appConfig.YoutubeDL.MaxNoOfParallelDownloads = 1;
            _appConfig.StopDownloadInstanceAfterSeconds = 100;
            var downloader = CreateYTDownloaderToDownload(QueueOfVideosNumbered(1, 2));
            var threadsStartedAtEachWait = new List<int>();
            _fakeCurrentThreadWaiter.RunWhileWaiting = () =>
            {
                threadsStartedAtEachWait.Add(_fakeThreadStarter.ThreadsStarted);
                var theInstanceHasBeenLeftAloneForTwoWaits = threadsStartedAtEachWait.Count > 2;
                if (theInstanceHasBeenLeftAloneForTwoWaits)
                {
                    _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();
                }
            };

            downloader.StartDownloadingQueue();

            Assert.Equal(new[] { 1, 1, 1 }, threadsStartedAtEachWait.Take(3));
            Assert.Equal(new[] { "https://www.youtube.com/watch?v=video-1", "https://www.youtube.com/watch?v=video-2" }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.VideoUrl));
        }
    }
}
