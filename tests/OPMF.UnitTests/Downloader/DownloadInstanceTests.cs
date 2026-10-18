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
    public class DownloadInstanceTests : IDisposable
    {
        private const int ConsoleRowOfTheInstance = 3;
        private const string DownloadFolderPath = "DownloadFolder";
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private readonly FakeVideoDownloaderExecutableRunner _fakeVideoDownloaderExecutableRunner = new FakeVideoDownloaderExecutableRunner();
        private readonly FakeInterruptibleDownloadThreadStarter _fakeThreadStarter = new FakeInterruptibleDownloadThreadStarter();
        private readonly RecordingConsoleRowOverwriter _consoleRowOverwriter = new RecordingConsoleRowOverwriter();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly YoutubeDLConfig _youtubeDLConfig = new YoutubeDLConfig();
        private readonly VideoDownloaderExecutableLocation _youtubeDlLocation = new VideoDownloaderExecutableLocation("youtube-dl", "Bin/youtube-dl.exe", "https://example/youtube-dl.exe");
        private readonly DownloadInstance _downloadInstance;

        public DownloadInstanceTests()
        {
            _downloadInstance = new DownloadInstance(
                ConsoleRowOfTheInstance,
                _youtubeDlLocation,
                null,
                _fakeVideoDownloaderExecutableRunner,
                _youtubeDLConfig,
                DownloadFolderPath,
                _fakeThreadStarter,
                _consoleRowOverwriter,
                _fakeTimeProvider,
                new Logger(_databaseConnectionOpener));
        }

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private DateTime ClockNow => _fakeTimeProvider.GetLocalNow().LocalDateTime;

        private static Metadata VideoTitled(string title) => new YoutubeMetadata { SiteId = "video-1", Url = "https://www.youtube.com/watch?v=video-1", Title = title };

        private void DownloadAndRunTheDownloadThread(Metadata video)
        {
            _downloadInstance.Download(video);
            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();
        }

        private OPMFLog TheOnlyLoggedEntry()
        {
            OPMFLog loggedEntry = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => loggedEntry = databaseAdapter.OPMFLogDbCollection.GetAll().Single());
            return loggedEntry;
        }

        [Fact]
        public void TheInstanceKnowsItsConsoleRowAndWhenItWasCreated()
        {
            Assert.Equal(ConsoleRowOfTheInstance, _downloadInstance.ScreenPosition);
            Assert.Equal(ClockNow, _downloadInstance.StartDateTime);
        }

        [Fact]
        public void TheOutputPathIsTheDownloadFolderThePlainTitleAndTheConfiguredVideoExtension()
        {
            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            var run = Assert.Single(_fakeVideoDownloaderExecutableRunner.Runs);
            Assert.Equal(System.IO.Path.Join(DownloadFolderPath, "A plain title.mp4"), run.OutputFilePath);
        }

        [Fact]
        public void TheOutputPathReplacesForwardSlashesQuotesAndPercentSignsInTheTitle()
        {
            DownloadAndRunTheDownloadThread(VideoTitled("a/b \"quoted\" 100%"));

            var run = Assert.Single(_fakeVideoDownloaderExecutableRunner.Runs);
            Assert.Equal(System.IO.Path.Join(DownloadFolderPath, "aforward slashb ''quoted'' 100percent.mp4"), run.OutputFilePath);
        }

        [Fact]
        public void TheExecutableIsRunWithTheExecutablesFilePathAndTheVideoUrl()
        {
            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            var run = Assert.Single(_fakeVideoDownloaderExecutableRunner.Runs);
            Assert.Equal("Bin/youtube-dl.exe", run.ExecutableFilePath);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", run.VideoUrl);
        }

        [Fact]
        public void DownloadProgressLinesGetTheTitleCutToFortyCharactersAppended()
        {
            var titleOfFortyFiveCharacters = "0123456789012345678901234567890123456789ABCDE";
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(standardOutputLines: new[] { "[download]  42.0% of 10MiB" });

            DownloadAndRunTheDownloadThread(VideoTitled(titleOfFortyFiveCharacters));

            Assert.Equal(
                new[] { "[download]  42.0% of 10MiB Video: 0123456789012345678901234567890123456789..." },
                _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }

        [Fact]
        public void ATitleShorterThanFortyCharactersIsNotCut()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(standardOutputLines: new[] { "[download] 100%" });

            DownloadAndRunTheDownloadThread(VideoTitled("Short title"));

            Assert.Equal(new[] { "[download] 100% Video: Short title..." }, _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }

        [Fact]
        public void OtherOutputLinesAreWrittenOnTheInstancesConsoleRowAsTheyAre()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(standardOutputLines: new[] { "[youtube] Extracting URL" });

            DownloadAndRunTheDownloadThread(VideoTitled("Short title"));

            Assert.Equal(new[] { "[youtube] Extracting URL" }, _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }

        [Fact]
        public void NoErrorOutputMarksTheVideoAsDownloaded()
        {
            var video = VideoTitled("A plain title");

            DownloadAndRunTheDownloadThread(video);

            Assert.Equal(MetadataStatus.Downloaded, video.Status);
        }

        [Fact]
        public void AnEmptyErrorOutputMarksTheVideoAsDownloaded()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "");
            var video = VideoTitled("A plain title");

            DownloadAndRunTheDownloadThread(video);

            Assert.Equal(MetadataStatus.Downloaded, video.Status);
        }

        [Fact]
        public void AnErrorOutputThatIsListedAsNotAFailureMarksTheVideoAsDownloadedAfterTrimming()
        {
            var errorLineThatIsNotAFailure = _youtubeDLConfig.FalseErrorMessages[0];
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "  " + errorLineThatIsNotAFailure + "\n");
            var video = VideoTitled("A plain title");

            DownloadAndRunTheDownloadThread(video);

            Assert.Equal(MetadataStatus.Downloaded, video.Status);
        }

        [Fact]
        public void AnotherErrorOutputLeavesTheVideoNotDownloaded()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "ERROR: unable to download");
            var video = VideoTitled("A plain title");

            DownloadAndRunTheDownloadThread(video);

            Assert.Equal(MetadataStatus.New, video.Status);
        }

        [Fact]
        public void AnErrorOutputLogsAWarningWithTheTitleAndUrlOfTheVideo()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "ERROR: unable to download");

            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            var loggedEntry = TheOnlyLoggedEntry();
            Assert.Equal(OPMFLogType.Warning, loggedEntry.Type);
            Assert.Equal("ERROR: unable to download", loggedEntry.Message);
            Assert.Equal("A plain title", loggedEntry.Variables["metadata.Title"]);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", loggedEntry.Variables["metadata.Url"]);
            Assert.Equal(ClockNow, loggedEntry.DateCreated);
        }

        [Fact]
        public void AnErrorOutputWritesAnErrorMessageOnTheInstancesConsoleRow()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(lastErrorOutputLine: "ERROR: unable to download");

            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            Assert.Equal(new[] { "An error has occured. See error log..." }, _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }

        [Fact]
        public void AnExceptionFromTheExecutableLogsAnErrorWithTheTitleAndUrlOfTheVideo()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(exceptionToThrow: new InvalidOperationException("the executable could not start"));

            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            var loggedEntry = TheOnlyLoggedEntry();
            Assert.IsType<OPMFError>(loggedEntry);
            Assert.Equal(OPMFLogType.Error, loggedEntry.Type);
            Assert.Equal("the executable could not start", loggedEntry.Message);
            Assert.Contains("InvalidOperationException", loggedEntry.ExceptionObject);
            Assert.Equal("A plain title", loggedEntry.Variables["metadata.Title"]);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", loggedEntry.Variables["metadata.Url"]);
            Assert.Equal(ClockNow, loggedEntry.DateCreated);
        }

        [Fact]
        public void AnExceptionFromTheExecutableLeavesTheVideoNotDownloaded()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(exceptionToThrow: new InvalidOperationException("the executable could not start"));
            var video = VideoTitled("A plain title");

            DownloadAndRunTheDownloadThread(video);

            Assert.Equal(MetadataStatus.New, video.Status);
        }

        [Fact]
        public void TheInstanceIsDownloadingFromTheStartOfDownloadUntilTheDownloadThreadEnds()
        {
            Assert.True(_downloadInstance.NotDownloading);

            _downloadInstance.Download(VideoTitled("A plain title"));
            Assert.False(_downloadInstance.NotDownloading);

            _fakeThreadStarter.RunEveryThreadBodyThatHasNotRunYet();
            Assert.True(_downloadInstance.NotDownloading);
        }

        [Fact]
        public void TheInstanceIsNotDownloadingAgainAfterTheExecutableThrew()
        {
            _fakeVideoDownloaderExecutableRunner.ScriptNextRun(exceptionToThrow: new InvalidOperationException("the executable could not start"));

            DownloadAndRunTheDownloadThread(VideoTitled("A plain title"));

            Assert.True(_downloadInstance.NotDownloading);
        }

        [Fact]
        public void StopDownloadThreadInterruptsTheThreadThatWasStarted()
        {
            _downloadInstance.Download(VideoTitled("A plain title"));

            _downloadInstance.StopDownloadThread();

            Assert.Equal(1, _fakeThreadStarter.InterruptActionsInvoked);
        }

        [Fact]
        public void StopDownloadThreadDoesNothingWhenNoThreadWasStarted()
        {
            _downloadInstance.StopDownloadThread();

            Assert.Equal(0, _fakeThreadStarter.InterruptActionsInvoked);
        }

        [Fact]
        public void CleanUpInstanceWritesItsMessageOnTheInstancesConsoleRow()
        {
            _downloadInstance.CleanUpInstance();

            Assert.Equal(new[] { "download instance not needed. removed..." }, _consoleRowOverwriter.MessagesWrittenOnConsoleRow(ConsoleRowOfTheInstance));
        }
    }
}
