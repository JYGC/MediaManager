using System;

namespace OPMF.Downloader.YTDownloader
{
    public static class MergeContainerRules
    {
        public static string GetMergeContainerOfVideoExtensionOrThrow(string videoExtension)
        {
            if (videoExtension == "mp4" || videoExtension == "mkv")
            {
                return videoExtension;
            }
            throw new InvalidOperationException(
                "VideoExtension in config.json is \"" + videoExtension + "\", but downloads can only be merged into mp4 or mkv. Set VideoExtension under YoutubeDL to mp4 or mkv.");
        }
    }
}
