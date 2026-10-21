using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class ChannelMetadataServicesTests
    {
        private readonly FakeChannelRepository _fakeChannelRepository = new FakeChannelRepository();
        private readonly FakeVideoMetadataRepository _fakeVideoMetadataRepository = new FakeVideoMetadataRepository();
        private readonly FakeVideoSiteRepository _fakeVideoSiteRepository = new FakeVideoSiteRepository();
        private readonly ChannelMetadataServices _channelMetadataServices;

        public ChannelMetadataServicesTests()
        {
            _channelMetadataServices = new ChannelMetadataServices(_fakeChannelRepository, _fakeVideoMetadataRepository, _fakeVideoSiteRepository);
        }

        private void StoreChannel(string siteId, string name) => _fakeChannelRepository.StoredChannels.Add(new RepositoryChannelDto { SiteId = siteId, Name = name });

        private void StoreVideo(string siteId, string channelSiteId, string title = null, RepositoryVideoDownloadStatus status = RepositoryVideoDownloadStatus.New) =>
            _fakeVideoMetadataRepository.StoredVideos.Add(new RepositoryVideoMetadataDto { SiteId = siteId, ChannelSiteId = channelSiteId, Title = title ?? ("Video " + siteId), DownloadStatus = status });

        [Fact]
        public void GetNewVideosWithChannelsJoinsEachNewVideoWithItsChannel()
        {
            StoreChannel("c1", "One");
            StoreChannel("c2", "Two");
            StoreVideo("v1", "c1");
            StoreVideo("v2", "c2");
            StoreVideo("v3", "c1", status: RepositoryVideoDownloadStatus.ToDownload);

            var videosWithChannels = _channelMetadataServices.GetNewVideosWithChannels(0, 10);

            Assert.Equal(new[] { ("v1", "One"), ("v2", "Two") }, videosWithChannels.Select(item => (item.Video.SiteId, item.Channel.Name)));
        }

        [Fact]
        public void GetNewVideosWithChannelsAsksForTheChannelsOfTheVideosOnlyOnceEach()
        {
            StoreChannel("c1", "One");
            StoreVideo("v1", "c1");
            StoreVideo("v2", "c1");

            _channelMetadataServices.GetNewVideosWithChannels(0, 10);

            Assert.Equal(new[] { "c1" }, Assert.Single(_fakeChannelRepository.SiteIdsRequested));
        }

        [Fact]
        public void GetNewVideosWithChannelsPassesTheSkipAndPageSizeOn()
        {
            _channelMetadataServices.GetNewVideosWithChannels(20, 10);

            Assert.Equal((20, 10), Assert.Single(_fakeVideoMetadataRepository.PagesRequested));
        }

        [Fact]
        public void GetVideosWithChannelsQueuedToDownloadOrWaitingJoinsTheQueuedAndWaitingVideosWithTheirChannels()
        {
            StoreChannel("c1", "One");
            StoreVideo("v1", "c1", status: RepositoryVideoDownloadStatus.ToDownload);
            StoreVideo("v2", "c1", status: RepositoryVideoDownloadStatus.Wait);
            StoreVideo("v3", "c1", status: RepositoryVideoDownloadStatus.Downloaded);

            var videosWithChannels = _channelMetadataServices.GetVideosWithChannelsQueuedToDownloadOrWaiting(0, 10);

            Assert.Equal(new[] { "v1", "v2" }, videosWithChannels.Select(item => item.Video.SiteId));
        }

        [Fact]
        public void GetVideosWithChannelsWhereVideoTitleContainsWordJoinsTheMatchingVideosWithTheirChannels()
        {
            StoreChannel("c1", "One");
            StoreVideo("v1", "c1", title: "a keyword title");
            StoreVideo("v2", "c1", title: "another title");

            var videosWithChannels = _channelMetadataServices.GetVideosWithChannelsWhereVideoTitleContainsWord("keyword", 0, 10);

            Assert.Equal("v1", Assert.Single(videosWithChannels).Video.SiteId);
        }

        [Fact]
        public void AVideoWhoseChannelIsNotStoredThrowsAKeyNotFoundException()
        {
            StoreVideo("v1", "channel-that-is-not-stored");

            Assert.Throws<KeyNotFoundException>(() => _channelMetadataServices.GetNewVideosWithChannels(0, 10));
        }

        [Fact]
        public void GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWordsSearchesTheVideosOfTheChannelsWhoseNameMatches()
        {
            StoreChannel("c1", "Sparkle Channel");
            StoreChannel("c2", "Other");
            StoreVideo("v1", "c1", title: "alpha");
            StoreVideo("v2", "c2", title: "alpha");

            var videosWithChannels = _channelMetadataServices.GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords("Sparkle", "alpha", 0, 10);

            Assert.Equal(new[] { "Sparkle" }, _fakeChannelRepository.WordsInChannelNamesSearched);
            var (channelSiteIds, wordInVideoTitle) = Assert.Single(_fakeVideoMetadataRepository.VideosOfChannelsSearches);
            Assert.Equal(new[] { "c1" }, channelSiteIds);
            Assert.Equal("alpha", wordInVideoTitle);
            var videoWithChannel = Assert.Single(videosWithChannels);
            Assert.Equal(("v1", "Sparkle Channel"), (videoWithChannel.Video.SiteId, videoWithChannel.Channel.Name));
        }

        [Fact]
        public void GetVideoWithChannelByUrlReturnsTheStoredVideoWithItsChannelWithoutFetchingFromTheSite()
        {
            StoreChannel("c1", "One");
            StoreVideo("abc123", "c1");
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => "abc123";

            var videoWithChannel = _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("https://www.youtube.com/watch?v=abc123");

            Assert.Equal("abc123", videoWithChannel.Video.SiteId);
            Assert.Equal("One", videoWithChannel.Channel.Name);
            Assert.Empty(_fakeVideoSiteRepository.VideoSiteIdsFetched);
            Assert.Empty(_fakeVideoMetadataRepository.VideosInserted);
        }

        [Fact]
        public void GetVideoWithChannelByUrlLooksTheStoredVideoUpBySiteIdUsingTheSiteIdOfTheUrl()
        {
            _fakeVideoSiteRepository.SiteIdFromUrl = url => "site-id-from-" + url;
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = _ => new RepositoryVideoWithChannelDto
            {
                Video = new RepositoryVideoMetadataDto { SiteId = "v1", ChannelSiteId = "c1" },
                Channel = new RepositoryChannelDto { SiteId = "c1" },
            };

            _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url");

            Assert.Equal(new[] { "site-id-from-the-url" }, _fakeVideoMetadataRepository.SiteIdSearches);
        }

        [Fact]
        public void GetVideoWithChannelByUrlFetchesAnUnstoredVideoFromTheSiteAndStoresTheVideoAndItsChannel()
        {
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => "v1";
            var fetchedVideo = new RepositoryVideoMetadataDto { SiteId = "v1", Title = "Fetched video", ChannelSiteId = "c1", OriginalResponseFromVideoSite = new object() };
            var fetchedChannel = new RepositoryChannelDto { SiteId = "c1", Name = "Fetched channel", OriginalResponseFromVideoSite = new object() };
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = _ => new RepositoryVideoWithChannelDto { Video = fetchedVideo, Channel = fetchedChannel };

            var videoWithChannel = _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url");

            Assert.Equal(new[] { "v1" }, _fakeVideoSiteRepository.VideoSiteIdsFetched);
            Assert.Same(fetchedVideo, Assert.Single(_fakeVideoMetadataRepository.VideosInserted));
            Assert.Same(fetchedChannel, Assert.Single(_fakeChannelRepository.ChannelsInsertedOrUpdated));
            Assert.Equal("Fetched video", videoWithChannel.Video.Title);
            Assert.Equal("Fetched channel", videoWithChannel.Channel.Name);
        }

        [Fact]
        public void GetVideoWithChannelByUrlThrowsFailedToGetVideoWhenTheSiteReturnsNoVideo()
        {
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = _ => new RepositoryVideoWithChannelDto { Video = null, Channel = null };

            var exception = Assert.Throws<Exception>(() => _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url"));

            Assert.Equal("Failed to get video", exception.Message);
            Assert.Empty(_fakeVideoMetadataRepository.VideosInserted);
            Assert.Empty(_fakeChannelRepository.ChannelsInsertedOrUpdated);
        }

        [Fact]
        public void GetVideoWithChannelByUrlThrowsFailedToGetVideoWhenTheSiteReturnsAVideoButNoChannel()
        {
            _fakeVideoSiteRepository.VideoWithChannelBySiteId = _ => new RepositoryVideoWithChannelDto { Video = new RepositoryVideoMetadataDto { SiteId = "v1" }, Channel = null };

            var exception = Assert.Throws<Exception>(() => _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url"));

            Assert.Equal("Failed to get video", exception.Message);
            Assert.Empty(_fakeVideoMetadataRepository.VideosInserted);
        }

        [Fact]
        public void GetVideoWithChannelByUrlLetsTheUrlFormatExceptionPropagate()
        {
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => throw new Exception("URL format is not recognized. Cannot get site ID.");

            var exception = Assert.Throws<Exception>(() => _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url"));

            Assert.Equal("URL format is not recognized. Cannot get site ID.", exception.Message);
        }

        [Fact]
        public void GetVideoWithChannelByUrlReturnsANullChannelWhenTheStoredVideosChannelIsNotStored()
        {
            StoreVideo("abc123", "channel-that-is-not-stored");
            _fakeVideoSiteRepository.SiteIdFromUrl = _ => "abc123";

            var videoWithChannel = _channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("the-url");

            Assert.Equal("abc123", videoWithChannel.Video.SiteId);
            Assert.Null(videoWithChannel.Channel);
        }
    }
}
