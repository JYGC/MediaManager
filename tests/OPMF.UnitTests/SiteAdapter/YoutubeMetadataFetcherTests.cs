using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using LiteDB;
using Microsoft.Extensions.Time.Testing;
using OPMF.Entities;
using OPMF.Logging;
using OPMF.SiteAdapter.Youtube;
using OPMF.UnitTests.Support;
using Xunit;
using EntityChannel = OPMF.Entities.Channel;

namespace OPMF.UnitTests.SiteAdapter
{
    public class YoutubeMetadataFetcherTests : IDisposable
    {
        private const int NewChannelPastVideoDayLimit = 3;
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private readonly FakeYoutubeDataApi _fakeYoutubeDataApi = new FakeYoutubeDataApi();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener(CreateBsonMapperThatStoresANullReferenceExceptionAsText());
        private readonly StringWriter _standardOutput = new StringWriter();
        private readonly YoutubeMetadataFetcher _youtubeMetadataFetcher;

        public YoutubeMetadataFetcherTests()
        {
            _youtubeMetadataFetcher = new YoutubeMetadataFetcher(
                _fakeYoutubeDataApi, NewChannelPastVideoDayLimit, _fakeTimeProvider, new Logger(_databaseConnectionOpener), _standardOutput);
        }

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private static BsonMapper CreateBsonMapperThatStoresANullReferenceExceptionAsText()
        {
            // LiteDB cannot map an exception that was thrown (its TargetSite fails to serialize), so a test cannot store the real one
            var bsonMapper = new BsonMapper();
            bsonMapper.RegisterType<NullReferenceException>(exception => new BsonValue(exception.ToString()), bson => new NullReferenceException(bson.AsString));
            return bsonMapper;
        }

        private DateTime ClockNow => _fakeTimeProvider.GetLocalNow().LocalDateTime;

        private static Activity UploadActivity(string videoSiteId, string title, DateTime publishedAt)
        {
            return new Activity
            {
                ContentDetails = new ActivityContentDetails { Upload = new ActivityContentDetailsUpload { VideoId = videoSiteId } },
                Snippet = new ActivitySnippet
                {
                    Title = title,
                    Description = "description of " + title,
                    ChannelId = "channel-1",
                    PublishedAt = publishedAt,
                    Thumbnails = new ThumbnailDetails { Default__ = new Thumbnail { Url = "https://thumbnails/" + videoSiteId, Width = 120, Height = 90 } },
                },
            };
        }

        private static Activity ActivityWithoutAnUpload()
        {
            return new Activity { ContentDetails = new ActivityContentDetails(), Snippet = new ActivitySnippet { Title = "a like" } };
        }

        private static Activity ActivityThatFailsWhenItIsMapped(string videoSiteId)
        {
            var activity = UploadActivity(videoSiteId, "title of " + videoSiteId, new DateTime(2026, 10, 1));
            activity.Snippet.Thumbnails = null;
            return activity;
        }

        private static ActivityListResponse ActivitiesPage(string nextPageToken, params Activity[] activities)
        {
            return new ActivityListResponse { Items = activities.ToList(), NextPageToken = nextPageToken };
        }

        private static YoutubeChannel ChannelCheckedAt(DateTime? lastCheckedOut, DateTime? lastActivityDate = null)
        {
            return new YoutubeChannel { SiteId = "channel-1", Name = "The channel", LastCheckedOut = lastCheckedOut, LastActivityDate = lastActivityDate };
        }

        private List<Metadata> FetchMetadataOfChannels(params YoutubeChannel[] channels)
        {
            var channelsToCheck = channels.Cast<EntityChannel>().ToList();
            return _youtubeMetadataFetcher.FetchMetadata(ref channelsToCheck);
        }

