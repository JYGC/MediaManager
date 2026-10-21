using MediaManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using OPMF.Filesystem;
using OPMF.Settings;

namespace MediaManager.Services2;

public static class ServiceLayerServiceCollectionExtensions
{
    public static IServiceCollection AddMediaManagerServiceLayer(this IServiceCollection services)
    {
        services.AddTransient<IMissingFolderCreator, MissingFolderCreator>();
        services.AddTransient<IDownloadedVideoFileMover, DownloadedVideoFileMover>();
        services.AddTransient<IConfigFileYoutubeDLKeyWriter, ConfigFileYoutubeDLKeyWriter>();

        services.AddTransient<IChannelServices, ChannelServices>();
        services.AddTransient<IMetadataServices, MetadataServices>();
        // The Application layer registers the reader of the config file
        services.AddTransient<IDownloadQualitySettingsServices>(serviceProvider => new DownloadQualitySettingsServices(
            serviceProvider.GetRequiredService<IReadonlySettings>(),
            serviceProvider.GetRequiredService<ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing>().ReadConfigFileOrWriteDefaultConfigFileWhenMissing,
            serviceProvider.GetRequiredService<IConfigFileYoutubeDLKeyWriter>(),
            serviceProvider.GetRequiredService<ILogRepository>()));
        services.AddTransient<IChannelMetadataServices, ChannelMetadataServices>();
        services.AddTransient<ILogServices, LogServices>();
        services.AddTransient<ITaskRunnerServices, TaskRunnerServices>();
        services.AddTransient<INewVideoMetadataFetchServices, NewVideoMetadataFetchServices>();
        services.AddTransient<IVideoDownloadServices, VideoDownloadServices>();

        return services.AddMediaManagerRepositoryLayer();
    }
}
