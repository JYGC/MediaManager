using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPMF.Settings;
using Xunit;

namespace OPMF.ContractTests
{
    public class ConfigFileContractTests
    {
        private static List<string> NamesOfTheKeysOf(JObject jsonObject)
        {
            return jsonObject.Properties().Select(property => property.Name).OrderBy(name => name, System.StringComparer.Ordinal).ToList();
        }

        [Fact]
        public void ANewAppConfigIsWrittenWithExactlyTheTopLevelKeysOfTheConfigFile()
        {
            var configFileJson = JObject.Parse(JsonConvert.SerializeObject(new AppConfig(), Formatting.Indented));

            Assert.Equal(
                new[] { "GoogleClientSecretPath", "NewChannelPastVideoDayLimit", "StopDownloadInstanceAfterSeconds", "VideoOutputFolderPath", "YoutubeDL" },
                NamesOfTheKeysOf(configFileJson));
        }

        [Fact]
        public void TheYoutubeDLObjectIsWrittenWithExactlyTheKeysOfTheConfigFileIncludingTheExecutableTriedFirst()
        {
            var youtubeDLJson = (JObject)JObject.Parse(JsonConvert.SerializeObject(new AppConfig(), Formatting.Indented))["YoutubeDL"];

            Assert.Equal(
                new[]
                {
                    "AudioQualityToDownload",
                    "CheckForBinaryUpdates",
                    "ExecutableTriedFirstToDownloadVideos",
                    "FalseErrorMessages",
                    "GetSubtitles",
                    "MaxNoOfParallelDownloads",
                    "MaximumVideoResolutionOnTheShorterSide",
                    "SubtitleExtension",
                    "VideoCodecToPreferWhenDownloading",
                    "VideoExtension",
                },
                NamesOfTheKeysOf(youtubeDLJson));
        }

        [Theory]
        [InlineData(VideoDownloaderExecutable.YoutubeDl, "youtube-dl")]
        [InlineData(VideoDownloaderExecutable.YtDlp, "yt-dlp")]
        public void TheExecutableTriedFirstIsWrittenAsTheTextSpelledLikeTheExecutable(VideoDownloaderExecutable executable, string expectedText)
        {
            var youtubeDLConfig = new YoutubeDLConfig { ExecutableTriedFirstToDownloadVideos = executable };

            var youtubeDLJson = JObject.Parse(JsonConvert.SerializeObject(youtubeDLConfig));

            Assert.Equal(expectedText, (string)youtubeDLJson["ExecutableTriedFirstToDownloadVideos"]);
        }

        [Theory]
        [InlineData("youtube-dl", VideoDownloaderExecutable.YoutubeDl)]
        [InlineData("yt-dlp", VideoDownloaderExecutable.YtDlp)]
        public void TheExecutableTriedFirstIsReadFromTheTextSpelledLikeTheExecutable(string text, VideoDownloaderExecutable expectedExecutable)
        {
            var youtubeDLConfig = JsonConvert.DeserializeObject<YoutubeDLConfig>("{ \"ExecutableTriedFirstToDownloadVideos\": \"" + text + "\" }");

            Assert.Equal(expectedExecutable, youtubeDLConfig.ExecutableTriedFirstToDownloadVideos);
        }

        [Theory]
        [InlineData(VideoCodecChoice.H264, "h264")]
        [InlineData(VideoCodecChoice.AnyCodec, "any")]
        public void TheVideoCodecToPreferIsWrittenAsItsText(VideoCodecChoice choice, string expectedText)
        {
            var youtubeDLJson = JObject.Parse(JsonConvert.SerializeObject(new YoutubeDLConfig { VideoCodecToPreferWhenDownloading = choice }));

            Assert.Equal(expectedText, (string)youtubeDLJson["VideoCodecToPreferWhenDownloading"]);
        }

