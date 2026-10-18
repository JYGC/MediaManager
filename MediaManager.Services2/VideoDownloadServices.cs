using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Filesystem;
using OPMF.Settings;

namespace MediaManager.Services2;

public interface IVideoDownloadServices
{
    void DownloadEveryVideoQueuedToDownload();
    void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt);
}

public class VideoDownloadServices(
    IVideoDownloaderRepository videoDownloaderRepository,
    IVideoMetadataRepository videoMetadataRepository,
    ILogRepository logRepository,
    IMissingFolderCreator missingFolderCreator,
    IDownloadedVideoFileMover downloadedVideoFileMover,
    IReadonlySettings readonlySettings,
    AppConfig appConfig,
    TextWriter standardOutput) : IVideoDownloadServices
{
    public void DownloadEveryVideoQueuedToDownload()
    {
        DownloadVideos(videoMetadataRepository.GetVideosQueuedToDownload);
    }

    public void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt)
    {
        // An unknown site id gives a queue holding null, which fails when the queue is first used, as it always has
        DownloadVideos(() => [videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(siteIdOrPartOfIt)!]);
    }

    private void DownloadVideos(Func<List<RepositoryVideoMetadataDto>> getVideosToDownload)
    {
        List<RepositoryVideoMetadataDto>? videosToDownload = null;
        try
        {
            videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();

            standardOutput.WriteLine("fetching videos");
            videosToDownload = getVideosToDownload();
            videoMetadataRepository.SetBeingDownloadedFlagOfVideos(videosToDownload, true);

            List<RepositoryVideoMetadataDto> downloadedVideos = videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(videosToDownload);
            videoMetadataRepository.UpdateDownloadStatusOfVideos(downloadedVideos);
            videoMetadataRepository.SetBeingDownloadedFlagOfVideos(downloadedVideos, false);

            missingFolderCreator.CreateFolderWhenMissing(appConfig.VideoOutputFolderPath);
            downloadedVideoFileMover.MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(
                readonlySettings.GetDownloadFolderPath(),
                appConfig.VideoOutputFolderPath,
                [appConfig.YoutubeDL.VideoExtension, appConfig.YoutubeDL.SubtitleExtension]);
        }
        catch (Exception exception)
        {
            logRepository.RecordErrorLogEntry(ErrorLogEntryToRecordCreator.CreateWithVariablesSerializedAsJsonText(
                exception,
                new Dictionary<string, object?> { { "metadatas", videosToDownload } }));
            throw;
        }
    }
}
