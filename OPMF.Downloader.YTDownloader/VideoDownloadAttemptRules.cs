using System;
using System.Collections.Generic;
using System.Linq;

namespace OPMF.Downloader.YTDownloader
{
    public enum VideoDownloadAttemptOutcome
    {
        Succeeded,
        Failed,
        StoppedBecauseDownloadInstanceTimedOut,
    }

    public static class VideoDownloadAttemptRules
    {
        public static bool DidDownloadAttemptSucceed(string lastErrorOutputLineOfAttempt, IEnumerable<string> errorOutputLinesThatAreNotFailures)
        {
            return string.IsNullOrEmpty(lastErrorOutputLineOfAttempt) ||
                errorOutputLinesThatAreNotFailures.Contains(lastErrorOutputLineOfAttempt.Trim());
        }

        public static VideoDownloadAttemptOutcome RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(
            Func<VideoDownloadAttemptOutcome> attemptWithExecutableTriedFirst,
            Func<VideoDownloadAttemptOutcome> attemptWithFallbackExecutable)
        {
            VideoDownloadAttemptOutcome outcomeOfAttemptWithExecutableTriedFirst = attemptWithExecutableTriedFirst();
            bool shouldTryFallbackExecutable = outcomeOfAttemptWithExecutableTriedFirst == VideoDownloadAttemptOutcome.Failed && attemptWithFallbackExecutable != null;
            return shouldTryFallbackExecutable ? attemptWithFallbackExecutable() : outcomeOfAttemptWithExecutableTriedFirst;
        }
    }
}