        [Theory]
        [InlineData("h264", VideoCodecChoice.H264)]
        [InlineData("any", VideoCodecChoice.AnyCodec)]
        public void TheVideoCodecToPreferIsReadFromItsText(string text, VideoCodecChoice expectedChoice)
        {
            var youtubeDLConfig = JsonConvert.DeserializeObject<YoutubeDLConfig>("{ \"VideoCodecToPreferWhenDownloading\": \"" + text + "\" }");

            Assert.Equal(expectedChoice, youtubeDLConfig.VideoCodecToPreferWhenDownloading);
        }

        [Theory]
        [InlineData(MaximumVideoResolutionChoice.NoLimit, "none")]
        [InlineData(MaximumVideoResolutionChoice.UpTo2160p, "2160p")]
        [InlineData(MaximumVideoResolutionChoice.UpTo1440p, "1440p")]
        [InlineData(MaximumVideoResolutionChoice.UpTo1080p, "1080p")]
        [InlineData(MaximumVideoResolutionChoice.UpTo720p, "720p")]
        [InlineData(MaximumVideoResolutionChoice.UpTo480p, "480p")]
        [InlineData(MaximumVideoResolutionChoice.UpTo360p, "360p")]
        public void TheMaximumVideoResolutionIsWrittenAsItsText(MaximumVideoResolutionChoice choice, string expectedText)
        {
            var youtubeDLJson = JObject.Parse(JsonConvert.SerializeObject(new YoutubeDLConfig { MaximumVideoResolutionOnTheShorterSide = choice }));

            Assert.Equal(expectedText, (string)youtubeDLJson["MaximumVideoResolutionOnTheShorterSide"]);
        }

        [Theory]
        [InlineData("none", MaximumVideoResolutionChoice.NoLimit)]
        [InlineData("2160p", MaximumVideoResolutionChoice.UpTo2160p)]
        [InlineData("1440p", MaximumVideoResolutionChoice.UpTo1440p)]
        [InlineData("1080p", MaximumVideoResolutionChoice.UpTo1080p)]
        [InlineData("720p", MaximumVideoResolutionChoice.UpTo720p)]
        [InlineData("480p", MaximumVideoResolutionChoice.UpTo480p)]
        [InlineData("360p", MaximumVideoResolutionChoice.UpTo360p)]
        public void TheMaximumVideoResolutionIsReadFromItsText(string text, MaximumVideoResolutionChoice expectedChoice)
        {
            var youtubeDLConfig = JsonConvert.DeserializeObject<YoutubeDLConfig>("{ \"MaximumVideoResolutionOnTheShorterSide\": \"" + text + "\" }");

            Assert.Equal(expectedChoice, youtubeDLConfig.MaximumVideoResolutionOnTheShorterSide);
        }

        [Theory]
        [InlineData(AudioQualityChoice.BestAac, "best-aac")]
        [InlineData(AudioQualityChoice.BestOfAnyCodec, "best-any-codec")]
        [InlineData(AudioQualityChoice.SmallestAac, "smallest-aac")]
        public void TheAudioQualityIsWrittenAsItsText(AudioQualityChoice choice, string expectedText)
        {
            var youtubeDLJson = JObject.Parse(JsonConvert.SerializeObject(new YoutubeDLConfig { AudioQualityToDownload = choice }));

            Assert.Equal(expectedText, (string)youtubeDLJson["AudioQualityToDownload"]);
        }

        [Theory]
        [InlineData("best-aac", AudioQualityChoice.BestAac)]
        [InlineData("best-any-codec", AudioQualityChoice.BestOfAnyCodec)]
        [InlineData("smallest-aac", AudioQualityChoice.SmallestAac)]
        public void TheAudioQualityIsReadFromItsText(string text, AudioQualityChoice expectedChoice)
        {
            var youtubeDLConfig = JsonConvert.DeserializeObject<YoutubeDLConfig>("{ \"AudioQualityToDownload\": \"" + text + "\" }");

            Assert.Equal(expectedChoice, youtubeDLConfig.AudioQualityToDownload);
        }
    }
}
