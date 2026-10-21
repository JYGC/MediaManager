using System;
using OPMF.Downloader.YTDownloader;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class VideoDownloadAttemptRulesTests
    {
        private class RecordingAttempt
        {
            private readonly VideoDownloadAttemptOutcome _outcomeToReturn;

            public bool Ran { get; private set; }

            public RecordingAttempt(VideoDownloadAttemptOutcome outcomeToReturn)
            {
                _outcomeToReturn = outcomeToReturn;
            }

            public VideoDownloadAttemptOutcome Run()
            {
                Ran = true;
                return _outcomeToReturn;
            }
        }

        [Fact]
        public void TheMessageOfAnAttemptThatCreatedNoFileIsTheLastErrorOutputLine()
        {
            var message = VideoDownloadAttemptRules.CreateMessageOfAttemptThatCreatedNoVideoFile("ERROR: unable to download", "yt-dlp", "DownloadFolder/A title.mp4");

            Assert.Equal("ERROR: unable to download", message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithNoErrorOutputTheMessageNamesTheExecutableAndTheOutputPath(string lastErrorOutputLine)
        {
            var message = VideoDownloadAttemptRules.CreateMessageOfAttemptThatCreatedNoVideoFile(lastErrorOutputLine, "yt-dlp", "DownloadFolder/A title.mp4");

            Assert.Equal("yt-dlp finished without creating DownloadFolder/A title.mp4", message);
        }

        [Fact]
        public void WhenTheFirstAttemptSucceedsTheFallbackDoesNotRun()
        {
            var attemptWithExecutableTriedFirst = new RecordingAttempt(VideoDownloadAttemptOutcome.Succeeded);
            var attemptWithFallbackExecutable = new RecordingAttempt(VideoDownloadAttemptOutcome.Succeeded);

            var outcome = VideoDownloadAttemptRules.RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(attemptWithExecutableTriedFirst.Run, attemptWithFallbackExecutable.Run);

            Assert.Equal(VideoDownloadAttemptOutcome.Succeeded, outcome);
            Assert.True(attemptWithExecutableTriedFirst.Ran);
            Assert.False(attemptWithFallbackExecutable.Ran);
        }

        [Theory]
        [InlineData(VideoDownloadAttemptOutcome.Succeeded)]
        [InlineData(VideoDownloadAttemptOutcome.Failed)]
        [InlineData(VideoDownloadAttemptOutcome.StoppedBecauseDownloadInstanceTimedOut)]
        public void WhenTheFirstAttemptFailsAndAFallbackIsAvailableTheOutcomeIsThatOfTheFallbackAttempt(VideoDownloadAttemptOutcome outcomeOfTheFallbackAttempt)
        {
            var attemptWithExecutableTriedFirst = new RecordingAttempt(VideoDownloadAttemptOutcome.Failed);
            var attemptWithFallbackExecutable = new RecordingAttempt(outcomeOfTheFallbackAttempt);

            var outcome = VideoDownloadAttemptRules.RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(attemptWithExecutableTriedFirst.Run, attemptWithFallbackExecutable.Run);

            Assert.Equal(outcomeOfTheFallbackAttempt, outcome);
            Assert.True(attemptWithFallbackExecutable.Ran);
        }

        [Fact]
        public void WhenTheFirstAttemptFailsAndThereIsNoFallbackTheOutcomeIsFailed()
        {
            var attemptWithExecutableTriedFirst = new RecordingAttempt(VideoDownloadAttemptOutcome.Failed);

            var outcome = VideoDownloadAttemptRules.RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(attemptWithExecutableTriedFirst.Run, null);

            Assert.Equal(VideoDownloadAttemptOutcome.Failed, outcome);
            Assert.True(attemptWithExecutableTriedFirst.Ran);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhenTheFirstAttemptIsStoppedTheFallbackDoesNotRunWhetherOrNotItIsAvailable(bool fallbackIsAvailable)
        {
            var attemptWithExecutableTriedFirst = new RecordingAttempt(VideoDownloadAttemptOutcome.StoppedBecauseDownloadInstanceTimedOut);
            var attemptWithFallbackExecutable = new RecordingAttempt(VideoDownloadAttemptOutcome.Succeeded);
            Func<VideoDownloadAttemptOutcome> fallbackAttemptOrNull = fallbackIsAvailable ? attemptWithFallbackExecutable.Run : null;

            var outcome = VideoDownloadAttemptRules.RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(attemptWithExecutableTriedFirst.Run, fallbackAttemptOrNull);

            Assert.Equal(VideoDownloadAttemptOutcome.StoppedBecauseDownloadInstanceTimedOut, outcome);
            Assert.False(attemptWithFallbackExecutable.Ran);
        }
    }
}
