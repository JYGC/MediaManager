using System;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.UnitTests.Support;
using Xunit;
using static OPMF.UnitTests.Support.DatabaseTestData;

namespace OPMF.UnitTests.Repositories
{
    public class ChannelRepositoryTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly ChannelRepository _channelRepository;

        public ChannelRepositoryTests()
        {
            _channelRepository = new ChannelRepository(_databaseConnectionOpener);
        }

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private void StoreChannels(params OPMF.Entities.Channel[] channels)
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(channels));
        }

        private OPMF.Entities.Channel StoredChannelWithSiteId(string siteId)
        {
            OPMF.Entities.Channel storedChannel = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => storedChannel = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId(siteId));
            return storedChannel;
        }

        [Fact]
        public void GetAllChannelsReturnsEveryStoredChannelAsADto()
        {
            StoreChannels(Channel("c1", "One"), Channel("c2", "Two"));

            var channels = _channelRepository.GetAllChannels();

            Assert.Equal(new[] { "One", "Two" }, channels.Select(channel => channel.Name).OrderBy(name => name));
        }

        [Fact]
        public void GetChannelsThatAreNotBlacklistedOmitsTheBlacklistedOnes()
        {
            StoreChannels(Channel("keep", blacklisted: false), Channel("skip", blacklisted: true));

            var channels = _channelRepository.GetChannelsThatAreNotBlacklisted();

            Assert.Equal("keep", Assert.Single(channels).SiteId);
        }

        [Fact]
        public void GetChannelsWhoseNameContainsMatchesTheWordInTheName()
        {
            StoreChannels(Channel("h1", "Sparkle Channel"), Channel("h2", "More Sparkle"), Channel("m1", "Nothing here"));

            var channels = _channelRepository.GetChannelsWhoseNameContains("Sparkle");

            Assert.Equal(new[] { "h1", "h2" }, channels.Select(channel => channel.SiteId).OrderBy(siteId => siteId));
        }

        [Fact]
        public void GetChannelsWithSiteIdsReturnsOnlyTheRequestedChannels()
        {
            StoreChannels(Channel("g1"), Channel("g2"), Channel("g3"));

            var channels = _channelRepository.GetChannelsWithSiteIds(new[] { "g1", "g3" });

            Assert.Equal(new[] { "g1", "g3" }, channels.Select(channel => channel.SiteId).OrderBy(siteId => siteId));
        }

        [Fact]
        public void GetChannelWithSiteIdReturnsTheChannelAsADto()
        {
            StoreChannels(Channel("c1", "One"));

            var channel = _channelRepository.GetChannelWithSiteId("c1");

            Assert.Equal("One", channel.Name);
        }

        [Fact]
        public void GetChannelWithSiteIdReturnsNullWhenNoChannelIsStoredWithThatSiteId()
        {
            Assert.Null(_channelRepository.GetChannelWithSiteId("not-stored"));
        }

        [Fact]
        public void InsertNewChannelsInsertsChannelsThatAreNotStoredYet()
        {
            var channelDto = new RepositoryChannelDto { SiteId = "new-1", Name = "Brand new", Url = "https://example/new-1", Description = "About it" };

            _channelRepository.InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(new[] { channelDto });

            var storedChannel = StoredChannelWithSiteId("new-1");
            Assert.Equal("Brand new", storedChannel.Name);
            Assert.Equal("https://example/new-1", storedChannel.Url);
            Assert.Equal("About it", storedChannel.Description);
        }

        [Fact]
        public void InsertNewChannelsUpdatesTheNameUrlThumbnailAndDescriptionOfAnExistingChannel()
        {
            StoreChannels(Channel("c1", "Original name"));
            var channelDto = new RepositoryChannelDto
            {
                SiteId = "c1",
                Name = "Updated name",
                Url = "https://example/updated",
                Description = "Updated description",
                Thumbnail = new RepositoryThumbnailDto { Url = "https://example/thumbnail", WidthInPixels = 50, HeightInPixels = 40 },
            };

            _channelRepository.InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(new[] { channelDto });

            var storedChannel = StoredChannelWithSiteId("c1");
            Assert.Equal("Updated name", storedChannel.Name);
            Assert.Equal("https://example/updated", storedChannel.Url);
            Assert.Equal("Updated description", storedChannel.Description);
            Assert.Equal("https://example/thumbnail", storedChannel.Thumbnail.Url);
            Assert.Equal(50, storedChannel.Thumbnail.Width);
            Assert.Equal(40, storedChannel.Thumbnail.Height);
        }

        [Fact]
        public void InsertNewChannelsDoesNotChangeTheBlacklistFlagOfAnExistingChannel()
        {
            StoreChannels(Channel("c1", blacklisted: true));

            _channelRepository.InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(new[] { new RepositoryChannelDto { SiteId = "c1", IsBlacklisted = false } });

            Assert.True(StoredChannelWithSiteId("c1").Blacklisted);
        }

        [Fact]
        public void UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtPersistsBothDates()
        {
            StoreChannels(Channel("c1"));
            var channelDto = _channelRepository.GetChannelWithSiteId("c1");
            channelDto.LastCheckedForNewVideosAt = new DateTime(2026, 10, 1);
            channelDto.LatestVideoPublishedAt = new DateTime(2026, 9, 30);

            _channelRepository.UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(new[] { channelDto });

            var storedChannel = StoredChannelWithSiteId("c1");
            Assert.Equal(new DateTime(2026, 10, 1), storedChannel.LastCheckedOut);
            Assert.Equal(new DateTime(2026, 9, 30), storedChannel.LastActivityDate);
        }

        [Fact]
        public void UpdateBlacklistFlagOfChannelsPersistsTheFlag()
        {
            StoreChannels(Channel("c1", blacklisted: false));
            var channelDto = _channelRepository.GetChannelWithSiteId("c1");
            channelDto.IsBlacklisted = true;

            _channelRepository.UpdateBlacklistFlagOfChannels(new[] { channelDto });

            Assert.True(StoredChannelWithSiteId("c1").Blacklisted);
        }
    }
}
