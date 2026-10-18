using System;
using OPMF.Downloader.YTDownloader;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class VideoDownloadAttemptRulesTests
    {
        private static readonly string[] ErrorLinesThatAreNotFailures = { "WARNING: the best pre-merged format" };

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
        public void NoErrorOutputIsASuccess()
        {
            Assert.True(VideoDownloadAttemptRules.DidDownloadAttemptSucceed(null, ErrorLinesThatAreNotFailures));
        }

        [Fact]
        public void EmptyErrorOutputIsASuccess()
        {
            Assert.True(VideoDownloadAttemptRules.DidDownloadAttemptSucceed("", ErrorLinesThatAreNotFailures));
        }

        [Fact]
        public void ALineListedAsNotAFailureIsASuccessAfterTrimming()
        {
            Assert.True(VideoDownloadAttemptRules.DidDownloadAttemptSucceed("  WARNING: the best pre-merged format\r\n", ErrorLinesThatAreNotFailures));
        }

        [Fact]
        public void AnyOtherErrorLineIsAFailure()
        {
            Assert.False(VideoDownloadAttemptRules.DidDownloadAttemptSucceed("ERROR: unable to download", ErrorLinesThatAreNotFailures));
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
