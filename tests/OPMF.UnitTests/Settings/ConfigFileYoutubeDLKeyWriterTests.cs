using System.IO;
using Newtonsoft.Json.Linq;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class ConfigFileYoutubeDLKeyWriterTests
    {
        private readonly ConfigFileYoutubeDLKeyWriter _writer = new ConfigFileYoutubeDLKeyWriter();

        private static string WriteConfigFile(TemporaryFolderForOneTest temporaryFolder, string configFileText)
        {
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            File.WriteAllText(configFilePath, configFileText);
            return configFilePath;
        }

        private static JObject ReadConfigFileAsJson(string configFilePath)
        {
            return JObject.Parse(File.ReadAllText(configFilePath));
        }

        [Fact]
        public void OnlyTheGivenKeyChanges()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"NewChannelPastVideoDayLimit\": 3, \"YoutubeDL\": { \"VideoExtension\": \"mkv\", \"GetSubtitles\": true, \"AudioQualityToDownload\": \"best-aac\" } }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.AudioQualityToDownload), AudioQualityChoice.SmallestAac);

            var configFileJson = ReadConfigFileAsJson(configFilePath);
            Assert.Equal("smallest-aac", (string)configFileJson["YoutubeDL"]["AudioQualityToDownload"]);
            Assert.Equal("mkv", (string)configFileJson["YoutubeDL"]["VideoExtension"]);
            Assert.True((bool)configFileJson["YoutubeDL"]["GetSubtitles"]);
            Assert.Equal(3, (int)configFileJson["NewChannelPastVideoDayLimit"]);
        }

        [Fact]
        public void AMissingKeyIsAdded()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"YoutubeDL\": { \"VideoExtension\": \"mp4\" } }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.VideoCodecToPreferWhenDownloading), VideoCodecChoice.AnyCodec);

            var youtubeDLJson = (JObject)ReadConfigFileAsJson(configFilePath)["YoutubeDL"];
            Assert.Equal("any", (string)youtubeDLJson["VideoCodecToPreferWhenDownloading"]);
            Assert.Equal("mp4", (string)youtubeDLJson["VideoExtension"]);
        }

        [Fact]
        public void AMissingYoutubeDLObjectIsAdded()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"NewChannelPastVideoDayLimit\": 3 }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.MaximumVideoResolutionOnTheShorterSide), MaximumVideoResolutionChoice.UpTo720p);

            var configFileJson = ReadConfigFileAsJson(configFilePath);
            Assert.Equal("720p", (string)configFileJson["YoutubeDL"]["MaximumVideoResolutionOnTheShorterSide"]);
            Assert.Equal(3, (int)configFileJson["NewChannelPastVideoDayLimit"]);
        }

        [Fact]
        public void UnknownKeysAndEveryOtherValueAreKept()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"SomethingAddedByAFutureVersion\": { \"A\": [1, 2, 3] }, \"YoutubeDL\": { \"FalseErrorMessages\": [ \"one\", \"two\" ], \"AnotherUnknownKey\": \"x\" } }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.AudioQualityToDownload), AudioQualityChoice.BestOfAnyCodec);

            var configFileJson = ReadConfigFileAsJson(configFilePath);
            Assert.Equal(new JArray(1, 2, 3), configFileJson["SomethingAddedByAFutureVersion"]["A"]);
            Assert.Equal(new JArray("one", "two"), configFileJson["YoutubeDL"]["FalseErrorMessages"]);
            Assert.Equal("x", (string)configFileJson["YoutubeDL"]["AnotherUnknownKey"]);
        }

        [Theory]
        [InlineData(MaximumVideoResolutionChoice.NoLimit, "none")]
        [InlineData(MaximumVideoResolutionChoice.UpTo720p, "720p")]
        public void AnEnumIsWrittenAsItsText(MaximumVideoResolutionChoice choice, string expectedText)
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"YoutubeDL\": {} }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.MaximumVideoResolutionOnTheShorterSide), choice);

            Assert.Equal(expectedText, (string)ReadConfigFileAsJson(configFilePath)["YoutubeDL"]["MaximumVideoResolutionOnTheShorterSide"]);
        }

        [Fact]
        public void TheWrittenFileIsIndented()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{\"YoutubeDL\":{}}");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.VideoCodecToPreferWhenDownloading), VideoCodecChoice.H264);

            Assert.Contains("\n", File.ReadAllText(configFilePath).Trim());
        }

        [Fact]
        public void AMissingConfigFileThrowsFileNotFoundException()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");

            Assert.Throws<FileNotFoundException>(() =>
                _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.VideoCodecToPreferWhenDownloading), VideoCodecChoice.H264));
        }

        [Fact]
        public void NoTemporaryFileIsLeftAfterwards()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFile(temporaryFolder, "{ \"YoutubeDL\": {} }");

            _writer.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(configFilePath, nameof(YoutubeDLConfig.AudioQualityToDownload), AudioQualityChoice.SmallestAac);

            Assert.False(File.Exists(configFilePath + ".tmp"));
            Assert.Equal(new[] { configFilePath }, Directory.GetFiles(temporaryFolder.FolderPath));
        }
    }
}
