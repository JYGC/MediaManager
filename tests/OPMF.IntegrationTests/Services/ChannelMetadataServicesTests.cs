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
    public class ChannelMetadataServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public ChannelMetadataServicesTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetNew_PairsMetadataWithItsChannel()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chan = Channel(prefix + "_chan", "ChanName");
            var meta = Metadata(prefix + "_v1", channelSiteId: chan.SiteId, status: MetadataStatus.New);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { meta });
            });

            var svc = new ChannelMetadataServices();
            var got = svc.GetNew(0, int.MaxValue)
                .Where(cm => cm.Metadata.SiteId.StartsWith(prefix))
                .ToList();

            Assert.Single(got);
            Assert.Equal(meta.SiteId, got[0].Metadata.SiteId);
            Assert.Equal(chan.SiteId, got[0].Channel.SiteId);
            Assert.Equal("ChanName", got[0].Channel.Name);
        }

        [Fact]
        public void GetToDownloadAndWait_ReturnsOnlyMatching()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chan = Channel(prefix + "_c");
            var newM = Metadata(prefix + "_new", channelSiteId: chan.SiteId, status: MetadataStatus.New);
            var td = Metadata(prefix + "_td", channelSiteId: chan.SiteId, status: MetadataStatus.ToDownload);
            var wait = Metadata(prefix + "_w", channelSiteId: chan.SiteId, status: MetadataStatus.Wait);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { newM, td, wait });
            });

            var got = new ChannelMetadataServices()
                .GetToDownloadAndWait(0, int.MaxValue)
                .Where(cm => cm.Metadata.SiteId.StartsWith(prefix))
                .ToList();

            Assert.Equal(2, got.Count);
            Assert.All(got, cm => Assert.True(
                cm.Metadata.Status == MetadataStatus.ToDownload ||
                cm.Metadata.Status == MetadataStatus.Wait));
        }

        [Fact]
        public void GetByTitleContainingWord_FiltersByTitle()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var keyword = "Sparkle" + prefix;
            var chan = Channel(prefix + "_c");
            var hit = Metadata(prefix + "_a", channelSiteId: chan.SiteId, title: keyword + " 1");
            var miss = Metadata(prefix + "_b", channelSiteId: chan.SiteId, title: "boring");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { hit, miss });
            });

            var got = new ChannelMetadataServices()
                .GetByTitleContainingWord(keyword, 0, int.MaxValue);

            Assert.Single(got);
            Assert.Equal(hit.SiteId, got[0].Metadata.SiteId);
        }

        [Fact]
        public void GetByChannelAndTitleContainingWord_FiltersOnBoth()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chanWord = "Cookie" + prefix;
            var titleWord = "Recipe" + prefix;
            var matchChan = Channel(prefix + "_mc", chanWord + " Channel");
            var otherChan = Channel(prefix + "_oc", "Other");
            var hit = Metadata(prefix + "_h", channelSiteId: matchChan.SiteId, title: titleWord + " 1");
            var wrongChan = Metadata(prefix + "_wc", channelSiteId: otherChan.SiteId, title: titleWord + " 2");
            var wrongTitle = Metadata(prefix + "_wt", channelSiteId: matchChan.SiteId, title: "no match");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { matchChan, otherChan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { hit, wrongChan, wrongTitle });
            });

            var got = new ChannelMetadataServices()
                .GetByChannelAndTitleContainingWord(chanWord, titleWord, 0, int.MaxValue);

            Assert.Single(got);
            Assert.Equal(hit.SiteId, got[0].Metadata.SiteId);
        }
    }
}
