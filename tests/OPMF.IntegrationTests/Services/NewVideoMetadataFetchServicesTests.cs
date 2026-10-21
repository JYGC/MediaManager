using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.IntegrationTests.Support;
using OPMF.Logging;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class NewVideoMetadataFetchServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly FakeVideoSiteRepository _fakeVideoSiteRepository = new FakeVideoSiteRepository();
        private readonly LogRepository _logRepository;

        public NewVideoMetadataFetchServicesTests(IsolatedAppFolderFixture fixture)
        {
            _fixture = fixture;
            _logRepository = new LogRepository(fixture.DatabaseConnectionOpener, new Logger(fixture.DatabaseConnectionOpener), TimeProvider.System);
        }

        private NewVideoMetadataFetchServices CreateNewVideoMetadataFetchServices() =>
            new NewVideoMetadataFetchServices(
                new ChannelRepository(_fixture.DatabaseConnectionOpener),
                _fakeVideoSiteRepository,
                new VideoMetadataRepository(_fixture.DatabaseConnectionOpener),
                _logRepository,
                new StringWriter());

        [Fact]
        public void TheFetchedVideosAndTheUpdatedCheckDatesOfTheChannelsThatAreNotBlacklistedAreStored()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var checkedChannel = Channel(prefix + "_checked", "Checked");
            var blacklistedChannel = Channel(prefix + "_blacklisted", "Blacklisted", blacklisted: true);
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { checkedChannel, blacklistedChannel }));
            List<string> siteIdsOfChannelsAskedAbout = null;
            _fakeVideoSiteRepository.FetchResult = channels =>
            {
                siteIdsOfChannelsAskedAbout = channels.Select(channel => channel.SiteId).Where(siteId => siteId.StartsWith(prefix)).ToList();
                foreach (var channel in channels.Where(channel => channel.SiteId == prefix + "_checked"))
                {
                    channel.LastCheckedForNewVideosAt = new DateTime(2026, 10, 18);
                    channel.LatestVideoPublishedAt = new DateTime(2026, 10, 17);
                }
                return new RepositoryNewVideoMetadataFetchResultDto
                {
                    ChannelsWithUpdatedCheckDates = channels,
                    NewlyFetchedVideos = new List<RepositoryVideoMetadataDto>
                    {
                        new RepositoryVideoMetadataDto { SiteId = prefix + "_v1", Title = "Fetched", ChannelSiteId = prefix + "_checked" },
                    },
                };
            };

            CreateNewVideoMetadataFetchServices().FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            Assert.Equal(new[] { prefix + "_checked" }, siteIdsOfChannelsAskedAbout);
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
            {
                Assert.Equal("Fetched", db.YoutubeMetadataDbCollection.GetBySiteId(prefix + "_v1").Title);
                var storedChannel = db.YoutubeChannelDbCollection.GetBySiteId(prefix + "_checked");
                Assert.Equal(new DateTime(2026, 10, 18), storedChannel.LastCheckedOut);
                Assert.Equal(new DateTime(2026, 10, 17), storedChannel.LastActivityDate);
                Assert.Null(db.YoutubeChannelDbCollection.GetBySiteId(prefix + "_blacklisted").LastCheckedOut);
            });
        }

        [Fact]
        public void AFailureRecordsAnErrorEntryWithTheChannelsAsJsonTextAndRethrowsTheOriginalException()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel(prefix + "_c", "Failing channel") }));
            var exceptionOfTheSite = new InvalidOperationException(prefix + " the sign in failed");
            _fakeVideoSiteRepository.FetchResult = _ => throw exceptionOfTheSite;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(
                () => CreateNewVideoMetadataFetchServices().FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted());

            Assert.Same(exceptionOfTheSite, exceptionThatPropagated);
            var recordedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));
            var recordedError = Assert.Single(recordedErrors, error => error.Message == prefix + " the sign in failed");
            Assert.Contains("\"SiteId\": \"" + prefix + "_c\"", recordedError.VariablesAsIndentedJson);
            Assert.Contains("\"metadatas\": \"null\"", recordedError.VariablesAsIndentedJson);
        }
    }
}
