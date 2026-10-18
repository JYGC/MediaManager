using System;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class ChannelMetadataServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly FakeVideoSiteRepository _fakeVideoSiteRepository = new FakeVideoSiteRepository();

        public ChannelMetadataServicesTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        private ChannelMetadataServices CreateChannelMetadataServices() =>
            new ChannelMetadataServices(
                new ChannelRepository(_fixture.DatabaseConnectionOpener),
                new VideoMetadataRepository(_fixture.DatabaseConnectionOpener),
                _fakeVideoSiteRepository);

        [Fact]
        public void GetNewVideosWithChannels_PairsVideoWithItsChannel()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chan = Channel(prefix + "_chan", "ChanName");
            var meta = Metadata(prefix + "_v1", channelSiteId: chan.SiteId, status: MetadataStatus.New);

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { meta });
            });

            var got = CreateChannelMetadataServices().GetNewVideosWithChannels(0, int.MaxValue)
                .Where(cm => cm.Video.SiteId.StartsWith(prefix))
                .ToList();

            Assert.Single(got);
            Assert.Equal(meta.SiteId, got[0].Video.SiteId);
            Assert.Equal(chan.SiteId, got[0].Channel.SiteId);
            Assert.Equal("ChanName", got[0].Channel.Name);
        }

        [Fact]
        public void GetVideosWithChannelsQueuedToDownloadOrWaiting_ReturnsOnlyMatching()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chan = Channel(prefix + "_c");
            var newM = Metadata(prefix + "_new", channelSiteId: chan.SiteId, status: MetadataStatus.New);
            var td = Metadata(prefix + "_td", channelSiteId: chan.SiteId, status: MetadataStatus.ToDownload);
            var wait = Metadata(prefix + "_w", channelSiteId: chan.SiteId, status: MetadataStatus.Wait);

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { newM, td, wait });
            });

            var got = CreateChannelMetadataServices().GetVideosWithChannelsQueuedToDownloadOrWaiting(0, int.MaxValue)
                .Where(cm => cm.Video.SiteId.StartsWith(prefix))
                .ToList();

            Assert.Equal(2, got.Count);
            Assert.All(got, cm => Assert.True(
                cm.Video.DownloadStatus == MediaManager.Dtos.Services.ServiceVideoDownloadStatus.ToDownload ||
                cm.Video.DownloadStatus == MediaManager.Dtos.Services.ServiceVideoDownloadStatus.Wait));
        }

        [Fact]
        public void GetVideosWithChannelsWhereVideoTitleContainsWord_FiltersByTitle()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var keyword = "Sparkle" + prefix;
            var chan = Channel(prefix + "_c");
            var hit = Metadata(prefix + "_a", channelSiteId: chan.SiteId, title: keyword + " 1");
            var miss = Metadata(prefix + "_b", channelSiteId: chan.SiteId, title: "boring");

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { hit, miss });
            });

            var got = CreateChannelMetadataServices().GetVideosWithChannelsWhereVideoTitleContainsWord(keyword, 0, int.MaxValue);

            Assert.Single(got);
            Assert.Equal(hit.SiteId, got[0].Video.SiteId);
        }

        [Fact]
        public void GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords_FiltersOnBoth()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chanWord = "Cookie" + prefix;
            var titleWord = "Recipe" + prefix;
            var matchChan = Channel(prefix + "_mc", chanWord + " Channel");
            var otherChan = Channel(prefix + "_oc", "Other");
            var hit = Metadata(prefix + "_h", channelSiteId: matchChan.SiteId, title: titleWord + " 1");
            var wrongChan = Metadata(prefix + "_wc", channelSiteId: otherChan.SiteId, title: titleWord + " 2");
            var wrongTitle = Metadata(prefix + "_wt", channelSiteId: matchChan.SiteId, title: "no match");

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { matchChan, otherChan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { hit, wrongChan, wrongTitle });
            });

            var got = CreateChannelMetadataServices().GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(chanWord, titleWord, 0, int.MaxValue);

            Assert.Single(got);
            Assert.Equal(hit.SiteId, got[0].Video.SiteId);
        }

        [Fact]
        public void GetVideoWithChannelByUrl_StoresTheVideoAndChannelFetchedFromTheSiteWhenTheVideoIsNotStored()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => prefix + "_fetched";
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = siteId => new RepositoryVideoWithChannelDto
            {
                Video = new RepositoryVideoMetadataDto { SiteId = siteId, Title = "Fetched video", ChannelSiteId = prefix + "_fc", OriginalResponseFromVideoSite = new object() },
                Channel = new RepositoryChannelDto { SiteId = prefix + "_fc", Name = "Fetched channel", IsBlacklisted = true, IsAddedBySingleVideo = true },
            };

            var videoWithChannel = CreateChannelMetadataServices().GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("https://www.youtube.com/watch?v=any");

            Assert.Equal("Fetched video", videoWithChannel.Video.Title);
            Assert.Equal("Fetched channel", videoWithChannel.Channel.Name);
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                Assert.Equal("Fetched video", db.YoutubeMetadataDbCollection.GetBySiteId(prefix + "_fetched").Title);
                var storedChannel = db.YoutubeChannelDbCollection.GetBySiteId(prefix + "_fc");
                Assert.Equal("Fetched channel", storedChannel.Name);
                Assert.True(storedChannel.Blacklisted);
                Assert.True(storedChannel.IsAddedBySingleVideo);
            });
        }

        [Fact]
        public void GetVideoWithChannelByUrl_ReturnsTheStoredVideoWithoutFetchingFromTheSite()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chan = Channel(prefix + "_c", "Stored channel");
            var meta = Metadata(prefix + "_stored", channelSiteId: chan.SiteId);
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { chan });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { meta });
            });
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => meta.SiteId;
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = _ => throw new InvalidOperationException("the site must not be asked for a stored video");

            var videoWithChannel = CreateChannelMetadataServices().GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("https://www.youtube.com/watch?v=any");

            Assert.Equal(meta.SiteId, videoWithChannel.Video.SiteId);
            Assert.Equal("Stored channel", videoWithChannel.Channel.Name);
        }
    }
}
