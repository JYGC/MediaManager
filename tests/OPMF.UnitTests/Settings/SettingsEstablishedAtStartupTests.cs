using System;
using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class SettingsEstablishedAtStartupTests
    {
        private readonly SettingsEstablishedAtStartup _settingsEstablishedAtStartup = new SettingsEstablishedAtStartup();

        [Fact]
        public void ReadingTheReadonlySettingsBeforeStartupKeptThemThrowsAnInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _settingsEstablishedAtStartup.ReadonlySettings);
        }

        [Fact]
        public void ReadingTheAppConfigBeforeStartupKeptItThrowsAnInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _settingsEstablishedAtStartup.AppConfig);
        }

        [Fact]
        public void TheReadonlySettingsAreReturnedOnceStartupKeptThem()
        {
            var readonlySettings = new ReadonlySettings();

            _settingsEstablishedAtStartup.KeepReadonlySettingsInitializedAtStartup(readonlySettings);

            Assert.Same(readonlySettings, _settingsEstablishedAtStartup.ReadonlySettings);
        }

        [Fact]
        public void TheAppConfigIsReturnedOnceStartupKeptIt()
        {
            var appConfig = new AppConfig();

            _settingsEstablishedAtStartup.KeepAppConfigEstablishedAtStartup(appConfig);

            Assert.Same(appConfig, _settingsEstablishedAtStartup.AppConfig);
        }

        [Fact]
        public void KeepingTheReadonlySettingsDoesNotMakeTheAppConfigAvailable()
        {
            _settingsEstablishedAtStartup.KeepReadonlySettingsInitializedAtStartup(new ReadonlySettings());

            Assert.Throws<InvalidOperationException>(() => _settingsEstablishedAtStartup.AppConfig);
        }

        [Fact]
        public void KeepingTheAppConfigDoesNotMakeTheReadonlySettingsAvailable()
        {
            _settingsEstablishedAtStartup.KeepAppConfigEstablishedAtStartup(new AppConfig());

            Assert.Throws<InvalidOperationException>(() => _settingsEstablishedAtStartup.ReadonlySettings);
        }
    }
}
