using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;

namespace OPMF.UnitTests.Support
{
    public class CallRecorder
    {
        public List<string> Calls { get; } = new List<string>();
    }

    public class FakeChannelRepository : IChannelRepository
    {
        private readonly CallRecorder _callRecorder;

        public FakeChannelRepository(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public List<RepositoryChannelDto> StoredChannels { get; } = new List<RepositoryChannelDto>();
        public List<RepositoryChannelDto> ChannelsInsertedOrUpdated { get; } = new List<RepositoryChannelDto>();
        public List<RepositoryChannelDto> ChannelsWhoseCheckDatesWereUpdated { get; } = new List<RepositoryChannelDto>();
        public List<RepositoryChannelDto> ChannelsWhoseBlacklistFlagWasUpdated { get; } = new List<RepositoryChannelDto>();
        public List<string> WordsInChannelNamesSearched { get; } = new List<string>();
        public List<IEnumerable<string>> SiteIdsRequested { get; } = new List<IEnumerable<string>>();

        public List<RepositoryChannelDto> GetAllChannels()
        {
            _callRecorder.Calls.Add("GetAllChannels");
            return StoredChannels.ToList();
        }

        public virtual List<RepositoryChannelDto> GetChannelsThatAreNotBlacklisted()
        {
            _callRecorder.Calls.Add("GetChannelsThatAreNotBlacklisted");
            return StoredChannels.Where(channel => !channel.IsBlacklisted).ToList();
        }

        public List<RepositoryChannelDto> GetChannelsWhoseNameContains(string wordInChannelName)
        {
            _callRecorder.Calls.Add("GetChannelsWhoseNameContains");
            WordsInChannelNamesSearched.Add(wordInChannelName);
            return StoredChannels.Where(channel => channel.Name.Contains(wordInChannelName)).ToList();
        }

        public List<RepositoryChannelDto> GetChannelsWithSiteIds(IEnumerable<string> channelSiteIds)
        {
            _callRecorder.Calls.Add("GetChannelsWithSiteIds");
            var requestedSiteIds = channelSiteIds.ToList();
            SiteIdsRequested.Add(requestedSiteIds);
            return StoredChannels.Where(channel => requestedSiteIds.Contains(channel.SiteId)).ToList();
        }

        public RepositoryChannelDto GetChannelWithSiteId(string channelSiteId)
        {
            _callRecorder.Calls.Add("GetChannelWithSiteId");
            return StoredChannels.FirstOrDefault(channel => channel.SiteId == channelSiteId);
        }

        public void InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(IEnumerable<RepositoryChannelDto> channels)
        {
            _callRecorder.Calls.Add("InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels");
            ChannelsInsertedOrUpdated.AddRange(channels);
        }

        public void UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(IEnumerable<RepositoryChannelDto> channels)
        {
            _callRecorder.Calls.Add("UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels");
            ChannelsWhoseCheckDatesWereUpdated.AddRange(channels);
        }

        public void UpdateBlacklistFlagOfChannels(IEnumerable<RepositoryChannelDto> channels)
        {
            _callRecorder.Calls.Add("UpdateBlacklistFlagOfChannels");
            ChannelsWhoseBlacklistFlagWasUpdated.AddRange(channels);
        }
    }
}
