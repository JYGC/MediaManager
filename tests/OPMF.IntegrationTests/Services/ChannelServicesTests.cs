using System;
using System.Linq;
using MediaManager.Services2;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class ChannelServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public ChannelServicesTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetAll_ReturnsAllStoredChannels()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var a = Channel(prefix + "_a", "A");
            var b = Channel(prefix + "_b", "B");

            DatabaseAdapter.AccessDbAdapter(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { a, b }));

            var svc = new ChannelServices();
            var got = svc.GetAll().Where(c => c.SiteId.StartsWith(prefix)).ToList();

            Assert.Equal(2, got.Count);
            Assert.Contains(got, c => c.Name == "A");
            Assert.Contains(got, c => c.Name == "B");
        }

        [Fact]
        public void UpdateBlackListStatus_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c = Channel(prefix + "_bl", blacklisted: false);

            DatabaseAdapter.AccessDbAdapter(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c }));

            var svc = new ChannelServices();
            var fromSvc = svc.GetAll().First(x => x.SiteId == c.SiteId);
            fromSvc.Blacklisted = true;
            svc.UpdateBlackListStatus(new[] { fromSvc });

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                Assert.True(db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId).Blacklisted);
            });
        }
    }
}