        [Fact]
        public void PublishedAfterIsTheLastCheckMinusTwoDaysForAChannelThatWasChecked()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null));

            FetchMetadataOfChannels(ChannelCheckedAt(new DateTime(2026, 10, 10, 8, 0, 0)));

            var request = Assert.Single(_fakeYoutubeDataApi.ActivitiesPageRequests);
            Assert.Equal("channel-1", request.ChannelSiteId);
            Assert.Equal(new DateTime(2026, 10, 8, 8, 0, 0), request.PublishedAfter);
        }

        [Fact]
        public void PublishedAfterIsNowMinusTheNewChannelDayLimitForAChannelThatWasNeverChecked()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null));

            FetchMetadataOfChannels(ChannelCheckedAt(null));

            var request = Assert.Single(_fakeYoutubeDataApi.ActivitiesPageRequests);
            Assert.Equal(ClockNow.AddDays(-NewChannelPastVideoDayLimit), request.PublishedAfter);
        }

        [Fact]
        public void PageTokensAreFollowedUntilThereAreNone()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage("token-1", UploadActivity("video-1", "first", new DateTime(2026, 10, 1))));
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage("token-2", UploadActivity("video-2", "second", new DateTime(2026, 10, 2))));
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, UploadActivity("video-3", "third", new DateTime(2026, 10, 3))));

            var videos = FetchMetadataOfChannels(ChannelCheckedAt(null));

            Assert.Equal(new string[] { null, "token-1", "token-2" }, _fakeYoutubeDataApi.ActivitiesPageRequests.Select(request => request.PageToken));
            Assert.Equal(new[] { "video-1", "video-2", "video-3" }, videos.Select(video => video.SiteId));
        }

        [Fact]
        public void ActivitiesWithoutAnUploadAreSkipped()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, ActivityWithoutAnUpload(), UploadActivity("video-1", "first", new DateTime(2026, 10, 1))));

            var videos = FetchMetadataOfChannels(ChannelCheckedAt(null));

            Assert.Equal("video-1", Assert.Single(videos).SiteId);
        }

        [Fact]
        public void AnUploadIsMappedToYoutubeMetadata()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, UploadActivity("video-1", "first", new DateTime(2026, 10, 1, 12, 30, 0))));

            var video = Assert.Single(FetchMetadataOfChannels(ChannelCheckedAt(null)));

            Assert.IsType<YoutubeMetadata>(video);
            Assert.Equal("video-1", video.SiteId);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", video.Url);
            Assert.Equal("first", video.Title);
            Assert.Equal("description of first", video.Description);
            Assert.Equal("channel-1", video.ChannelSiteId);
            Assert.Equal(new DateTime(2026, 10, 1, 12, 30, 0), video.PublishedAt);
            Assert.Equal("https://thumbnails/video-1", video.Thumbnail.Url);
            Assert.Equal(120, video.Thumbnail.Width);
            Assert.Equal(90, video.Thumbnail.Height);
        }

        [Fact]
        public void TheChannelsLatestActivityDateBecomesTheNewestPublishedAtOfTheUploads()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null,
                UploadActivity("video-1", "older", new DateTime(2026, 10, 1)),
                UploadActivity("video-2", "newest", new DateTime(2026, 10, 5)),
                UploadActivity("video-3", "middle", new DateTime(2026, 10, 3))));
            var channel = ChannelCheckedAt(null);

            FetchMetadataOfChannels(channel);

            Assert.Equal(new DateTime(2026, 10, 5), channel.LastActivityDate);
        }

        [Fact]
        public void AnEarlierUploadDoesNotLowerTheChannelsLatestActivityDate()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, UploadActivity("video-1", "older", new DateTime(2026, 10, 1))));
            var channel = ChannelCheckedAt(null, lastActivityDate: new DateTime(2026, 10, 9));

            FetchMetadataOfChannels(channel);

            Assert.Equal(new DateTime(2026, 10, 9), channel.LastActivityDate);
        }

        [Fact]
        public void TheChannelsLastCheckedOutIsTheTimeAfterTheLastPageWasFetched()
        {
            _fakeYoutubeDataApi.RunWhenAnActivitiesPageIsRequested = () => _fakeTimeProvider.Advance(TimeSpan.FromMinutes(2));
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage("token-1"));
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null));
            var channel = ChannelCheckedAt(null);

            FetchMetadataOfChannels(channel);

            Assert.Equal(InstantTheFakeClockStartsAt.AddMinutes(4).LocalDateTime, channel.LastCheckedOut);
        }

        [Fact]
        public void AnActivityThatFailsLogsAnInfoEntryAndLetsTheOtherActivitiesThrough()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null,
                ActivityThatFailsWhenItIsMapped("video-broken"),
                UploadActivity("video-fine", "fine", new DateTime(2026, 10, 2))));

            var videos = FetchMetadataOfChannels(ChannelCheckedAt(null));

            Assert.Equal("video-fine", Assert.Single(videos).SiteId);
            OPMFLog loggedEntry = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => loggedEntry = databaseAdapter.OPMFLogDbCollection.GetAll().Single());
            Assert.Equal(OPMFLogType.Info, loggedEntry.Type);
            Assert.StartsWith("skipping channel: ", loggedEntry.Message);
            Assert.Equal(new[] { "activity object json", "exception thrown", "nextPageToken" }, loggedEntry.Variables.Keys.OrderBy(key => key));
            Assert.Equal(ClockNow, loggedEntry.DateCreated);
        }

        [Fact]
        public void AnActivityThatFailsKeepsTheChannelsDates()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, ActivityThatFailsWhenItIsMapped("video-broken"), UploadActivity("video-fine", "fine", new DateTime(2026, 10, 2))));
            var channel = ChannelCheckedAt(new DateTime(2026, 10, 10), new DateTime(2026, 10, 9));

            FetchMetadataOfChannels(channel);

            Assert.Equal(new DateTime(2026, 10, 10), channel.LastCheckedOut);
            Assert.Equal(new DateTime(2026, 10, 9), channel.LastActivityDate);
        }

        [Fact]
        public void APlainChannelInTheListThrowsAnInvalidCastException()
        {
            var channels = new List<EntityChannel> { new EntityChannel { SiteId = "channel-1" } };

            Assert.Throws<InvalidCastException>(() => _youtubeMetadataFetcher.FetchMetadata(ref channels));
        }

        [Fact]
        public void TheConsoleLinesNameTheChannelAndEveryActivity()
        {
            _fakeYoutubeDataApi.ReturnActivitiesPage(ActivitiesPage(null, ActivityWithoutAnUpload(), UploadActivity("video-1", "first", new DateTime(2026, 10, 1))));

            FetchMetadataOfChannels(ChannelCheckedAt(null));

            Assert.Equal(
                "fetching video metadata for youtube channel: The channel" + Environment.NewLine +
                "no video id detected. skip" + Environment.NewLine +
                "fetched: first" + Environment.NewLine,
                _standardOutput.ToString());
        }
    }
}
