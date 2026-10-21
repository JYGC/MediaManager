using System;
using System.Linq;
using OPMF.UnitTests.Support;
using Xunit;
using static OPMF.UnitTests.Support.DatabaseTestData;

namespace OPMF.UnitTests.Database
{
    public class YoutubeChannelDbCollectionOnAnInMemoryDatabaseTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();

        public void Dispose() => _databaseConnectionOpener.Dispose();

        [Fact]
        public void InsertOrUpdateInsertsNewChannels()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("c1", "Name One"), Channel("c2", "Name Two") });

                Assert.Equal("Name One", databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("c1").Name);
                Assert.Equal("Name Two", databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("c2").Name);
            });
        }

        [Fact]
        public void InsertOrUpdateUpdatesTheFieldsOfAnExistingChannel()
        {
            var updatedChannel = Channel("upd", "Updated Name");
            updatedChannel.Description = "new desc";

            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("upd", "Original Name") });
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { updatedChannel });

                var storedChannel = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("upd");
                Assert.Equal("Updated Name", storedChannel.Name);
                Assert.Equal("new desc", storedChannel.Description);
            });
        }

        [Fact]
        public void GetNotBacklistedOmitsBlacklistedChannels()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("keep", blacklisted: false), Channel("skip", blacklisted: true) });

                var channels = databaseAdapter.YoutubeChannelDbCollection.GetNotBacklisted().ToList();

                Assert.Equal("keep", Assert.Single(channels).SiteId);
            });
        }

        [Fact]
        public void GetManyBySiteIdsReturnsTheRequestedSubset()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("g1"), Channel("g2"), Channel("g3") });

                var channels = databaseAdapter.YoutubeChannelDbCollection.GetManyBySiteIds(new[] { "g1", "g3" });

                Assert.Equal(new[] { "g1", "g3" }, channels.Select(channel => channel.SiteId).OrderBy(siteId => siteId));
            });
        }

        [Fact]
        public void GetManyByWordInNameFiltersByNameSubstring()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("h1", "Sparkle Channel"), Channel("h2", "More Sparkle"), Channel("m1", "Nothing here") });

                var channels = databaseAdapter.YoutubeChannelDbCollection.GetManyByWordInName("Sparkle").ToList();

                Assert.Equal(2, channels.Count);
            });
        }

        [Fact]
        public void UpdateLastCheckedOutAndActivityPersistsBothDates()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("t") });
                var storedChannel = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("t");
                storedChannel.LastCheckedOut = new DateTime(2024, 5, 1);
                storedChannel.LastActivityDate = new DateTime(2024, 4, 1);

                databaseAdapter.YoutubeChannelDbCollection.UpdateLastCheckedOutAndActivity(new[] { storedChannel });

                var channelReadAgain = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("t");
                Assert.Equal(new DateTime(2024, 5, 1), channelReadAgain.LastCheckedOut);
                Assert.Equal(new DateTime(2024, 4, 1), channelReadAgain.LastActivityDate);
            });
        }

        [Fact]
        public void UpdateBlackListStatusPersistsTheFlag()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel("bl", blacklisted: false) });
                var storedChannel = databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("bl");
                storedChannel.Blacklisted = true;

                databaseAdapter.YoutubeChannelDbCollection.UpdateBlackListStatus(new[] { storedChannel });

                Assert.True(databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("bl").Blacklisted);
            });
        }
    }
}
