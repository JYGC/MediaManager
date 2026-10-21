using OPMF.Settings;

namespace MediaManager.Initializations;

public interface IApplicationStartupUseCase
{
    void InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();
}

public class ApplicationStartupUseCase(
    SettingsEstablishedAtStartup settingsEstablishedAtStartup,
    Func<IReadonlySettings> createReadonlySettingsForThisBuildConfiguration,
    Action<IReadonlySettings> createAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing,
    Func<string, AppConfig> readConfigFileOrWriteDefaultConfigFileWhenMissing) : IApplicationStartupUseCase
{
    public void InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig()
    {
        IReadonlySettings readonlySettings = createReadonlySettingsForThisBuildConfiguration();
        settingsEstablishedAtStartup.KeepReadonlySettingsInitializedAtStartup(readonlySettings);

        createAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(readonlySettings);

        AppConfig appConfig = readConfigFileOrWriteDefaultConfigFileWhenMissing(readonlySettings.GetConfigFilePath());
        settingsEstablishedAtStartup.KeepAppConfigEstablishedAtStartup(appConfig);
    }
}
