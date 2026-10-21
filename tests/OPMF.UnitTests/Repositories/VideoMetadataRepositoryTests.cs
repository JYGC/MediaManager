using System;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Entities;
using OPMF.UnitTests.Support;
using Xunit;
using static OPMF.UnitTests.Support.DatabaseTestData;

namespace OPMF.UnitTests.Repositories
{
    public class VideoMetadataRepositoryTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly VideoMetadataRepository _videoMetadataRepository;

        public VideoMetadataRepositoryTests()
        {
            _videoMetadataRepository = new VideoMetadataRepository(_databaseConnectionOpener);
        }

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private void StoreVideos(params Metadata[] videos)
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => databaseAdapter.YoutubeMetadataDbCollection.InsertNew(videos));
        }

        private Metadata StoredVideoWithSiteId(string siteId)
        {
            Metadata storedVideo = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => storedVideo = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId(siteId));
            return storedVideo;
        }

        [Fact]
        public void GetVideosQueuedToDownloadReturnsOnlyVideosWithTheToDownloadStatus()
        {
            StoreVideos(Metadata("n", status: MetadataStatus.New), Metadata("td1", status: MetadataStatus.ToDownload), Metadata("td2", status: MetadataStatus.ToDownload), Metadata("w", status: MetadataStatus.Wait));

            var videos = _videoMetadataRepository.GetVideosQueuedToDownload();

            Assert.Equal(new[] { "td1", "td2" }, videos.Select(video => video.SiteId).OrderBy(siteId => siteId));
            Assert.All(videos, video => Assert.Equal(RepositoryVideoDownloadStatus.ToDownload, video.DownloadStatus));
        }

        [Fact]
        public void GetFirstVideoWhoseSiteIdContainsMatchesTheWholeSiteId()
        {
            StoreVideos(Metadata("abc123"), Metadata("other"));

            var video = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("abc123");

            Assert.Equal("abc123", video.SiteId);
        }

        [Fact]
        public void GetFirstVideoWhoseSiteIdContainsMatchesASubstringOfTheSiteId()
        {
            StoreVideos(Metadata("abc123xyz"), Metadata("other"));

            var video = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("c123x");

            Assert.Equal("abc123xyz", video.SiteId);
        }

        [Fact]
        public void GetFirstVideoWhoseSiteIdContainsReturnsNullWhenNoSiteIdMatches()
        {
            StoreVideos(Metadata("abc123"));

            Assert.Null(_videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("zzz"));
        }

        [Fact]
        public void GetNewVideosReturnsOnlyNewVideosOfTheRequestedPage()
        {
            StoreVideos(Metadata("n1"), Metadata("n2"), Metadata("n3"), Metadata("td", status: MetadataStatus.ToDownload));

            var firstTwo = _videoMetadataRepository.GetNewVideos(0, 2);
            var afterTheFirstTwo = _videoMetadataRepository.GetNewVideos(2, 2);

            Assert.Equal(2, firstTwo.Count);
            Assert.Single(afterTheFirstTwo);
            Assert.All(firstTwo.Concat(afterTheFirstTwo), video => Assert.Equal(RepositoryVideoDownloadStatus.New, video.DownloadStatus));
        }

        [Fact]
        public void GetVideosQueuedToDownloadOrWaitingReturnsVideosWithEitherStatus()
        {
            StoreVideos(Metadata("n", status: MetadataStatus.New), Metadata("td", status: MetadataStatus.ToDownload), Metadata("w", status: MetadataStatus.Wait), Metadata("d", status: MetadataStatus.Downloaded));

            var videos = _videoMetadataRepository.GetVideosQueuedToDownloadOrWaiting(0, int.MaxValue);

            Assert.Equal(new[] { "td", "w" }, videos.Select(video => video.SiteId).OrderBy(siteId => siteId));
        }

        [Fact]
        public void GetVideosWhoseTitleContainsReturnsMatchingVideosNewestFirst()
        {
            StoreVideos(
                Metadata("a", title: "keyword older", publishedAt: new DateTime(2020, 1, 1)),
                Metadata("b", title: "keyword newer", publishedAt: new DateTime(2024, 1, 1)),
                Metadata("c", title: "no match", publishedAt: new DateTime(2025, 1, 1)));

            var videos = _videoMetadataRepository.GetVideosWhoseTitleContains("keyword", 0, int.MaxValue);

            Assert.Equal(new[] { "b", "a" }, videos.Select(video => video.SiteId));
        }

        [Fact]
        public void GetVideosOfChannelsWhoseTitleContainsFiltersByChannelAndTitle()
        {
            StoreVideos(
                Metadata("a1", channelSiteId: "chA", title: "alpha"),
                Metadata("a2", channelSiteId: "chA", title: "beta"),
                Metadata("b1", channelSiteId: "chB", title: "alpha"));

            var videos = _videoMetadataRepository.GetVideosOfChannelsWhoseTitleContains(new[] { "chA" }, "alpha", 0, int.MaxValue);

            Assert.Equal("a1", Assert.Single(videos).SiteId);
        }

        [Fact]
        public void InsertVideosThatAreNotStoredYetInsertsTheVideosAndSkipsStoredOnes()
        {
            StoreVideos(Metadata("stored", title: "Original title"));
            var storedVideoDto = new RepositoryVideoMetadataDto { SiteId = "stored", Title = "Should not win" };
            var newVideoDto = new RepositoryVideoMetadataDto { SiteId = "new-1", Title = "Brand new", Url = "https://example/new-1", ChannelSiteId = "c1" };

            _videoMetadataRepository.InsertVideosThatAreNotStoredYet(new[] { storedVideoDto, newVideoDto });

            Assert.Equal("Original title", StoredVideoWithSiteId("stored").Title);
            var insertedVideo = StoredVideoWithSiteId("new-1");
            Assert.Equal("Brand new", insertedVideo.Title);
            Assert.Equal("c1", insertedVideo.ChannelSiteId);
        }

        [Fact]
        public void UpdateDownloadStatusOfVideosPersistsTheStatus()
        {
            StoreVideos(Metadata("a"));
            var videoDto = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("a");
            videoDto.DownloadStatus = RepositoryVideoDownloadStatus.Downloaded;

            _videoMetadataRepository.UpdateDownloadStatusOfVideos(new[] { videoDto });

            Assert.Equal(MetadataStatus.Downloaded, StoredVideoWithSiteId("a").Status);
        }

        [Fact]
        public void SetBeingDownloadedFlagOfVideosSetsAndClearsTheFlag()
        {
            StoreVideos(Metadata("a"));
            var videoDto = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("a");

            _videoMetadataRepository.SetBeingDownloadedFlagOfVideos(new[] { videoDto }, true);
            Assert.True(StoredVideoWithSiteId("a").IsBeingDownloaded);

            _videoMetadataRepository.SetBeingDownloadedFlagOfVideos(new[] { videoDto }, false);
            Assert.False(StoredVideoWithSiteId("a").IsBeingDownloaded);
        }

        [Fact]
        public void UpdateDownloadQualityChoiceOverridesOfVideoStoresTheThreeOverridesOfTheGivenVideoOnly()
        {
            StoreVideos(Metadata("q", title: "Stored title", status: MetadataStatus.ToDownload), Metadata("other"));
            var videoDtoWithOverrides = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("q");
            videoDtoWithOverrides.Title = "Changed title";
            videoDtoWithOverrides.VideoCodecChoiceOverride = RepositoryVideoCodecChoice.AnyCodec;
            videoDtoWithOverrides.MaximumVideoResolutionChoiceOverride = RepositoryMaximumVideoResolutionChoice.UpTo720p;
            videoDtoWithOverrides.AudioQualityChoiceOverride = RepositoryAudioQualityChoice.SmallestAac;

            _videoMetadataRepository.UpdateDownloadQualityChoiceOverridesOfVideo(videoDtoWithOverrides);

            var storedVideo = StoredVideoWithSiteId("q");
            Assert.Equal(EntityVideoCodecChoice.AnyCodec, storedVideo.VideoCodecChoiceOverride);
            Assert.Equal(EntityMaximumVideoResolutionChoice.UpTo720p, storedVideo.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(EntityAudioQualityChoice.SmallestAac, storedVideo.AudioQualityChoiceOverride);
            Assert.Equal("Stored title", storedVideo.Title);
            Assert.Null(StoredVideoWithSiteId("other").VideoCodecChoiceOverride);
        }

        [Fact]
        public void GetFirstVideoWhoseSiteIdContainsAndGetVideosQueuedToDownloadReturnTheOverrides()
        {
            var queuedVideoWithOverrides = Metadata("o", status: MetadataStatus.ToDownload);
            queuedVideoWithOverrides.VideoCodecChoiceOverride = EntityVideoCodecChoice.H264;
            queuedVideoWithOverrides.MaximumVideoResolutionChoiceOverride = EntityMaximumVideoResolutionChoice.UpTo480p;
            queuedVideoWithOverrides.AudioQualityChoiceOverride = EntityAudioQualityChoice.BestOfAnyCodec;
            StoreVideos(queuedVideoWithOverrides);

            var videoFoundBySiteId = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains("o");
            var queuedVideo = Assert.Single(_videoMetadataRepository.GetVideosQueuedToDownload());

            Assert.Equal(RepositoryVideoCodecChoice.H264, videoFoundBySiteId.VideoCodecChoiceOverride);
            Assert.Equal(RepositoryMaximumVideoResolutionChoice.UpTo480p, videoFoundBySiteId.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(RepositoryAudioQualityChoice.BestOfAnyCodec, videoFoundBySiteId.AudioQualityChoiceOverride);
            Assert.Equal(RepositoryVideoCodecChoice.H264, queuedVideo.VideoCodecChoiceOverride);
            Assert.Equal(RepositoryMaximumVideoResolutionChoice.UpTo480p, queuedVideo.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(RepositoryAudioQualityChoice.BestOfAnyCodec, queuedVideo.AudioQualityChoiceOverride);
        }
    }
}
