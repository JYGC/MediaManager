using MediaManager.Dtos.Repositories;
using OPMF.Database;
using OPMF.Entities;

namespace MediaManager.Repositories;

public interface IVideoMetadataRepository
{
    List<RepositoryVideoMetadataDto> GetVideosQueuedToDownload();
    RepositoryVideoMetadataDto? GetFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt); // Query.Contains, as today
    List<RepositoryVideoMetadataDto> GetNewVideos(int skip, int pageSize);
    List<RepositoryVideoMetadataDto> GetVideosQueuedToDownloadOrWaiting(int skip, int pageSize);
    List<RepositoryVideoMetadataDto> GetVideosWhoseTitleContains(string wordInVideoTitle, int skip, int pageSize);
    List<RepositoryVideoMetadataDto> GetVideosOfChannelsWhoseTitleContains(IEnumerable<string> channelSiteIds, string wordInVideoTitle, int skip, int pageSize);
    void InsertVideosThatAreNotStoredYet(IEnumerable<RepositoryVideoMetadataDto> videos);
    void UpdateDownloadStatusOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos);
    void SetBeingDownloadedFlagOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos, bool isBeingDownloaded);
}

public class VideoMetadataRepository(IDatabaseConnectionOpener databaseConnectionOpener) : IVideoMetadataRepository
{
    public List<RepositoryVideoMetadataDto> GetVideosQueuedToDownload()
    {
        return ReadVideosAsDtos(videoCollection => videoCollection.GetToDownload());
    }

    public RepositoryVideoMetadataDto? GetFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt)
    {
        RepositoryVideoMetadataDto? videoDto = null;
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
        {
            Metadata? video = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId(siteIdOrPartOfIt);
            videoDto = video == null ? null : DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(video);
        });
        return videoDto;
    }

    public List<RepositoryVideoMetadataDto> GetNewVideos(int skip, int pageSize)
    {
        return ReadVideosAsDtos(videoCollection => videoCollection.GetNew(skip, pageSize));
    }

    public List<RepositoryVideoMetadataDto> GetVideosQueuedToDownloadOrWaiting(int skip, int pageSize)
    {
        return ReadVideosAsDtos(videoCollection => videoCollection.GetToDownloadAndWait(skip, pageSize));
    }

    public List<RepositoryVideoMetadataDto> GetVideosWhoseTitleContains(string wordInVideoTitle, int skip, int pageSize)
    {
        return ReadVideosAsDtos(videoCollection => videoCollection.GetManyByWordInTitle(wordInVideoTitle, skip, pageSize));
    }

    public List<RepositoryVideoMetadataDto> GetVideosOfChannelsWhoseTitleContains(IEnumerable<string> channelSiteIds, string wordInVideoTitle, int skip, int pageSize)
    {
        return ReadVideosAsDtos(videoCollection => videoCollection.GetManyByChannelSiteIdAndWordInTitle(channelSiteIds, wordInVideoTitle, skip, pageSize));
    }

    public void InsertVideosThatAreNotStoredYet(IEnumerable<RepositoryVideoMetadataDto> videos)
    {
        List<Metadata> videosToInsert = ConvertToYoutubeMetadata(videos);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeMetadataDbCollection.InsertNew(videosToInsert));
    }

    public void UpdateDownloadStatusOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos)
    {
        List<Metadata> videosToUpdate = ConvertToYoutubeMetadata(videos);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeMetadataDbCollection.UpdateStatus(videosToUpdate));
    }

    public void SetBeingDownloadedFlagOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos, bool isBeingDownloaded)
    {
        List<Metadata> videosToUpdate = ConvertToYoutubeMetadata(videos);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(videosToUpdate, isBeingDownloaded));
    }

    private List<RepositoryVideoMetadataDto> ReadVideosAsDtos(Func<YoutubeMetadataDbCollection, IEnumerable<Metadata>> readVideos)
    {
        List<RepositoryVideoMetadataDto> videoDtos = [];
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            videoDtos = readVideos(databaseAdapter.YoutubeMetadataDbCollection)
                .Select(DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto)
                .ToList());
        return videoDtos;
    }

    private static List<Metadata> ConvertToYoutubeMetadata(IEnumerable<RepositoryVideoMetadataDto> videoDtos)
    {
        return videoDtos.Select(videoDto => (Metadata)DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto)).ToList();
    }
}
