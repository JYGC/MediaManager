using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class DownloadQualitySettingsTests
    {
        [Fact]
        public void TheDefaultVideoCodecToPreferWhenDownloadingIsH264()
        {
            Assert.Equal(VideoCodecChoice.H264, new YoutubeDLConfig().VideoCodecToPreferWhenDownloading);
        }

        [Fact]
        public void TheDefaultMaximumVideoResolutionOnTheShorterSideIsNoLimit()
        {
            Assert.Equal(MaximumVideoResolutionChoice.NoLimit, new YoutubeDLConfig().MaximumVideoResolutionOnTheShorterSide);
        }

        [Fact]
        public void TheDefaultAudioQualityToDownloadIsBestAac()
        {
            Assert.Equal(AudioQualityChoice.BestAac, new YoutubeDLConfig().AudioQualityToDownload);
        }
    }
}
