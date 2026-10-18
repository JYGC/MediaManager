using System;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class ChannelServicesTests
    {
        private readonly FakeChannelRepository _fakeChannelRepository = new FakeChannelRepository();
        private readonly ChannelServices _channelServices;

        public ChannelServicesTests()
        {
            _channelServices = new ChannelServices(_fakeChannelRepository);
        }

        [Fact]
        public void GetAllChannelsReturnsTheRepositorysChannelsAsServiceDtos()
        {
            _fakeChannelRepository.StoredChannels.Add(new RepositoryChannelDto { SiteId = "c1", Name = "One", IsBlacklisted = true });
            _fakeChannelRepository.StoredChannels.Add(new RepositoryChannelDto { SiteId = "c2", Name = "Two" });

            var channels = _channelServices.GetAllChannels();

            Assert.Equal(new[] { "One", "Two" }, channels.Select(channel => channel.Name));
            Assert.Equal(new[] { true, false }, channels.Select(channel => channel.IsBlacklisted));
        }

        [Fact]
        public void UpdateBlacklistFlagOfChannelsPassesTheChannelsToTheRepositoryAsRepositoryDtos()
        {
            var channelsToUpdate = new[]
            {
                new ServiceChannelDto { SiteId = "c1", IsBlacklisted = true },
                new ServiceChannelDto { SiteId = "c2", IsBlacklisted = false },
            };

            _channelServices.UpdateBlacklistFlagOfChannels(channelsToUpdate);

            Assert.Equal(new[] { "c1", "c2" }, _fakeChannelRepository.ChannelsWhoseBlacklistFlagWasUpdated.Select(channel => channel.SiteId));
            Assert.Equal(new[] { true, false }, _fakeChannelRepository.ChannelsWhoseBlacklistFlagWasUpdated.Select(channel => channel.IsBlacklisted));
        }
    }
}
