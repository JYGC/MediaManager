using System;
using System.IO;
using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class AppConfigTests
    {
        [Fact]
        public void Default_NewChannelPastVideoDayLimit_Is1()
        {
            Assert.Equal(1, new AppConfig().NewChannelPastVideoDayLimit);
        }

        [Fact]
        public void Default_StopDownloadInstanceAfterSeconds_Is12Hours()
        {
            Assert.Equal(60 * 60 * 12, new AppConfig().StopDownloadInstanceAfterSeconds);
        }

        [Fact]
        public void Default_GoogleClientSecretPath_IsNonEmpty()
        {
            Assert.False(string.IsNullOrWhiteSpace(new AppConfig().GoogleClientSecretPath));
        }

        [Fact]
        public void Default_VideoOutputFolderPath_PointsUnderDesktop()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var expected = Path.Join(desktop, "OffPeakVideos");
            Assert.Equal(expected, new AppConfig().VideoOutputFolderPath);
        }

        [Fact]
        public void Default_YoutubeDL_IsInitialized()
        {
            Assert.NotNull(new AppConfig().YoutubeDL);
        }
    }

    public class YoutubeDLConfigTests
    {
        [Fact]
        public void Defaults_AreSetAsExpected()
        {
            var cfg = new YoutubeDLConfig();
            Assert.True(cfg.CheckForBinaryUpdates);
            Assert.False(cfg.GetSubtitles);
            Assert.Equal("mp4", cfg.VideoExtension);
            Assert.Equal("vtt", cfg.SubtitleExtension);
            Assert.Equal(5, cfg.MaxNoOfParallelDownloads);
        }

        [Fact]
        public void Default_FalseErrorMessages_ContainsKnownPreMergedWarning()
        {
            var cfg = new YoutubeDLConfig();
            Assert.NotNull(cfg.FalseErrorMessages);
            Assert.Contains(cfg.FalseErrorMessages, m => m.Contains("pre-merged"));
        }
    }
}
