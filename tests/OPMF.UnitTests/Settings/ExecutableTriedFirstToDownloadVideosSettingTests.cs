using System.IO;
using Newtonsoft.Json;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class ExecutableTriedFirstToDownloadVideosSettingTests
    {
        private readonly ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing _configFileReader = new ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing();

        private static string WriteConfigFileWithYoutubeDLObject(TemporaryFolderForOneTest temporaryFolder, string youtubeDLObjectJson)
        {
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            File.WriteAllText(configFilePath, "{ \"YoutubeDL\": " + youtubeDLObjectJson + " }");
            return configFilePath;
        }

        [Fact]
        public void TheDefaultIsYoutubeDl()
        {
            Assert.Equal(VideoDownloaderExecutable.YoutubeDl, new YoutubeDLConfig().ExecutableTriedFirstToDownloadVideos);
        }

        [Fact]
        public void AConfigFileWithoutTheKeyGivesYoutubeDl()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFileWithYoutubeDLObject(temporaryFolder, "{ \"CheckForBinaryUpdates\": true }");

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(VideoDownloaderExecutable.YoutubeDl, appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos);
        }

        [Fact]
        public void AConfigFileWithoutTheKeyIsLeftByteForByteUnchanged()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFileWithYoutubeDLObject(temporaryFolder, "{ \"CheckForBinaryUpdates\": true }");
            var bytesBeforeReading = File.ReadAllBytes(configFilePath);

            _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(bytesBeforeReading, File.ReadAllBytes(configFilePath));
        }

        [Fact]
        public void TheTextYtDlpGivesYtDlp()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFileWithYoutubeDLObject(temporaryFolder, "{ \"ExecutableTriedFirstToDownloadVideos\": \"yt-dlp\" }");

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(VideoDownloaderExecutable.YtDlp, appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos);
        }

        [Fact]
        public void TheTextYoutubeDlGivesYoutubeDl()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFileWithYoutubeDLObject(temporaryFolder, "{ \"ExecutableTriedFirstToDownloadVideos\": \"youtube-dl\" }");

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(VideoDownloaderExecutable.YoutubeDl, appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos);
        }

        [Fact]
        public void AnUnknownTextValueThrowsAJsonSerializationException()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = WriteConfigFileWithYoutubeDLObject(temporaryFolder, "{ \"ExecutableTriedFirstToDownloadVideos\": \"wget\" }");

            Assert.Throws<JsonSerializationException>(() => _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath));
        }

        [Fact]
        public void ANewlyWrittenConfigFileContainsTheKeyWithTheTextYoutubeDl()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");

            _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Contains("\"ExecutableTriedFirstToDownloadVideos\": \"youtube-dl\"", File.ReadAllText(configFilePath));
        }
    }
}
