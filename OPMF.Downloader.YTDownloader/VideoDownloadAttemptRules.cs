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
        public static string CreateMessageOfAttemptThatCreatedNoVideoFile(string lastErrorOutputLineOrNull, string executableName, string outputFilePath)
        {
            return string.IsNullOrWhiteSpace(lastErrorOutputLineOrNull)
                ? executableName + " finished without creating " + outputFilePath
                : lastErrorOutputLineOrNull;
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
