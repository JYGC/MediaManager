using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Entities;
using OPMF.SiteAdapter;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Repositories
{
    public class VideoSiteRepositoryTests
    {
        private readonly FakeMetadataFetcher _fakeMetadataFetcher = new FakeMetadataFetcher();
        private readonly FakeSiteVideoMetadataGetter _fakeSiteVideoMetadataGetter = new FakeSiteVideoMetadataGetter();
        private int _metadataFetchersCreated;
        private int _videoMetadataGettersCreated;
        private Exception _exceptionTheNextMetadataFetcherCreationThrows;
        private Exception _exceptionTheNextVideoMetadataGetterCreationThrows;

        private VideoSiteRepository CreateVideoSiteRepository()
        {
            return new VideoSiteRepository(
                () =>
                {
                    _metadataFetchersCreated++;
                    ThrowAndForget(ref _exceptionTheNextMetadataFetcherCreationThrows);
                    return _fakeMetadataFetcher;
                },
                () =>
                {
                    _videoMetadataGettersCreated++;
                    ThrowAndForget(ref _exceptionTheNextVideoMetadataGetterCreationThrows);
                    return _fakeSiteVideoMetadataGetter;
                });
        }

        private static void ThrowAndForget(ref Exception exceptionToThrow)
        {
            var exception = exceptionToThrow;
            exceptionToThrow = null;
            if (exception != null)
            {
                throw exception;
            }
        }

        private static RepositoryChannelDto ChannelDto(string siteId) => new RepositoryChannelDto { SiteId = siteId, Name = "Channel " + siteId };

        [Fact]
        public void NothingIsCreatedWhenTheRepositoryIsCreated()
        {
            CreateVideoSiteRepository();

            Assert.Equal(0, _metadataFetchersCreated);
            Assert.Equal(0, _videoMetadataGettersCreated);
        }

        [Fact]
        public void TheMetadataFetcherIsCreatedOnFirstUseOnly()
        {
            var videoSiteRepository = CreateVideoSiteRepository();

            videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto> { ChannelDto("c1") });
            videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto> { ChannelDto("c1") });

            Assert.Equal(1, _metadataFetchersCreated);
            Assert.Equal(0, _videoMetadataGettersCreated);
        }

        [Fact]
        public void TheMetadataFetcherIsCreatedAgainAfterItsCreationThrew()
        {
            _exceptionTheNextMetadataFetcherCreationThrows = new InvalidOperationException("the sign in failed");
            var videoSiteRepository = CreateVideoSiteRepository();

            Assert.Throws<InvalidOperationException>(() => videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto>()));
            videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto>());
            videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto>());

            Assert.Equal(2, _metadataFetchersCreated);
        }

        [Fact]
        public void TheVideoMetadataGetterIsCreatedOnFirstUseOnly()
        {
            var videoSiteRepository = CreateVideoSiteRepository();

            videoSiteRepository.GetVideoSiteIdFromVideoUrl("https://www.youtube.com/watch?v=a");
            videoSiteRepository.FetchVideoWithItsChannelFromVideoSite("a");

            Assert.Equal(1, _videoMetadataGettersCreated);
            Assert.Equal(0, _metadataFetchersCreated);
        }

        [Fact]
        public void TheVideoMetadataGetterIsCreatedAgainAfterItsCreationThrew()
        {
            _exceptionTheNextVideoMetadataGetterCreationThrows = new InvalidOperationException("the sign in failed");
            var videoSiteRepository = CreateVideoSiteRepository();

            Assert.Throws<InvalidOperationException>(() => videoSiteRepository.GetVideoSiteIdFromVideoUrl("url"));
            videoSiteRepository.GetVideoSiteIdFromVideoUrl("url");

            Assert.Equal(2, _videoMetadataGettersCreated);
        }

        [Fact]
        public void TheChannelDtosAreHandedToTheMetadataFetcherAsYoutubeChannels()
        {
            var videoSiteRepository = CreateVideoSiteRepository();

            videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto> { ChannelDto("c1"), ChannelDto("c2") });

            Assert.All(_fakeMetadataFetcher.ChannelsReceived, channel => Assert.IsType<YoutubeChannel>(channel));
            Assert.Equal(new[] { "c1", "c2" }, _fakeMetadataFetcher.ChannelsReceived.Select(channel => channel.SiteId));
        }

        [Fact]
        public void TheResultHoldsTheChannelsWithTheDatesTheMetadataFetcherUpdated()
        {
            _fakeMetadataFetcher.ChangeEachChannelWhenFetching = channel =>
            {
                channel.LastCheckedOut = new DateTime(2026, 10, 18);
                channel.LastActivityDate = new DateTime(2026, 10, 17);
            };
            var videoSiteRepository = CreateVideoSiteRepository();

            var result = videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto> { ChannelDto("c1") });

            var channelWithUpdatedDates = Assert.Single(result.ChannelsWithUpdatedCheckDates);
            Assert.Equal("c1", channelWithUpdatedDates.SiteId);
            Assert.Equal(new DateTime(2026, 10, 18), channelWithUpdatedDates.LastCheckedForNewVideosAt);
            Assert.Equal(new DateTime(2026, 10, 17), channelWithUpdatedDates.LatestVideoPublishedAt);
        }

        [Fact]
        public void TheResultHoldsTheNewlyFetchedVideosAsDtos()
        {
            _fakeMetadataFetcher.VideosToReturn = new List<Metadata>
            {
                new YoutubeMetadata { SiteId = "v1", Title = "First", Thumbnail = new EntityThumbnail(), Status = MetadataStatus.New },
                new YoutubeMetadata { SiteId = "v2", Title = "Second", Thumbnail = new EntityThumbnail(), Status = MetadataStatus.New },
            };
            var videoSiteRepository = CreateVideoSiteRepository();

            var result = videoSiteRepository.FetchNewVideoMetadataForChannels(new List<RepositoryChannelDto> { ChannelDto("c1") });

            Assert.Equal(new[] { "First", "Second" }, result.NewlyFetchedVideos.Select(video => video.Title));
        }

        [Fact]
        public void TheVideoSiteIdComesFromTheVideoMetadataGetter()
        {
            var videoSiteRepository = CreateVideoSiteRepository();

            Assert.Equal("site-id-of-the-url", videoSiteRepository.GetVideoSiteIdFromVideoUrl("the-url"));
        }

        [Fact]
        public void AnExceptionOfTheVideoMetadataGetterPropagatesUnchangedFromGettingTheSiteId()
        {
            var exceptionOfTheGetter = new Exception("URL format is not recognized. Cannot get site ID.");
            _fakeSiteVideoMetadataGetter.SiteIdFromUrl = _ => throw exceptionOfTheGetter;
            var videoSiteRepository = CreateVideoSiteRepository();

            var exceptionThatPropagated = Assert.Throws<Exception>(() => videoSiteRepository.GetVideoSiteIdFromVideoUrl("the-url"));

            Assert.Same(exceptionOfTheGetter, exceptionThatPropagated);
        }

        [Fact]
        public void TheVideoWithItsChannelIsFetchedFromTheSiteAsDtosKeepingTheOriginalResponses()
        {
            var videoResponse = new object();
            var channelResponse = new object();
            _fakeSiteVideoMetadataGetter.VideoAndChannelBySiteId = siteId => (
                new YoutubeMetadata { SiteId = siteId, Title = "The video", Thumbnail = new EntityThumbnail(), OriginalResponse = videoResponse },
                new YoutubeChannel { SiteId = "channel-1", Name = "The channel", Thumbnail = new EntityThumbnail(), OriginalResponse = channelResponse });
            var videoSiteRepository = CreateVideoSiteRepository();

            var videoWithChannel = videoSiteRepository.FetchVideoWithItsChannelFromVideoSite("v1");

            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Same(videoResponse, videoWithChannel.Video.OriginalResponseFromVideoSite);
            Assert.Equal("The channel", videoWithChannel.Channel.Name);
            Assert.Same(channelResponse, videoWithChannel.Channel.OriginalResponseFromVideoSite);
        }

        [Fact]
        public void WhenTheSiteReturnsNoVideoTheVideoAndTheChannelAreNull()
        {
            _fakeSiteVideoMetadataGetter.VideoAndChannelBySiteId = _ => (null, null);
            var videoSiteRepository = CreateVideoSiteRepository();

            var videoWithChannel = videoSiteRepository.FetchVideoWithItsChannelFromVideoSite("v1");

            Assert.Null(videoWithChannel.Video);
            Assert.Null(videoWithChannel.Channel);
        }
    }
}
