using System;

namespace OPMF.Settings
{
    public class SettingsEstablishedAtStartup
    {
        private IReadonlySettings _readonlySettings;
        private AppConfig _appConfig;

        public IReadonlySettings ReadonlySettings =>
            _readonlySettings ?? throw new InvalidOperationException("The read-only settings have not been kept yet. Startup must run first.");

        public AppConfig AppConfig =>
            _appConfig ?? throw new InvalidOperationException("The config has not been kept yet. Startup must run first.");

        public void KeepReadonlySettingsInitializedAtStartup(IReadonlySettings readonlySettings)
        {
            _readonlySettings = readonlySettings;
        }

        public void KeepAppConfigEstablishedAtStartup(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }
    }
}
