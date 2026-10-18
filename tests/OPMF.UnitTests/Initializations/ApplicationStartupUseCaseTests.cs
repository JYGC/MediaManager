using System;
using MediaManager.Initializations;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Initializations
{
    public class ApplicationStartupUseCaseTests
    {
        private readonly CallRecorder _callRecorder = new CallRecorder();
        private readonly SettingsEstablishedAtStartup _settingsEstablishedAtStartup = new SettingsEstablishedAtStartup();
        private readonly ReadonlySettings _readonlySettingsTheFirstStepCreates = new ReadonlySettings();
        private readonly AppConfig _appConfigTheThirdStepReads = new AppConfig();
        private IReadonlySettings _readonlySettingsTheSecondStepReceived;
        private string _configFilePathTheThirdStepReceived;
        private Exception _exceptionTheThirdStepThrows;

        private ApplicationStartupUseCase CreateApplicationStartupUseCase()
        {
            return new ApplicationStartupUseCase(
                _settingsEstablishedAtStartup,
                () =>
                {
                    _callRecorder.Calls.Add("create the read-only settings");
                    return _readonlySettingsTheFirstStepCreates;
                },
                readonlySettings =>
                {
                    _callRecorder.Calls.Add("create the folders");
                    _readonlySettingsTheSecondStepReceived = readonlySettings;
                },
                configFilePath =>
                {
                    _callRecorder.Calls.Add("read the config");
                    _configFilePathTheThirdStepReceived = configFilePath;
                    if (_exceptionTheThirdStepThrows != null)
                    {
                        throw _exceptionTheThirdStepThrows;
                    }
                    return _appConfigTheThirdStepReads;
                });
        }

        [Fact]
        public void TheStepsRunInTheOrderOfReadonlySettingsThenFoldersThenConfig()
        {
            CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();

            Assert.Equal(new[] { "create the read-only settings", "create the folders", "read the config" }, _callRecorder.Calls);
        }

        [Fact]
        public void TheFoldersAreCreatedFromTheReadonlySettingsOfTheFirstStep()
        {
            CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();

            Assert.Same(_readonlySettingsTheFirstStepCreates, _readonlySettingsTheSecondStepReceived);
        }

        [Fact]
        public void TheConfigIsReadFromTheConfigFilePathOfTheReadonlySettings()
        {
            CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();

            Assert.Equal(_readonlySettingsTheFirstStepCreates.GetConfigFilePath(), _configFilePathTheThirdStepReceived);
        }

        [Fact]
        public void TheReadonlySettingsAndTheConfigAreKeptInTheSettingsEstablishedAtStartup()
        {
            CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();

            Assert.Same(_readonlySettingsTheFirstStepCreates, _settingsEstablishedAtStartup.ReadonlySettings);
            Assert.Same(_appConfigTheThirdStepReads, _settingsEstablishedAtStartup.AppConfig);
        }

        [Fact]
        public void TheReadonlySettingsAreAlreadyKeptWhenReadingTheConfigFails()
        {
            _exceptionTheThirdStepThrows = new InvalidOperationException("config.json is broken");

            Assert.Throws<InvalidOperationException>(() => CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig());

            Assert.Same(_readonlySettingsTheFirstStepCreates, _settingsEstablishedAtStartup.ReadonlySettings);
            Assert.Throws<InvalidOperationException>(() => _settingsEstablishedAtStartup.AppConfig);
        }

        [Fact]
        public void ExceptionFromTheThirdStepPropagatesUnchanged()
        {
            var exceptionOfTheThirdStep = new InvalidOperationException("config.json is broken");
            _exceptionTheThirdStepThrows = exceptionOfTheThirdStep;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(() => CreateApplicationStartupUseCase().InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig());

            Assert.Same(exceptionOfTheThirdStep, exceptionThatPropagated);
        }
    }
}
