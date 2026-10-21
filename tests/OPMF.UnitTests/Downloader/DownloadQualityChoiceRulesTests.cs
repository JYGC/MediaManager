using OPMF.Downloader.YTDownloader;
using OPMF.Entities;
using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class DownloadQualityChoiceRulesTests
    {
        private static readonly YoutubeDLConfig ConfigWithNonDefaultChoices = new YoutubeDLConfig
        {
            VideoCodecToPreferWhenDownloading = VideoCodecChoice.AnyCodec,
            MaximumVideoResolutionOnTheShorterSide = MaximumVideoResolutionChoice.UpTo1440p,
            AudioQualityToDownload = AudioQualityChoice.BestOfAnyCodec,
        };

        [Fact]
        public void AVideoWithNoOverridesGetsTheThreeChoicesOfTheConfigFile()
        {
            var choices = DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(new YoutubeMetadata(), ConfigWithNonDefaultChoices);

            Assert.Equal(new DownloadQualityChoices(VideoCodecChoice.AnyCodec, MaximumVideoResolutionChoice.UpTo1440p, AudioQualityChoice.BestOfAnyCodec), choices);
        }

        [Theory]
        [InlineData(EntityVideoCodecChoice.H264, VideoCodecChoice.H264)]
        [InlineData(EntityVideoCodecChoice.AnyCodec, VideoCodecChoice.AnyCodec)]
        public void TheVideoCodecOverrideWinsOverTheConfigFile(EntityVideoCodecChoice overrideOfTheVideo, VideoCodecChoice expectedChoice)
        {
            var config = new YoutubeDLConfig { VideoCodecToPreferWhenDownloading = expectedChoice == VideoCodecChoice.H264 ? VideoCodecChoice.AnyCodec : VideoCodecChoice.H264 };

            var choices = DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(new YoutubeMetadata { VideoCodecChoiceOverride = overrideOfTheVideo }, config);

            Assert.Equal(expectedChoice, choices.VideoCodec);
        }

        [Theory]
        [InlineData(EntityMaximumVideoResolutionChoice.NoLimit, MaximumVideoResolutionChoice.NoLimit)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo2160p, MaximumVideoResolutionChoice.UpTo2160p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo1440p, MaximumVideoResolutionChoice.UpTo1440p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo1080p, MaximumVideoResolutionChoice.UpTo1080p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo720p, MaximumVideoResolutionChoice.UpTo720p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo480p, MaximumVideoResolutionChoice.UpTo480p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo360p, MaximumVideoResolutionChoice.UpTo360p)]
        public void TheMaximumVideoResolutionOverrideWinsOverTheConfigFile(EntityMaximumVideoResolutionChoice overrideOfTheVideo, MaximumVideoResolutionChoice expectedChoice)
        {
            var config = new YoutubeDLConfig { MaximumVideoResolutionOnTheShorterSide = expectedChoice == MaximumVideoResolutionChoice.UpTo480p ? MaximumVideoResolutionChoice.UpTo720p : MaximumVideoResolutionChoice.UpTo480p };

            var choices = DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(new YoutubeMetadata { MaximumVideoResolutionChoiceOverride = overrideOfTheVideo }, config);

            Assert.Equal(expectedChoice, choices.MaximumVideoResolution);
        }

        [Theory]
        [InlineData(EntityAudioQualityChoice.BestAac, AudioQualityChoice.BestAac)]
        [InlineData(EntityAudioQualityChoice.BestOfAnyCodec, AudioQualityChoice.BestOfAnyCodec)]
        [InlineData(EntityAudioQualityChoice.SmallestAac, AudioQualityChoice.SmallestAac)]
        public void TheAudioQualityOverrideWinsOverTheConfigFile(EntityAudioQualityChoice overrideOfTheVideo, AudioQualityChoice expectedChoice)
        {
            var config = new YoutubeDLConfig { AudioQualityToDownload = expectedChoice == AudioQualityChoice.BestAac ? AudioQualityChoice.SmallestAac : AudioQualityChoice.BestAac };

            var choices = DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(new YoutubeMetadata { AudioQualityChoiceOverride = overrideOfTheVideo }, config);

            Assert.Equal(expectedChoice, choices.AudioQuality);
        }

        [Fact]
        public void EachChoiceIsOverriddenIndependentlyOfTheOthers()
        {
            var video = new YoutubeMetadata { MaximumVideoResolutionChoiceOverride = EntityMaximumVideoResolutionChoice.UpTo720p };

            var choices = DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(video, ConfigWithNonDefaultChoices);

            Assert.Equal(new DownloadQualityChoices(VideoCodecChoice.AnyCodec, MaximumVideoResolutionChoice.UpTo720p, AudioQualityChoice.BestOfAnyCodec), choices);
        }
    }
}
