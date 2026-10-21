using System.IO;
using Newtonsoft.Json;
using OPMF.IntegrationTests.Support;
using OPMF.Settings;
using Xunit;

namespace OPMF.IntegrationTests.Settings
{
    public class ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissingTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing _configFileReader = new ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing();

        public ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissingTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        [Fact]
        public void ReadConfigFileOrWriteDefaultConfigFileWhenMissing_WritesDefaultsWhenMissing_ThenLoads()
        {
            var configPath = _fixture.ReadonlySettings.GetConfigFilePath();
            if (File.Exists(configPath)) File.Delete(configPath);

            var config = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configPath);

            Assert.True(File.Exists(configPath));
            Assert.NotNull(config);
            Assert.Equal(new AppConfig().NewChannelPastVideoDayLimit, config.NewChannelPastVideoDayLimit);
        }

        [Fact]
        public void ReadConfigFileOrWriteDefaultConfigFileWhenMissing_PrefersExistingFileOverDefaults()
        {
            var configPath = _fixture.ReadonlySettings.GetConfigFilePath();
            var customized = new AppConfig { NewChannelPastVideoDayLimit = 42 };
            File.WriteAllText(configPath, JsonConvert.SerializeObject(customized, Formatting.Indented));

            var config = _configFileReader.ReadConfigFileOrWriteDefaultConfigFileWhenMissing(configPath);

            Assert.Equal(42, config.NewChannelPastVideoDayLimit);
        }
    }
}
