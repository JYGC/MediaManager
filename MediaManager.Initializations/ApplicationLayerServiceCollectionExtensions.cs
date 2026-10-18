using MediaManager.Services2;
using Microsoft.Extensions.DependencyInjection;
using OPMF.Filesystem;
using OPMF.Settings;
using OPMF.TextLogging;

namespace MediaManager.Initializations;

public static class ApplicationLayerServiceCollectionExtensions
{
    // Expects the composition root to have registered TimeProvider and the standard-output TextWriter.
    public static IServiceCollection AddMediaManagerApplicationLayer(this IServiceCollection services)
    {
        services.AddSingleton<SettingsEstablishedAtStartup>();
        services.AddTransient<IReadonlySettings>(serviceProvider => serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>().ReadonlySettings);
        services.AddTransient<AppConfig>(serviceProvider => serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>().AppConfig);
        services.AddSingleton<TextLog>(serviceProvider => new TextLog(
            serviceProvider.GetRequiredService<IReadonlySettings>().GetTextLogFile(),
            serviceProvider.GetRequiredService<TimeProvider>()));
        services.AddTransient<ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing>();

        services.AddTransient<IApplicationStartupUseCase>(serviceProvider => new ApplicationStartupUseCase(
            serviceProvider.GetRequiredService<SettingsEstablishedAtStartup>(),
            ConfigHelper.CreateReadonlySettingsForThisBuildConfiguration,
            serviceProvider.GetRequiredService<IMissingFolderCreator>().CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing,
            serviceProvider.GetRequiredService<ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing>().ReadConfigFileOrWriteDefaultConfigFileWhenMissing));
        services.AddTransient<IUnhandledErrorRecordingUseCase, UnhandledErrorRecordingUseCase>();
        // Recording an error needs the database path that startup itself keeps, so the recorder must not be created before startup ran
        services.AddTransient<Lazy<IUnhandledErrorRecordingUseCase>>(serviceProvider =>
            new Lazy<IUnhandledErrorRecordingUseCase>(() => serviceProvider.GetRequiredService<IUnhandledErrorRecordingUseCase>()));
        services.AddTransient<IApplicationEntryPointUseCase, ApplicationEntryPointUseCase>();
        services.AddTransient<ICommandLineTaskUseCases, CommandLineTaskUseCases>();
        services.AddTransient<IChannelBrowsingUseCases, ChannelBrowsingUseCases>();
        services.AddTransient<IVideoBrowsingUseCases, VideoBrowsingUseCases>();
        services.AddTransient<ILogBrowsingUseCases, LogBrowsingUseCases>();

        return services.AddMediaManagerServiceLayer();
    }
}
