using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using MediaManager.Dtos.Repositories;
using MediaManager.Services2;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class VideoDownloadServicesTests : IDisposable
    {
        private readonly TemporaryFolderForOneTest _temporaryFolder = new TemporaryFolderForOneTest();
        private readonly CallRecorder _callRecorder = new CallRecorder();
        private readonly FakeVideoDownloaderRepository _fakeVideoDownloaderRepository;
        private readonly FakeVideoMetadataRepository _fakeVideoMetadataRepository;
        private readonly FakeLogRepository _fakeLogRepository;
        private readonly RecordingMissingFolderCreator _recordingMissingFolderCreator;
        private readonly RecordingDownloadedVideoFileMover _recordingDownloadedVideoFileMover;
        private readonly TemporaryFolderReadonlySettings _readonlySettings;
        private readonly AppConfig _appConfig = new AppConfig { VideoOutputFolderPath = "C:/the/video/output/folder" };
        private readonly VideoDownloadServices _videoDownloadServices;

        public VideoDownloadServicesTests()
        {
            _fakeVideoDownloaderRepository = new FakeVideoDownloaderRepository(_callRecorder);
            _fakeVideoMetadataRepository = new FakeVideoMetadataRepository(_callRecorder);
            _fakeLogRepository = new FakeLogRepository(_callRecorder);
            _recordingMissingFolderCreator = new RecordingMissingFolderCreator(_callRecorder);
            _recordingDownloadedVideoFileMover = new RecordingDownloadedVideoFileMover(_callRecorder);
            _readonlySettings = new TemporaryFolderReadonlySettings(_temporaryFolder.FolderPath);
            _videoDownloadServices = new VideoDownloadServices(
                _fakeVideoDownloaderRepository,
                _fakeVideoMetadataRepository,
                _fakeLogRepository,
                _recordingMissingFolderCreator,
                _recordingDownloadedVideoFileMover,
                _readonlySettings,
                _appConfig,
                new CallRecordingStandardOutput(_callRecorder));
        }

        public void Dispose() => _temporaryFolder.Dispose();

        private void StoreVideo(string siteId, RepositoryVideoDownloadStatus status) =>
            _fakeVideoMetadataRepository.StoredVideos.Add(new RepositoryVideoMetadataDto { SiteId = siteId, Title = "Video " + siteId, DownloadStatus = status });

        [Fact]
        public void TheStepsRunInTheOrderOfPreparingMarkingDownloadingUpdatingUnmarkingAndMovingFiles()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            Assert.Equal(
                new[]
                {
                    "PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded",
                    "print: fetching videos",
                    "GetVideosQueuedToDownload",
                    "SetBeingDownloadedFlagOfVideos(True)",
                    "DownloadVideosAndReturnThemWithTheirResultingDownloadStatus",
                    "UpdateDownloadStatusOfVideos",
                    "SetBeingDownloadedFlagOfVideos(False)",
                    "CreateFolderWhenMissing",
                    "MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist",
                },
                _callRecorder.Calls);
        }

        [Fact]
        public void OnlyTheVideosQueuedToDownloadAreDownloaded()
        {
            StoreVideo("queued", RepositoryVideoDownloadStatus.ToDownload);
            StoreVideo("not-queued", RepositoryVideoDownloadStatus.New);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            var queueDownloaded = Assert.Single(_fakeVideoDownloaderRepository.QueuesDownloaded);
            Assert.Equal(new[] { "queued" }, queueDownloaded.Select(video => video.SiteId));
        }

        [Fact]
        public void TheFlagIsSetOnTheQueuedVideosBeforeDownloadingAndClearedOnTheDownloadedVideosAfterwards()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            Assert.Equal(
                new[] { (new List<string> { "v1" }, true), (new List<string> { "v1" }, false) },
                _fakeVideoMetadataRepository.BeingDownloadedFlagChanges.Select(change => (change.VideoSiteIds, change.IsBeingDownloaded)));
        }

        [Fact]
        public void TheDownloadStatusesReturnedByTheDownloaderAreStored()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            var updatedVideo = Assert.Single(_fakeVideoMetadataRepository.VideosWhoseDownloadStatusWasUpdated);
            Assert.Equal(RepositoryVideoDownloadStatus.Downloaded, updatedVideo.DownloadStatus);
        }

        [Fact]
        public void TheOutputFolderIsCreatedBeforeAnyFileIsMoved()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            Assert.Equal(new[] { "C:/the/video/output/folder" }, _recordingMissingFolderCreator.FoldersCreated);
            Assert.True(_callRecorder.Calls.IndexOf("CreateFolderWhenMissing") < _callRecorder.Calls.IndexOf("MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist"));
        }

        [Fact]
        public void TheFilesMoveFromTheDownloadFolderToTheOutputFolderWithTheVideoAndSubtitleExtensionsOfTheConfig()
        {
            _appConfig.YoutubeDL.VideoExtension = "mkv";
            _appConfig.YoutubeDL.SubtitleExtension = "srt";
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadEveryVideoQueuedToDownload();

            var move = Assert.Single(_recordingDownloadedVideoFileMover.Moves);
            Assert.Equal(_readonlySettings.GetDownloadFolderPath(), move.SourceFolderPath);
            Assert.Equal("C:/the/video/output/folder", move.DestinationFolderPath);
            Assert.Equal(new[] { "mkv", "srt" }, move.FileExtensions);
        }

        [Fact]
        public void DownloadingTheFirstVideoWhoseSiteIdContainsUsesTheSubstringLookupAndQueuesOnlyThatVideo()
        {
            StoreVideo("abc123xyz", RepositoryVideoDownloadStatus.New);
            StoreVideo("other", RepositoryVideoDownloadStatus.ToDownload);

            _videoDownloadServices.DownloadFirstVideoWhoseSiteIdContains("c123");

            Assert.Equal(new[] { "c123" }, _fakeVideoMetadataRepository.SiteIdSearches);
            var queueDownloaded = Assert.Single(_fakeVideoDownloaderRepository.QueuesDownloaded);
            Assert.Equal(new[] { "abc123xyz" }, queueDownloaded.Select(video => video.SiteId));
        }

        [Fact]
        public void DownloadingAnUnknownSiteIdPassesAQueueHoldingNullOnAsToday()
        {
            _fakeVideoDownloaderRepository.ResultOfDownloading = videos => videos;

            _videoDownloadServices.DownloadFirstVideoWhoseSiteIdContains("unknown");

            var queueDownloaded = Assert.Single(_fakeVideoDownloaderRepository.QueuesDownloaded);
            Assert.Null(Assert.Single(queueDownloaded));
        }

        [Fact]
        public void AFailureWhileDownloadingRecordsTheErrorWithTheQueuedVideosThenRethrowsTheSameException()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);
            var exceptionOfTheDownloader = new InvalidOperationException("downloading failed");
            _fakeVideoDownloaderRepository.ExceptionToThrowWhenDownloading = exceptionOfTheDownloader;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(() => _videoDownloadServices.DownloadEveryVideoQueuedToDownload());

            Assert.Same(exceptionOfTheDownloader, exceptionThatPropagated);
            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("downloading failed", recordedError.Message);
            Assert.StartsWith("System.InvalidOperationException: downloading failed", recordedError.ExceptionText);
            Assert.Equal(new[] { "metadatas" }, recordedError.VariablesAsJsonText.Keys);
            Assert.Equal("v1", (string)JsonNode.Parse(recordedError.VariablesAsJsonText["metadatas"]).AsArray().Single()["SiteId"]);
        }

        [Fact]
        public void AFailureWhilePreparingTheDownloaderRecordsTheErrorWithNullVideosAndMarksNothing()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);
            _fakeVideoDownloaderRepository.ExceptionToThrowWhenPreparing = new InvalidOperationException("the executable could not be downloaded");

            Assert.Throws<InvalidOperationException>(() => _videoDownloadServices.DownloadEveryVideoQueuedToDownload());

            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("null", recordedError.VariablesAsJsonText["metadatas"]);
            Assert.Empty(_fakeVideoMetadataRepository.BeingDownloadedFlagChanges);
            Assert.DoesNotContain("print: fetching videos", _callRecorder.Calls);
        }

        [Fact]
        public void NoErrorIsRecordedAndNoFileIsMovedBeforeAFailure()
        {
            StoreVideo("v1", RepositoryVideoDownloadStatus.ToDownload);
            _fakeVideoDownloaderRepository.ExceptionToThrowWhenDownloading = new InvalidOperationException("downloading failed");

            Assert.Throws<InvalidOperationException>(() => _videoDownloadServices.DownloadEveryVideoQueuedToDownload());

            Assert.Empty(_recordingDownloadedVideoFileMover.Moves);
            Assert.Empty(_recordingMissingFolderCreator.FoldersCreated);
        }
    }
}
