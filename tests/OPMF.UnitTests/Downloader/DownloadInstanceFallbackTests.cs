using System;
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
    public class DownloadInstanceFallbackTests : IDisposable
    {
        private const int ConsoleRowOfTheInstance = 2;
        private const string ErrorLineThatIsAFailure = "ERROR: unable to download";
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private readonly FakeVideoDownloaderExecutableRunner _fakeVideoDownloaderExecutableRunner = new FakeVideoDownloaderExecutableRunner();
        private readonly FakeInterruptibleDownloadThreadStarter _fakeThreadStarter = new FakeInterruptibleDownloadThreadStarter();
        private readonly RecordingConsoleRowOverwriter _consoleRowOverwriter = new RecordingConsoleRowOverwriter();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly VideoDownloaderExecutableLocation _youtubeDlLocation = new VideoDownloaderExecutableLocation("youtube-dl", "Bin/youtube-dl.exe", "https://example/youtube-dl.exe");
        private readonly VideoDownloaderExecutableLocation _ytDlpLocation = new VideoDownloaderExecutableLocation("yt-dlp", "Bin/yt-dlp.exe", "https://example/yt-dlp.exe");

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private DownloadInstance CreateDownloadInstanceWithFallback(VideoDownloaderExecutableLocation fallbackExecutable)
        {
            return new DownloadInstance(
                ConsoleRowOfTheInstance,
                _youtubeDlLocation,
                fallbackExecutable,
                _fakeVideoDownloaderExecutableRunner,
                new YoutubeDLConfig(),
                "DownloadFolder",
                _fakeThreadStarter,
                _consoleRowOverwriter,
                _fakeTimeProvider,
                new Logger(_databaseConnectionOpener));
        }

        private static Metadata VideoNumbered(int number) =>
            new YoutubeMetadata { SiteId = "video-" + number, Url = "https://www.youtube.com/watch?v=video-" + number, Title = "Video " + number };

        private void ScriptFailureOfTheNextRun() => _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: ErrorLineThatIsAFailure);

        private System.Collections.Generic.List<OPMFLog> AllLoggedEntries()
        {
            System.Collections.Generic.List<OPMFLog> loggedEntries = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => loggedEntries = databaseAdapter.OPMFLogDbCollection.GetAll().ToList());
            return loggedEntries;
        }

        [Fact]
        public void AFailedFirstAttemptRunsTheFallbackExecutableOnTheSameVideo()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(
                new[] { ("Bin/youtube-dl.exe", "https://www.youtube.com/watch?v=video-1"), ("Bin/yt-dlp.exe", "https://www.youtube.com/watch?v=video-1") },
                _fakeVideoDownloaderExecutableRunner.Runs.Select(run => (run.ExecutableFilePath, run.VideoUrl)));
        }

        [Fact]
        public void TheFallbackAttemptWritesToTheSameOutputPathAsTheFirstAttempt()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Single(_fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.OutputFilePath).Distinct());
        }

        [Fact]
        public void OnlyTheFallbackAttemptTellsTheExecutableNotToResumePartialFiles()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(new[] { false, true }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.TellExecutableNotToResumePartialFiles));
        }

        [Fact]
        public void WhenTheFallbackSucceedsTheVideoIsMarkedAsDownloaded()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();
            var video = VideoNumbered(1);

            downloadInstance.Download(video);
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(MetadataStatus.Downloaded, video.Status);
        }

        [Fact]
        public void WhenBothAttemptsFailTheVideoIsNotMarkedAsDownloaded()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();
            ScriptFailureOfTheNextRun();
            var video = VideoNumbered(1);

            downloadInstance.Download(video);
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(MetadataStatus.New, video.Status);
        }

        [Fact]
        public void WhenTheFirstAttemptSucceedsTheFallbackDoesNotRun()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(new[] { "Bin/youtube-dl.exe" }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void WithNoFallbackAFailedFirstAttemptIsNotRetried()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(null);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(new[] { "Bin/youtube-dl.exe" }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void AFirstAttemptThatThrowsAlsoRunsTheFallback()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(exceptionToThrow: new InvalidOperationException("the executable could not start"));

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(new[] { "Bin/youtube-dl.exe", "Bin/yt-dlp.exe" }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void TheInstanceIsNotDownloadingAgainOnceBothAttemptsAreDone()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            Assert.False(downloadInstance.NotDownloading);
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.True(downloadInstance.NotDownloading);
        }

        [Fact]
        public void EachFailedAttemptIsLoggedWithTheNameOfTheExecutableThatFailed()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            var executableNamesInTheLoggedEntries = AllLoggedEntries().Select(entry => (string)entry.Variables["executableName"]).OrderBy(name => name);
            Assert.Equal(new[] { "youtube-dl", "yt-dlp" }, executableNamesInTheLoggedEntries);
        }

        [Fact]
        public void AnAttemptThatThrowsIsLoggedAsAnErrorWithTheNameOfTheExecutable()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(null);
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(exceptionToThrow: new InvalidOperationException("the executable could not start"));

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            var loggedEntry = Assert.Single(AllLoggedEntries());
            Assert.IsType<OPMFError>(loggedEntry);
            Assert.Equal("youtube-dl", loggedEntry.Variables["executableName"]);
            Assert.Equal("Video 1", loggedEntry.Variables["metadata.Title"]);
        }

        [Fact]
        public void TheConsoleRowSaysTheFirstExecutableFailedAndTheFallbackIsBeingTried()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();

            downloadInstance.Download(VideoNumbered(1));
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Contains("youtube-dl failed, trying yt-dlp", _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }

        [Fact]
        public void NoFallbackRunsAfterTheInstanceWasStopped()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();
            downloadInstance.Download(VideoNumbered(1));

            downloadInstance.StopDownloadThread();
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(new[] { "Bin/youtube-dl.exe" }, _fakeVideoDownloaderExecutableRunner.Runs.Select(run => run.ExecutableFilePath));
        }

        [Fact]
        public void AStoppedAttemptDoesNotMarkTheVideoAsDownloaded()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            var video = VideoNumbered(1);
            downloadInstance.Download(video);

            downloadInstance.StopDownloadThread();
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            Assert.Equal(MetadataStatus.New, video.Status);
        }

        [Fact]
        public void TheOldThreadStillSkipsItsFallbackAfterTheInstanceMovedOnToTheNextVideo()
        {
            var downloadInstance = CreateDownloadInstanceWithFallback(_ytDlpLocation);
            ScriptFailureOfTheNextRun();
            ScriptFailureOfTheNextRun();
            downloadInstance.Download(VideoNumbered(1));
            downloadInstance.StopDownloadThread();
            downloadInstance.Download(VideoNumbered(2));

            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();

            var fallbackRuns = _fakeVideoDownloaderExecutableRunner.Runs.Where(run => run.TellExecutableNotToResumePartialFiles).ToList();
            Assert.Equal("https://www.youtube.com/watch?v=video-2", Assert.Single(fallbackRuns).VideoUrl);
        }
    }
}
