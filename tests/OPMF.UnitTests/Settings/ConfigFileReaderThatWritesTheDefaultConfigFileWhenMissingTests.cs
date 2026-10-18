using System.IO;
using Newtonsoft.Json;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissingTests
    {
        private readonly ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing _configFileReader = new ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing();

        [Fact]
        public void AnExistingConfigFileIsRead()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            File.WriteAllText(configFilePath, "{ \"NewChannelPastVideoDayLimit\": 7, \"VideoOutputFolderPath\": \"C:/Videos\" }");

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(7, appConfig.NewChannelPastVideoDayLimit);
            Assert.Equal("C:/Videos", appConfig.VideoOutputFolderPath);
        }

        [Fact]
        public void KeysMissingFromAnExistingConfigFileGetTheirDefaults()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            File.WriteAllText(configFilePath, "{ \"NewChannelPastVideoDayLimit\": 7 }");
            var defaultAppConfig = new AppConfig();

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(defaultAppConfig.StopDownloadInstanceAfterSeconds, appConfig.StopDownloadInstanceAfterSeconds);
            Assert.Equal(defaultAppConfig.GoogleClientSecretPath, appConfig.GoogleClientSecretPath);
            Assert.Equal(defaultAppConfig.YoutubeDL.MaxNoOfParallelDownloads, appConfig.YoutubeDL.MaxNoOfParallelDownloads);
        }

        [Fact]
        public void AnExistingConfigFileIsLeftByteForByteUnchanged()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            var bytesOfTheConfigFileBeforeReading = System.Text.Encoding.UTF8.GetBytes("{ \"NewChannelPastVideoDayLimit\":   7 }\n\n");
            File.WriteAllBytes(configFilePath, bytesOfTheConfigFileBeforeReading);

            _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(bytesOfTheConfigFileBeforeReading, File.ReadAllBytes(configFilePath));
        }

        [Fact]
        public void WithNoConfigFileTheDefaultConfigIsReturned()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");
            var defaultAppConfig = new AppConfig();

            var appConfig = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(JsonConvert.SerializeObject(defaultAppConfig), JsonConvert.SerializeObject(appConfig));
        }

        [Fact]
        public void WithNoConfigFileAnIndentedDefaultConfigFileIsWritten()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var configFilePath = Path.Combine(temporaryFolder.FolderPath, "config.json");

            _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configFilePath);

            Assert.Equal(JsonConvert.SerializeObject(new AppConfig(), Formatting.Indented), File.ReadAllText(configFilePath));
        }
    }
}
