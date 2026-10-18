using System;
using OPMF.Downloader.YTDownloader;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class VideoDownloaderExecutableOrderAndLocationRulesTests
    {
        private static readonly VideoDownloaderExecutable ANumberWithNoExecutable = (VideoDownloaderExecutable)99;
        private readonly TemporaryFolderReadonlySettings _readonlySettings = new TemporaryFolderReadonlySettings(System.IO.Path.GetTempPath());

        [Fact]
        public void TheFallbackOfYoutubeDlIsYtDlp()
        {
            Assert.Equal(VideoDownloaderExecutable.YtDlp, VideoDownloaderExecutableOrderAndLocationRules.GetFallbackExecutable(VideoDownloaderExecutable.YoutubeDl));
        }

        [Fact]
        public void TheFallbackOfYtDlpIsYoutubeDl()
        {
            Assert.Equal(VideoDownloaderExecutable.YoutubeDl, VideoDownloaderExecutableOrderAndLocationRules.GetFallbackExecutable(VideoDownloaderExecutable.YtDlp));
        }

        [Fact]
        public void TheFallbackOfANumberWithNoExecutableThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VideoDownloaderExecutableOrderAndLocationRules.GetFallbackExecutable(ANumberWithNoExecutable));
        }

        [Fact]
        public void TheLocationOfYoutubeDlHasItsNameItsBinPathAndTheGithubReleaseUrl()
        {
            var location = VideoDownloaderExecutableOrderAndLocationRules.GetLocationOfExecutable(VideoDownloaderExecutable.YoutubeDl, _readonlySettings);

            Assert.Equal("youtube-dl", location.ExecutableName);
            Assert.Equal(_readonlySettings.GetYoutubeDLPath(), location.ExecutableFilePath);
            Assert.Equal("https://github.com/ytdl-org/youtube-dl/releases/latest/download/youtube-dl.exe", location.DownloadUrlUsedWhenExecutableIsMissing);
        }

        [Fact]
        public void TheLocationOfYtDlpHasItsNameItsBinPathAndTheGithubReleaseUrl()
        {
            var location = VideoDownloaderExecutableOrderAndLocationRules.GetLocationOfExecutable(VideoDownloaderExecutable.YtDlp, _readonlySettings);

            Assert.Equal("yt-dlp", location.ExecutableName);
            Assert.Equal(_readonlySettings.GetYtDlpExecutablePath(), location.ExecutableFilePath);
            Assert.Equal("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", location.DownloadUrlUsedWhenExecutableIsMissing);
        }

        [Fact]
        public void TheLocationOfANumberWithNoExecutableThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VideoDownloaderExecutableOrderAndLocationRules.GetLocationOfExecutable(ANumberWithNoExecutable, _readonlySettings));
        }
    }
}
