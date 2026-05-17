using System;
using System.Linq;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Database
{
    public class YoutubeChannelDbCollectionInsertOrUpdateTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeChannelDbCollectionInsertOrUpdateTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void InsertOrUpdate_InsertsNewChannels()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c1 = Channel(prefix + "_c1", "Name One");
            var c2 = Channel(prefix + "_c2", "Name Two");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c1, c2 });
                Assert.Equal("Name One", db.YoutubeChannelDbCollection.GetBySiteId(c1.SiteId).Name);
                Assert.Equal("Name Two", db.YoutubeChannelDbCollection.GetBySiteId(c2.SiteId).Name);
            });
        }

        [Fact]
        public void InsertOrUpdate_UpdatesExistingChannelFields()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var original = Channel(prefix + "_upd", "Original Name");
            var updated = Channel(prefix + "_upd", "Updated Name");
            updated.Description = "new desc";

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { original });
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { updated });

                var stored = db.YoutubeChannelDbCollection.GetBySiteId(updated.SiteId);
                Assert.Equal("Updated Name", stored.Name);
                Assert.Equal("new desc", stored.Description);
            });
        }
    }

    public class YoutubeChannelDbCollectionQueryTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeChannelDbCollectionQueryTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetNotBacklisted_OmitsBlacklisted()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var keep = Channel(prefix + "_keep", blacklisted: false);
            var skip = Channel(prefix + "_skip", blacklisted: true);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { keep, skip });
                var got = db.YoutubeChannelDbCollection.GetNotBacklisted()
                    .Where(c => c.SiteId.StartsWith(prefix)).ToList();
                Assert.Single(got);
                Assert.Equal(keep.SiteId, got[0].SiteId);
            });
        }

        [Fact]
        public void GetManyBySiteIds_ReturnsRequestedSubset()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c1 = Channel(prefix + "_g1");
            var c2 = Channel(prefix + "_g2");
            var c3 = Channel(prefix + "_g3");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c1, c2, c3 });
                var got = db.YoutubeChannelDbCollection.GetManyBySiteIds(new[] { c1.SiteId, c3.SiteId });
                Assert.Equal(2, got.Count);
                Assert.Contains(got, c => c.SiteId == c1.SiteId);
                Assert.Contains(got, c => c.SiteId == c3.SiteId);
            });
        }

        [Fact]
        public void GetManyByWordInName_FiltersByNameSubstring()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var hit1 = Channel(prefix + "_h1", "Sparkle Channel");
            var hit2 = Channel(prefix + "_h2", "More Sparkle");
            var miss = Channel(prefix + "_m1", "Nothing here");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { hit1, hit2, miss });
                var got = db.YoutubeChannelDbCollection.GetManyByWordInName("Sparkle")
                    .Where(c => c.SiteId.StartsWith(prefix)).ToList();
                Assert.Equal(2, got.Count);
            });
        }
    }

    public class YoutubeChannelDbCollectionUpdateTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeChannelDbCollectionUpdateTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void UpdateLastCheckedOutAndActivity_PersistsDates()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c = Channel(prefix + "_t");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c });

                var stored = db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId);
                stored.LastCheckedOut = new DateTime(2024, 5, 1);
                stored.LastActivityDate = new DateTime(2024, 4, 1);
                db.YoutubeChannelDbCollection.UpdateLastCheckedOutAndActivity(new[] { stored });

                var refetched = db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId);
                Assert.Equal(new DateTime(2024, 5, 1), refetched.LastCheckedOut);
                Assert.Equal(new DateTime(2024, 4, 1), refetched.LastActivityDate);
            });
        }

        [Fact]
        public void UpdateBlackListStatus_PersistsFlag()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c = Channel(prefix + "_bl", blacklisted: false);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c });

                var stored = db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId);
                stored.Blacklisted = true;
                db.YoutubeChannelDbCollection.UpdateBlackListStatus(new[] { stored });

                Assert.True(db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId).Blacklisted);
            });
        }
    }
}
