using System;
using System.Linq;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class ChannelServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;

        public ChannelServicesTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        private ChannelServices CreateChannelServices() =>
            new ChannelServices(new ChannelRepository(_fixture.DatabaseConnectionOpener));

        [Fact]
        public void GetAllChannels_ReturnsAllStoredChannels()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var a = Channel(prefix + "_a", "A");
            var b = Channel(prefix + "_b", "B");

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { a, b }));

            var got = CreateChannelServices().GetAllChannels().Where(c => c.SiteId.StartsWith(prefix)).ToList();

            Assert.Equal(2, got.Count);
            Assert.Contains(got, c => c.Name == "A");
            Assert.Contains(got, c => c.Name == "B");
        }

        [Fact]
        public void UpdateBlacklistFlagOfChannels_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var c = Channel(prefix + "_bl", blacklisted: false);

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { c }));

            var svc = CreateChannelServices();
            var fromSvc = svc.GetAllChannels().First(x => x.SiteId == c.SiteId);
            fromSvc.IsBlacklisted = true;
            svc.UpdateBlacklistFlagOfChannels(new[] { fromSvc });

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                Assert.True(db.YoutubeChannelDbCollection.GetBySiteId(c.SiteId).Blacklisted);
            });
        }
    }
}
