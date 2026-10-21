using System;
using OPMF.Settings;

namespace OPMF.Downloader.YTDownloader
{
    public static class VideoDownloaderExecutableOrderAndLocationRules
    {
        public static VideoDownloaderExecutable GetFallbackExecutable(VideoDownloaderExecutable executableTriedFirst)
        {
            switch (executableTriedFirst)
            {
                case VideoDownloaderExecutable.YoutubeDl:
                    return VideoDownloaderExecutable.YtDlp;
                case VideoDownloaderExecutable.YtDlp:
                    return VideoDownloaderExecutable.YoutubeDl;
                default:
                    throw new ArgumentOutOfRangeException(nameof(executableTriedFirst), executableTriedFirst, "No video downloader executable has this value.");
            }
        }

        public static VideoDownloaderExecutableLocation GetLocationOfExecutable(VideoDownloaderExecutable executable, IReadonlySettings readonlySettings)
        {
            switch (executable)
            {
                case VideoDownloaderExecutable.YoutubeDl:
                    return new VideoDownloaderExecutableLocation(
                        "youtube-dl",
                        readonlySettings.GetYoutubeDLPath(),
                        "https://github.com/ytdl-org/youtube-dl/releases/latest/download/youtube-dl.exe");
                case VideoDownloaderExecutable.YtDlp:
                    return new VideoDownloaderExecutableLocation(
                        "yt-dlp",
                        readonlySettings.GetYtDlpExecutablePath(),
                        "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe");
                default:
                    throw new ArgumentOutOfRangeException(nameof(executable), executable, "No video downloader executable has this value.");
            }
        }
    }
}
