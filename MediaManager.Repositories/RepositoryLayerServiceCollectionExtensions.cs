using System.Reflection;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using OPMF.Database;
using OPMF.Downloader.YTDownloader;
using OPMF.Logging;
using OPMF.Settings;
using OPMF.SiteAdapter.Youtube;
using OPMF.TextLogging;

namespace MediaManager.Repositories;

public static class RepositoryLayerServiceCollectionExtensions
{
    // Expects the caller to have registered IReadonlySettings, AppConfig, TextLog, TimeProvider and the standard-output TextWriter.
    public static IServiceCollection AddMediaManagerRepositoryLayer(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseConnectionOpener>(serviceProvider => new SharedModeDatabaseFileConnectionOpener(
            serviceProvider.GetRequiredService<IReadonlySettings>().GetDatabasePath(),
            serviceProvider.GetRequiredService<TextLog>()));
        services.AddTransient<Logger>();
        services.AddTransient<IExternalProcessStarter, ExternalProcessStarter>();

        services.AddTransient<IChannelRepository, ChannelRepository>();
        services.AddTransient<IVideoMetadataRepository, VideoMetadataRepository>();
        services.AddTransient<ILogRepository, LogRepository>();
        services.AddTransient<ITaskRunnerProcessRepository, TaskRunnerProcessRepository>();
        services.AddTransient<IVideoSiteRepository>(serviceProvider => new VideoSiteRepository(
            () => new YoutubeMetadataFetcher(
                SignInToGoogleAndCreateYoutubeDataApi(serviceProvider, typeof(YoutubeMetadataFetcher)),
                serviceProvider.GetRequiredService<AppConfig>().NewChannelPastVideoDayLimit,
                serviceProvider.GetRequiredService<TimeProvider>(),
                serviceProvider.GetRequiredService<Logger>(),
                serviceProvider.GetRequiredService<TextWriter>()),
            () => new YoutubeVideoMetadataGetter(SignInToGoogleAndCreateYoutubeDataApi(serviceProvider, typeof(YoutubeVideoMetadataGetter)))));
        services.AddTransient<IVideoDownloaderRepository>(serviceProvider => new VideoDownloaderRepository(() => CreateYTDownloader(serviceProvider)));

        return services;
    }

    private static IYoutubeDataApi SignInToGoogleAndCreateYoutubeDataApi(IServiceProvider serviceProvider, Type youtubeClientTypeWhoseNameIsSentToGoogle)
    {
        AppConfig appConfig = serviceProvider.GetRequiredService<AppConfig>();
        string clientSecretFilePath = GoogleClientSecretFileLocation.GetClientSecretFilePathInFolderOfEntryExecutable(
            Assembly.GetEntryAssembly()!.GetName().CodeBase!, appConfig.GoogleClientSecretPath);
        var googleSignIn = new GoogleSignIn(
            clientSecretFilePath,
            serviceProvider.GetRequiredService<IReadonlySettings>().GetCredentialPath(),
            serviceProvider.GetRequiredService<TextWriter>());
        return new YoutubeDataApi(googleSignIn.SignInAndGetCredentialForScopes([YouTubeService.Scope.YoutubeReadonly]), youtubeClientTypeWhoseNameIsSentToGoogle.ToString());
    }

    private static YTDownloader CreateYTDownloader(IServiceProvider serviceProvider)
    {
        AppConfig appConfig = serviceProvider.GetRequiredService<AppConfig>();
        Logger logger = serviceProvider.GetRequiredService<Logger>();
        return new YTDownloader(
            serviceProvider.GetRequiredService<IReadonlySettings>(),
            appConfig,
            new NYoutubeDLVideoDownloaderExecutableRunner(appConfig.YoutubeDL.GetSubtitles),
            new MissingVideoDownloaderExecutableDownloader(new WebClientUrlToFileDownloader(), logger, serviceProvider.GetRequiredService<TextWriter>()),
            new InterruptibleDownloadThreadStarter(),
            new ThreadSleepCurrentThreadWaiter(),
            new ConsoleRowOverwriter(),
            serviceProvider.GetRequiredService<TimeProvider>(),
            logger);
    }
}
