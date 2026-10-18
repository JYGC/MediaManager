using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class ConfigHelperCreateReadonlySettingsForThisBuildConfigurationTests
    {
#if DEBUG
        [Fact]
        public void InADebugBuildItReturnsTheDevSettings()
        {
            var readonlySettings = ConfigHelper.CreateReadonlySettingsForThisBuildConfiguration();

            Assert.IsType<ReadOnlyDevSettings>(readonlySettings);
        }
#else
        [Fact]
        public void InAReleaseBuildItReturnsTheNormalSettings()
        {
            var readonlySettings = ConfigHelper.CreateReadonlySettingsForThisBuildConfiguration();

            Assert.IsType<ReadonlySettings>(readonlySettings);
        }
#endif
    }
}
