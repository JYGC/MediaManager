using System;
using System.Linq;
using System.Text.Json.Nodes;
using MediaManager.Dtos.Repositories;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class NewVideoMetadataFetchServicesTests
    {
        private readonly CallRecorder _callRecorder = new CallRecorder();
        private readonly FakeChannelRepository _fakeChannelRepository;
        private readonly FakeVideoSiteRepository _fakeVideoSiteRepository;
        private readonly FakeVideoMetadataRepository _fakeVideoMetadataRepository;
        private readonly FakeLogRepository _fakeLogRepository;
        private readonly NewVideoMetadataFetchServices _newVideoMetadataFetchServices;

        public NewVideoMetadataFetchServicesTests()
        {
            _fakeChannelRepository = new FakeChannelRepository(_callRecorder);
            _fakeVideoSiteRepository = new FakeVideoSiteRepository(_callRecorder);
            _fakeVideoMetadataRepository = new FakeVideoMetadataRepository(_callRecorder);
            _fakeLogRepository = new FakeLogRepository(_callRecorder);
            _newVideoMetadataFetchServices = new NewVideoMetadataFetchServices(
                _fakeChannelRepository,
                _fakeVideoSiteRepository,
                _fakeVideoMetadataRepository,
                _fakeLogRepository,
                new CallRecordingStandardOutput(_callRecorder));
        }

        private void StoreChannel(string siteId, bool isBlacklisted = false) =>
            _fakeChannelRepository.StoredChannels.Add(new RepositoryChannelDto { SiteId = siteId, Name = "Channel " + siteId, IsBlacklisted = isBlacklisted });

        [Fact]
        public void TheStepsRunInTheOrderOfLoadingFetchingInsertingAndUpdatingWithTheirMessagesAtTodaysPoints()
        {
            StoreChannel("c1");

            _newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            Assert.Equal(
                new[]
                {
                    "print: getting channels",
                    "GetChannelsThatAreNotBlacklisted",
                    "FetchNewVideoMetadataForChannels",
                    "print: saving metadata to database",
                    "InsertVideosThatAreNotStoredYet",
                    "print: updating channels",
                    "UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels",
                },
                _callRecorder.Calls);
        }

        [Fact]
        public void OnlyTheChannelsThatAreNotBlacklistedAreCheckedForNewVideos()
        {
            StoreChannel("kept");
            StoreChannel("skipped", isBlacklisted: true);

            _newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            var channelsChecked = Assert.Single(_fakeVideoSiteRepository.ChannelsFetchedFor);
            Assert.Equal(new[] { "kept" }, channelsChecked.Select(channel => channel.SiteId));
        }

        [Fact]
        public void TheNewlyFetchedVideosAreInsertedAndTheChannelsWithUpdatedCheckDatesAreStored()
        {
            StoreChannel("c1");
            _fakeVideoSiteRepository.FetchResult = channels => new RepositoryNewVideoMetadataFetchResultDto
            {
                ChannelsWithUpdatedCheckDates = new System.Collections.Generic.List<RepositoryChannelDto>
                {
                    new RepositoryChannelDto { SiteId = "c1", LastCheckedForNewVideosAt = new DateTime(2026, 10, 18) },
                },
                NewlyFetchedVideos = new System.Collections.Generic.List<RepositoryVideoMetadataDto> { new RepositoryVideoMetadataDto { SiteId = "v1" } },
            };

            _newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            Assert.Equal(new[] { "v1" }, _fakeVideoMetadataRepository.VideosInserted.Select(video => video.SiteId));
            var channelWithUpdatedDates = Assert.Single(_fakeChannelRepository.ChannelsWhoseCheckDatesWereUpdated);
            Assert.Equal(new DateTime(2026, 10, 18), channelWithUpdatedDates.LastCheckedForNewVideosAt);
        }

        [Fact]
        public void AFailureWhileFetchingRecordsTheErrorWithTheLoadedChannelsAndNoVideosThenRethrowsTheSameException()
        {
            StoreChannel("c1");
            var exceptionOfTheSite = new InvalidOperationException("the sign in failed");
            _fakeVideoSiteRepository.FetchResult = _ => throw exceptionOfTheSite;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(() => _newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted());

            Assert.Same(exceptionOfTheSite, exceptionThatPropagated);
            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("the sign in failed", recordedError.Message);
            Assert.StartsWith("System.InvalidOperationException: the sign in failed", recordedError.ExceptionText);
            Assert.Equal(new[] { "channels", "metadatas" }, recordedError.VariablesAsJsonText.Keys.OrderBy(key => key));
            Assert.Equal("c1", (string)JsonNode.Parse(recordedError.VariablesAsJsonText["channels"]).AsArray().Single()["SiteId"]);
            Assert.Equal("null", recordedError.VariablesAsJsonText["metadatas"]);
        }

        [Fact]
        public void AFailureWhileInsertingRecordsTheErrorWithTheFetchedVideos()
        {
            StoreChannel("c1");
            _fakeVideoSiteRepository.FetchResult = channels => new RepositoryNewVideoMetadataFetchResultDto
            {
                ChannelsWithUpdatedCheckDates = channels,
                NewlyFetchedVideos = new System.Collections.Generic.List<RepositoryVideoMetadataDto> { new RepositoryVideoMetadataDto { SiteId = "v1" }, new RepositoryVideoMetadataDto { SiteId = "v2" } },
            };
            var exceptionOfTheInsert = new InvalidOperationException("the database is locked");
            var failingVideoMetadataRepository = new FailingVideoMetadataRepository(exceptionOfTheInsert);
            var services = new NewVideoMetadataFetchServices(_fakeChannelRepository, _fakeVideoSiteRepository, failingVideoMetadataRepository, _fakeLogRepository, new System.IO.StringWriter());

            Assert.Throws<InvalidOperationException>(() => services.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted());

            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal(new[] { "v1", "v2" }, JsonNode.Parse(recordedError.VariablesAsJsonText["metadatas"]).AsArray().Select(video => (string)video["SiteId"]));
        }

        [Fact]
        public void AFailureWhileLoadingTheChannelsRecordsTheErrorWithNullChannelsAndNullVideos()
        {
            var failingChannelRepository = new FailingChannelRepository(new InvalidOperationException("the database is locked"));
            var services = new NewVideoMetadataFetchServices(failingChannelRepository, _fakeVideoSiteRepository, _fakeVideoMetadataRepository, _fakeLogRepository, new System.IO.StringWriter());

            Assert.Throws<InvalidOperationException>(() => services.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted());

            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("null", recordedError.VariablesAsJsonText["channels"]);
            Assert.Equal("null", recordedError.VariablesAsJsonText["metadatas"]);
        }

        [Fact]
        public void NoErrorIsRecordedWhenEverythingSucceeds()
        {
            StoreChannel("c1");

            _newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            Assert.Empty(_fakeLogRepository.RecordedErrors);
        }

        private class FailingVideoMetadataRepository : FakeVideoMetadataRepository
        {
            private readonly Exception _exceptionToThrow;

            public FailingVideoMetadataRepository(Exception exceptionToThrow)
            {
                _exceptionToThrow = exceptionToThrow;
            }

            public override void InsertVideosThatAreNotStoredYet(System.Collections.Generic.IEnumerable<RepositoryVideoMetadataDto> videos) => throw _exceptionToThrow;
        }

        private class FailingChannelRepository : FakeChannelRepository
        {
            private readonly Exception _exceptionToThrow;

            public FailingChannelRepository(Exception exceptionToThrow)
            {
                _exceptionToThrow = exceptionToThrow;
            }

            public override System.Collections.Generic.List<RepositoryChannelDto> GetChannelsThatAreNotBlacklisted() => throw _exceptionToThrow;
        }
    }
}
