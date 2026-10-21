using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using OPMF.Logging;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Repositories
{
    public class RepositoriesOnRealLiteDbTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly ChannelRepository _channelRepository;
        private readonly VideoMetadataRepository _videoMetadataRepository;
        private readonly LogRepository _logRepository;

        public RepositoriesOnRealLiteDbTests(IsolatedAppFolderFixture fixture)
        {
            _fixture = fixture;
            _channelRepository = new ChannelRepository(fixture.DatabaseConnectionOpener);
            _videoMetadataRepository = new VideoMetadataRepository(fixture.DatabaseConnectionOpener);
            _logRepository = new LogRepository(fixture.DatabaseConnectionOpener, new Logger(fixture.DatabaseConnectionOpener), TimeProvider.System);
        }

        private static string NewMarker() => Guid.NewGuid().ToString("N")[..6];

        private BsonDocument RawDocumentOf(string collectionName, string id)
        {
            using var liteDatabase = new LiteDatabase($"Filename={_fixture.ReadonlySettings.GetDatabasePath()};connection=shared");
            return liteDatabase.GetCollection(collectionName).FindById(id);
        }

        [Fact]
        public void AChannelWrittenThroughTheRepositoryIsStoredWithTheYoutubeChannelTypeLabel()
        {
            var marker = NewMarker();

            _channelRepository.InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(new[] { new RepositoryChannelDto { SiteId = marker + "_c", Name = "Written through the repository" } });

            Assert.Equal("OPMF.Entities.YoutubeChannel, OPMF.Entities", RawDocumentOf("YoutubeChannel", marker + "_c")["_type"].AsString);
        }

        [Fact]
        public void AVideoWrittenThroughTheRepositoryIsStoredWithTheYoutubeMetadataTypeLabel()
        {
            var marker = NewMarker();

            _videoMetadataRepository.InsertVideosThatAreNotStoredYet(new[] { new RepositoryVideoMetadataDto { SiteId = marker + "_v", Title = "Written through the repository" } });

            Assert.Equal("OPMF.Entities.YoutubeMetadata, OPMF.Entities", RawDocumentOf("YoutubeMetadata", marker + "_v")["_type"].AsString);
        }

        [Fact]
        public void ARecordedErrorIsStoredWithTheOPMFErrorTypeLabel()
        {
            var marker = NewMarker();

            _logRepository.RecordErrorLogEntry(new RepositoryErrorLogEntryToRecordDto { Message = marker + " failed", ExceptionText = "the exception text" });

            var loggedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));
            var recordedError = Assert.Single(loggedErrors, error => error.Message == marker + " failed");
            Assert.Equal("OPMF.Entities.OPMFError, OPMF.Entities", RawDocumentOf("OPMFLog", recordedError.Id)["_type"].AsString);
        }

        [Fact]
        public void TheVariablesOfARecordedErrorAreStoredAsStrings()
        {
            var marker = NewMarker();

            _logRepository.RecordErrorLogEntry(new RepositoryErrorLogEntryToRecordDto
            {
                Message = marker + " failed",
                VariablesAsJsonText = new Dictionary<string, string> { { "channels", "[{\"SiteId\":\"c1\"}]" }, { "metadatas", "null" } },
            });

            var loggedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));
            var recordedError = Assert.Single(loggedErrors, error => error.Message == marker + " failed");
            var rawVariables = RawDocumentOf("OPMFLog", recordedError.Id)["Variables"].AsDocument;
            Assert.True(rawVariables["channels"].IsString);
            Assert.Equal("[{\"SiteId\":\"c1\"}]", rawVariables["channels"].AsString);
            Assert.True(rawVariables["metadatas"].IsString);
        }

        [Fact]
        public void ARecordedErrorShowsItsJsonTextVariablesAsJsonWhenRead()
        {
            var marker = NewMarker();
            _logRepository.RecordErrorLogEntry(new RepositoryErrorLogEntryToRecordDto
            {
                Message = marker + " failed",
                VariablesAsJsonText = new Dictionary<string, string> { { "channels", "[{\"SiteId\":\"c1\"}]" } },
            });

            var loggedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));

            var recordedError = Assert.Single(loggedErrors, error => error.Message == marker + " failed");
            Assert.Contains("\"SiteId\": \"c1\"", recordedError.VariablesAsIndentedJson);
        }

        [Fact]
        public void ChannelsSeededTheOldWayAreReadBackCorrectlyThroughTheRepository()
        {
            var marker = NewMarker();
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeChannelDbCollection.InsertOrUpdate(new[] { Channel(marker + "_old", "Seeded the old way", blacklisted: true) }));

            var channel = _channelRepository.GetChannelWithSiteId(marker + "_old");

            Assert.Equal("Seeded the old way", channel.Name);
            Assert.True(channel.IsBlacklisted);
        }

        [Fact]
        public void VideosSeededTheOldWayAreReadBackCorrectlyThroughTheRepository()
        {
            var marker = NewMarker();
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(marker + "_old", status: MetadataStatus.ToDownload, title: "Seeded the old way") }));

            var queuedVideos = _videoMetadataRepository.GetVideosQueuedToDownload().Where(video => video.SiteId.StartsWith(marker)).ToList();

            var video = Assert.Single(queuedVideos);
            Assert.Equal("Seeded the old way", video.Title);
            Assert.Equal(RepositoryVideoDownloadStatus.ToDownload, video.DownloadStatus);
        }

        [Fact]
        public void LogEntriesSeededTheOldWayAreReadBackWithTheirObjectVariablesAsIndentedJson()
        {
            var marker = NewMarker();
            var oldEntry = new OPMFError(new InvalidOperationException(marker + " failed"))
            {
                Variables = new Dictionary<string, object> { { "metadatas", new List<Metadata> { Metadata(marker + "_v") } } },
            };
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.OPMFLogDbCollection.InsertBulk(new[] { (OPMFLog)oldEntry }));

            var loggedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));

            var readBackError = Assert.Single(loggedErrors, error => error.Message == marker + " failed");
            Assert.Contains(marker + "_v", readBackError.VariablesAsIndentedJson);
            Assert.Contains("\"Title\"", readBackError.VariablesAsIndentedJson);
        }

        [Fact]
        public void GetFirstVideoWhoseSiteIdContainsMatchesASubstringOfTheSiteId()
        {
            var marker = NewMarker();
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(marker + "_fullSiteId") }));

            var video = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_full");

            Assert.Equal(marker + "_fullSiteId", video.SiteId);
        }

        [Fact]
        public void TheDownloadQualityChoiceOverridesOfAVideoRoundTripThroughTheRepository()
        {
            var marker = NewMarker();
            _videoMetadataRepository.InsertVideosThatAreNotStoredYet(new[] { new RepositoryVideoMetadataDto { SiteId = marker + "_v", Title = "With overrides" } });
            var videoToChange = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_v");
            videoToChange.VideoCodecChoiceOverride = RepositoryVideoCodecChoice.AnyCodec;
            videoToChange.MaximumVideoResolutionChoiceOverride = RepositoryMaximumVideoResolutionChoice.UpTo720p;
            videoToChange.AudioQualityChoiceOverride = RepositoryAudioQualityChoice.SmallestAac;

            _videoMetadataRepository.UpdateDownloadQualityChoiceOverridesOfVideo(videoToChange);

            var videoReadBack = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_v");
            Assert.Equal(RepositoryVideoCodecChoice.AnyCodec, videoReadBack.VideoCodecChoiceOverride);
            Assert.Equal(RepositoryMaximumVideoResolutionChoice.UpTo720p, videoReadBack.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(RepositoryAudioQualityChoice.SmallestAac, videoReadBack.AudioQualityChoiceOverride);
            var rawDocument = RawDocumentOf("YoutubeMetadata", marker + "_v");
            Assert.Equal("AnyCodec", rawDocument["VideoCodecChoiceOverride"].AsString);
            Assert.Equal("UpTo720p", rawDocument["MaximumVideoResolutionChoiceOverride"].AsString);
            Assert.Equal("SmallestAac", rawDocument["AudioQualityChoiceOverride"].AsString);
        }

        [Fact]
        public void AVideoStoredWithoutOverridesReadsAsNullAndItsStoredDocumentHasNoOverrideFields()
        {
            var marker = NewMarker();
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(marker + "_old", title: "Seeded the old way") }));

            var video = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_old");

            Assert.Null(video.VideoCodecChoiceOverride);
            Assert.Null(video.MaximumVideoResolutionChoiceOverride);
            Assert.Null(video.AudioQualityChoiceOverride);
            var rawDocument = RawDocumentOf("YoutubeMetadata", marker + "_old");
            Assert.False(rawDocument.ContainsKey("VideoCodecChoiceOverride"));
            Assert.False(rawDocument.ContainsKey("MaximumVideoResolutionChoiceOverride"));
            Assert.False(rawDocument.ContainsKey("AudioQualityChoiceOverride"));
        }

        [Fact]
        public void UpdatingTheDownloadStatusOfAVideoKeepsItsDownloadQualityChoiceOverrides()
        {
            var marker = NewMarker();
            _videoMetadataRepository.InsertVideosThatAreNotStoredYet(new[] { new RepositoryVideoMetadataDto { SiteId = marker + "_v", Title = "With overrides" } });
            var videoWithOverrides = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_v");
            videoWithOverrides.MaximumVideoResolutionChoiceOverride = RepositoryMaximumVideoResolutionChoice.UpTo480p;
            _videoMetadataRepository.UpdateDownloadQualityChoiceOverridesOfVideo(videoWithOverrides);
            var videoWithoutOverridesInMemory = new RepositoryVideoMetadataDto { SiteId = marker + "_v", DownloadStatus = RepositoryVideoDownloadStatus.Downloaded };

            _videoMetadataRepository.UpdateDownloadStatusOfVideos(new[] { videoWithoutOverridesInMemory });

            var videoReadBack = _videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(marker + "_v");
            Assert.Equal(RepositoryVideoDownloadStatus.Downloaded, videoReadBack.DownloadStatus);
            Assert.Equal(RepositoryMaximumVideoResolutionChoice.UpTo480p, videoReadBack.MaximumVideoResolutionChoiceOverride);
        }
    }
}
