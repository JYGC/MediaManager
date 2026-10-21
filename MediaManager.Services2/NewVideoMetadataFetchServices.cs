using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;

namespace MediaManager.Services2;

public interface INewVideoMetadataFetchServices
{
    void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();
}

public class NewVideoMetadataFetchServices(
    IChannelRepository channelRepository,
    IVideoSiteRepository videoSiteRepository,
    IVideoMetadataRepository videoMetadataRepository,
    ILogRepository logRepository,
    TextWriter standardOutput) : INewVideoMetadataFetchServices
{
    public void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted()
    {
        List<RepositoryChannelDto>? channels = null;
        List<RepositoryVideoMetadataDto>? videos = null;
        try
        {
            standardOutput.WriteLine("getting channels");
            channels = channelRepository.GetChannelsThatAreNotBlacklisted();

            RepositoryNewVideoMetadataFetchResultDto fetchResult = videoSiteRepository.FetchNewVideoMetadataForChannels(channels);
            channels = fetchResult.ChannelsWithUpdatedCheckDates;
            videos = fetchResult.NewlyFetchedVideos;

            standardOutput.WriteLine("saving metadata to database");
            videoMetadataRepository.InsertVideosThatAreNotStoredYet(videos);

            standardOutput.WriteLine("updating channels");
            channelRepository.UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(channels);
        }
        catch (Exception exception)
        {
            logRepository.RecordErrorLogEntry(ErrorLogEntryToRecordCreator.CreateWithVariablesSerializedAsJsonText(
                exception,
                new Dictionary<string, object?> { { "channels", channels }, { "metadatas", videos } }));
            throw;
        }
    }
}
