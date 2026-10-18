using System;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using OPMF.Entities;
using OPMF.SiteAdapter.Youtube;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.SiteAdapter
{
    public class YoutubeVideoMetadataGetterTests
    {
        private readonly FakeYoutubeDataApi _fakeYoutubeDataApi = new FakeYoutubeDataApi();
        private readonly YoutubeVideoMetadataGetter _youtubeVideoMetadataGetter;

        public YoutubeVideoMetadataGetterTests()
        {
            _youtubeVideoMetadataGetter = new YoutubeVideoMetadataGetter(_fakeYoutubeDataApi);
        }

        private static Thumbnail ThumbnailOf(string url) => new Thumbnail { Url = url, Width = 120, Height = 90 };

        private static VideoListResponse VideoResponse(string channelSiteId)
        {
            return new VideoListResponse
            {
                Items = new List<Video>
                {
                    new Video
                    {
                        Snippet = new VideoSnippet
                        {
                            Title = "The video",
                            Description = "About the video",
                            ChannelId = channelSiteId,
                            PublishedAt = new DateTime(2026, 9, 1, 10, 0, 0),
                            Thumbnails = new ThumbnailDetails { Default__ = ThumbnailOf("https://thumbnails/video") },
                        },
                        ContentDetails = new VideoContentDetails { Duration = "PT1H2M3S" },
                    },
                },
            };
        }

        private static ChannelListResponse ChannelResponse()
        {
            return new ChannelListResponse
            {
                Items = new List<Google.Apis.YouTube.v3.Data.Channel>
                {
                    new Google.Apis.YouTube.v3.Data.Channel
                    {
                        Snippet = new ChannelSnippet
                        {
                            Title = "The channel",
                            Description = "About the channel",
                            Thumbnails = new ThumbnailDetails { Default__ = ThumbnailOf("https://thumbnails/channel") },
                        },
                    },
                },
            };
        }

        [Fact]
        public void TheSiteIdIsWhatFollowsWatchQuestionMarkVEqualsInTheUrl()
        {
            Assert.Equal("abc123", _youtubeVideoMetadataGetter.GetSiteIdFromURL("https://www.youtube.com/watch?v=abc123"));
        }

        [Fact]
        public void TheSiteIdEndsAtTheNextAmpersand()
        {
            Assert.Equal("abc123", _youtubeVideoMetadataGetter.GetSiteIdFromURL("https://www.youtube.com/watch?v=abc123&t=42s"));
        }

        [Fact]
        public void AnUrlWithNeitherWatchQuestionMarkVEqualsNorAnAmpersandThrowsTheUrlFormatException()
        {
            var exception = Assert.Throws<Exception>(() => _youtubeVideoMetadataGetter.GetSiteIdFromURL("https://youtu.be/abc123"));

            Assert.Equal("URL format is not recognized. Cannot get site ID.", exception.Message);
        }

        [Fact]
        public void AResponseWithNoItemsGivesNullForTheVideoAndTheChannel()
        {
            _fakeYoutubeDataApi.VideoResponsesByVideoSiteId["video-1"] = new VideoListResponse { Items = null };

            var (video, channel) = _youtubeVideoMetadataGetter.GetVideoByURL("video-1");

            Assert.Null(video);
            Assert.Null(channel);
            Assert.Empty(_fakeYoutubeDataApi.RequestedChannelSiteIds);
        }

        [Fact]
        public void TheVideoIsMappedToYoutubeMetadataWithItsOriginalResponseAndDuration()
        {
            var videoResponse = VideoResponse("channel-1");
            _fakeYoutubeDataApi.VideoResponsesByVideoSiteId["video-1"] = videoResponse;
            _fakeYoutubeDataApi.ChannelResponsesByChannelSiteId["channel-1"] = ChannelResponse();

            var (video, _) = _youtubeVideoMetadataGetter.GetVideoByURL("video-1");

            Assert.IsType<YoutubeMetadata>(video);
            Assert.Equal("video-1", video.SiteId);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", video.Url);
            Assert.Equal("The video", video.Title);
            Assert.Equal("About the video", video.Description);
            Assert.Equal("channel-1", video.ChannelSiteId);
            Assert.Equal(new DateTime(2026, 9, 1, 10, 0, 0), video.PublishedAt);
            Assert.Equal(new TimeSpan(1, 2, 3), video.Duration);
            Assert.Equal("https://thumbnails/video", video.Thumbnail.Url);
            Assert.Equal(120, video.Thumbnail.Width);
            Assert.Equal(90, video.Thumbnail.Height);
            Assert.Same(videoResponse, video.OriginalResponse);
        }

        [Fact]
        public void TheChannelIsMappedToABlacklistedYoutubeChannelAddedBySingleVideoWithItsOriginalResponse()
        {
            _fakeYoutubeDataApi.VideoResponsesByVideoSiteId["video-1"] = VideoResponse("channel-1");
            var channelResponse = ChannelResponse();
            _fakeYoutubeDataApi.ChannelResponsesByChannelSiteId["channel-1"] = channelResponse;

            var (_, channel) = _youtubeVideoMetadataGetter.GetVideoByURL("video-1");

            Assert.IsType<YoutubeChannel>(channel);
            Assert.Equal("channel-1", channel.SiteId);
            Assert.Equal("https://www.youtube.com/channel/channel-1", channel.Url);
            Assert.Equal("The channel", channel.Name);
            Assert.Equal("About the channel", channel.Description);
            Assert.True(channel.Blacklisted);
            Assert.True(channel.IsAddedBySingleVideo);
            Assert.Equal("https://thumbnails/channel", channel.Thumbnail.Url);
            Assert.Equal(120, channel.Thumbnail.Width);
            Assert.Equal(90, channel.Thumbnail.Height);
            Assert.Same(channelResponse, channel.OriginalResponse);
        }

        [Fact]
        public void TheChannelIsRequestedByTheChannelSiteIdOfTheVideo()
        {
            _fakeYoutubeDataApi.VideoResponsesByVideoSiteId["video-1"] = VideoResponse("channel-9");
            _fakeYoutubeDataApi.ChannelResponsesByChannelSiteId["channel-9"] = ChannelResponse();

            _youtubeVideoMetadataGetter.GetVideoByURL("video-1");

            Assert.Equal(new[] { "video-1" }, _fakeYoutubeDataApi.RequestedVideoSiteIds);
            Assert.Equal(new[] { "channel-9" }, _fakeYoutubeDataApi.RequestedChannelSiteIds);
        }

        [Fact]
        public void AnEmptyItemListThrowsAnArgumentOutOfRangeException()
        {
            _fakeYoutubeDataApi.VideoResponsesByVideoSiteId["video-1"] = new VideoListResponse { Items = new List<Video>() };

            Assert.Throws<ArgumentOutOfRangeException>(() => _youtubeVideoMetadataGetter.GetVideoByURL("video-1"));
        }
    }
}
