using MediaManager.Dtos.Repositories;
using OPMF.Downloader;
using OPMF.Entities;

namespace MediaManager.Repositories;

public interface IVideoDownloaderRepository
{
    void PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();
    List<RepositoryVideoMetadataDto> DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(List<RepositoryVideoMetadataDto> videosToDownload);
}

public class VideoDownloaderRepository(Func<IDownloader> createDownloaderThatDownloadsMissingExecutables) : IVideoDownloaderRepository
{
    private IDownloader? _downloader;

    public void PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded()
    {
        _downloader ??= createDownloaderThatDownloadsMissingExecutables();
    }

    public List<RepositoryVideoMetadataDto> DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(List<RepositoryVideoMetadataDto> videosToDownload)
    {
        PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();
        IDownloader downloader = _downloader!;
        downloader.DownloadQueue = videosToDownload
            .Select(videoDto => (Metadata)DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto))
            .ToList();

        downloader.StartDownloadingQueue();

        return downloader.DownloadQueue.Select(DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto).ToList();
    }
}
