using MediaManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using OPMF.Filesystem;

namespace MediaManager.Services2;

public static class ServiceLayerServiceCollectionExtensions
{
    public static IServiceCollection AddMediaManagerServiceLayer(this IServiceCollection services)
    {
        services.AddTransient<IMissingFolderCreator, MissingFolderCreator>();
        services.AddTransient<IDownloadedVideoFileMover, DownloadedVideoFileMover>();

        services.AddTransient<IChannelServices, ChannelServices>();
        services.AddTransient<IMetadataServices, MetadataServices>();
        services.AddTransient<IChannelMetadataServices, ChannelMetadataServices>();
        services.AddTransient<ILogServices, LogServices>();
        services.AddTransient<ITaskRunnerServices, TaskRunnerServices>();
        services.AddTransient<INewVideoMetadataFetchServices, NewVideoMetadataFetchServices>();
        services.AddTransient<IVideoDownloadServices, VideoDownloadServices>();

        return services.AddMediaManagerRepositoryLayer();
    }
}
