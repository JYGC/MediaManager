using MediaManager.Dtos.Repositories;
using OPMF.Database;
using OPMF.Entities;

namespace MediaManager.Repositories;

public interface IChannelRepository
{
    List<RepositoryChannelDto> GetAllChannels();
    List<RepositoryChannelDto> GetChannelsThatAreNotBlacklisted();
    List<RepositoryChannelDto> GetChannelsWhoseNameContains(string wordInChannelName);
    List<RepositoryChannelDto> GetChannelsWithSiteIds(IEnumerable<string> channelSiteIds);
    RepositoryChannelDto? GetChannelWithSiteId(string channelSiteId);
    void InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(IEnumerable<RepositoryChannelDto> channels);
    void UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(IEnumerable<RepositoryChannelDto> channels);
    void UpdateBlacklistFlagOfChannels(IEnumerable<RepositoryChannelDto> channels);
}

public class ChannelRepository(IDatabaseConnectionOpener databaseConnectionOpener) : IChannelRepository
{
    public List<RepositoryChannelDto> GetAllChannels()
    {
        return ReadChannelsAsDtos(channelCollection => channelCollection.GetAll());
    }

    public List<RepositoryChannelDto> GetChannelsThatAreNotBlacklisted()
    {
        return ReadChannelsAsDtos(channelCollection => channelCollection.GetNotBacklisted());
    }

    public List<RepositoryChannelDto> GetChannelsWhoseNameContains(string wordInChannelName)
    {
        return ReadChannelsAsDtos(channelCollection => channelCollection.GetManyByWordInName(wordInChannelName));
    }

    public List<RepositoryChannelDto> GetChannelsWithSiteIds(IEnumerable<string> channelSiteIds)
    {
        return ReadChannelsAsDtos(channelCollection => channelCollection.GetManyBySiteIds(channelSiteIds));
    }

    public RepositoryChannelDto? GetChannelWithSiteId(string channelSiteId)
    {
        RepositoryChannelDto? channelDto = null;
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
        {
            Channel? channel = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId(channelSiteId);
            channelDto = channel == null ? null : DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto(channel);
        });
        return channelDto;
    }

    public void InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(IEnumerable<RepositoryChannelDto> channels)
    {
        List<Channel> channelsToStore = ConvertToYoutubeChannels(channels);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(channelsToStore));
    }

    public void UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(IEnumerable<RepositoryChannelDto> channels)
    {
        List<Channel> channelsToUpdate = ConvertToYoutubeChannels(channels);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeChannelDbCollection.UpdateLastCheckedOutAndActivity(channelsToUpdate));
    }

    public void UpdateBlacklistFlagOfChannels(IEnumerable<RepositoryChannelDto> channels)
    {
        List<Channel> channelsToUpdate = ConvertToYoutubeChannels(channels);
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            databaseAdapter.YoutubeChannelDbCollection.UpdateBlackListStatus(channelsToUpdate));
    }

    private List<RepositoryChannelDto> ReadChannelsAsDtos(Func<YoutubeChannelDbCollection, IEnumerable<Channel>> readChannels)
    {
        List<RepositoryChannelDto> channelDtos = [];
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            channelDtos = readChannels(databaseAdapter.YoutubeChannelDbCollection)
                .Select(DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto)
                .ToList());
        return channelDtos;
    }

    private static List<Channel> ConvertToYoutubeChannels(IEnumerable<RepositoryChannelDto> channelDtos)
    {
        return channelDtos.Select(channelDto => (Channel)DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeChannel(channelDto)).ToList();
    }
}
