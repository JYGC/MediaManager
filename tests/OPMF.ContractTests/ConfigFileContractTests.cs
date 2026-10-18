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
                    "CheckForBinaryUpdates",
                    "ExecutableTriedFirstToDownloadVideos",
                    "FalseErrorMessages",
                    "GetSubtitles",
                    "MaxNoOfParallelDownloads",
                    "SubtitleExtension",
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
    }
}
