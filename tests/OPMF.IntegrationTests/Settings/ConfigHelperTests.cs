using System;
using System.IO;
using Newtonsoft.Json;
using OPMF.IntegrationTests.Support;
using OPMF.Settings;
using Xunit;

namespace OPMF.IntegrationTests.Settings
{
    public class ConfigHelperTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public ConfigHelperTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void EstablishConfig_WritesDefaultsWhenMissing_ThenLoads()
        {
            var configPath = ConfigHelper.ReadonlySettings.GetConfigFilePath();
            if (File.Exists(configPath)) File.Delete(configPath);

            ConfigHelper.EstablishConfig();

            Assert.True(File.Exists(configPath));
            Assert.NotNull(ConfigHelper.Config);
            Assert.Equal(new AppConfig().NewChannelPastVideoDayLimit, ConfigHelper.Config.NewChannelPastVideoDayLimit);
        }

        [Fact]
        public void EstablishConfig_PrefersExistingFileOverDefaults()
        {
            var configPath = ConfigHelper.ReadonlySettings.GetConfigFilePath();
            var customized = new AppConfig { NewChannelPastVideoDayLimit = 42 };
            File.WriteAllText(configPath, JsonConvert.SerializeObject(customized, Formatting.Indented));

            ConfigHelper.EstablishConfig();

            Assert.Equal(42, ConfigHelper.Config.NewChannelPastVideoDayLimit);
        }
    }
}
