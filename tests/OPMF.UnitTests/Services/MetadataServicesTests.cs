using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Services;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class MetadataServicesTests
    {
        private class RecordingTaskRunnerServices : ITaskRunnerServices
        {
            private readonly CallRecorder _callRecorder;

            public RecordingTaskRunnerServices(CallRecorder callRecorder)
            {
                _callRecorder = callRecorder;
            }

            public List<string> VideoSiteIdsStarted { get; } = new List<string>();

            public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId)
            {
                _callRecorder.Calls.Add("StartTaskRunnerProcessThatDownloadsVideoWithSiteId");
                VideoSiteIdsStarted.Add(videoSiteId);
            }
        }

        private readonly CallRecorder _callRecorder = new CallRecorder();
        private readonly FakeVideoMetadataRepository _fakeVideoMetadataRepository;
        private readonly FakeLogRepository _fakeLogRepository = new FakeLogRepository();
        private readonly RecordingTaskRunnerServices _recordingTaskRunnerServices;
        private readonly MetadataServices _metadataServices;

        public MetadataServicesTests()
        {
            _fakeVideoMetadataRepository = new FakeVideoMetadataRepository(_callRecorder);
            _recordingTaskRunnerServices = new RecordingTaskRunnerServices(_callRecorder);
            _metadataServices = new MetadataServices(_recordingTaskRunnerServices, _fakeVideoMetadataRepository, _fakeLogRepository);
        }

        private static ServiceVideoMetadataDto VideoDto(string siteId, bool isBeingDownloaded = false, ServiceVideoDownloadStatus status = ServiceVideoDownloadStatus.New) =>
            new ServiceVideoMetadataDto { SiteId = siteId, Title = "Video " + siteId, IsBeingDownloaded = isBeingDownloaded, DownloadStatus = status };

        [Fact]
        public void UpdateDownloadStatusOfVideosPassesTheVideosToTheRepositoryWithTheirStatuses()
        {
            _metadataServices.UpdateDownloadStatusOfVideos(new[] { VideoDto("v1", status: ServiceVideoDownloadStatus.ToDownload), VideoDto("v2", status: ServiceVideoDownloadStatus.Ignore) });

            Assert.Equal(new[] { "v1", "v2" }, _fakeVideoMetadataRepository.VideosWhoseDownloadStatusWasUpdated.Select(video => video.SiteId));
            Assert.Equal(
                new[] { MediaManager.Dtos.Repositories.RepositoryVideoDownloadStatus.ToDownload, MediaManager.Dtos.Repositories.RepositoryVideoDownloadStatus.Ignore },
                _fakeVideoMetadataRepository.VideosWhoseDownloadStatusWasUpdated.Select(video => video.DownloadStatus));
        }

        [Fact]
        public void ClearBeingDownloadedFlagOfVideoClearsTheFlagOnTheReturnedVideoAndInTheRepository()
        {
            var video = VideoDto("v1", isBeingDownloaded: true);

            var returnedVideo = _metadataServices.ClearBeingDownloadedFlagOfVideo(video);

            Assert.False(returnedVideo.IsBeingDownloaded);
            var flagChange = Assert.Single(_fakeVideoMetadataRepository.BeingDownloadedFlagChanges);
            Assert.Equal(new[] { "v1" }, flagChange.VideoSiteIds);
            Assert.False(flagChange.IsBeingDownloaded);
        }

        [Fact]
        public void ClearBeingDownloadedFlagOfVideoDoesNotStartTheTaskRunner()
        {
            _metadataServices.ClearBeingDownloadedFlagOfVideo(VideoDto("v1", isBeingDownloaded: true));

            Assert.Empty(_recordingTaskRunnerServices.VideoSiteIdsStarted);
        }

        [Fact]
        public void MarkVideoAsBeingDownloadedSetsTheFlagOnTheReturnedVideoAndInTheRepository()
        {
            var returnedVideo = _metadataServices.MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(VideoDto("v1"));

            Assert.True(returnedVideo.IsBeingDownloaded);
            var flagChange = Assert.Single(_fakeVideoMetadataRepository.BeingDownloadedFlagChanges);
            Assert.Equal(new[] { "v1" }, flagChange.VideoSiteIds);
            Assert.True(flagChange.IsBeingDownloaded);
        }

        [Fact]
        public void MarkVideoAsBeingDownloadedStartsTheTaskRunnerForTheVideoAfterTheFlagIsStored()
        {
            _metadataServices.MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(VideoDto("v1"));

            Assert.Equal(new[] { "v1" }, _recordingTaskRunnerServices.VideoSiteIdsStarted);
            Assert.Equal(new[] { "SetBeingDownloadedFlagOfVideos(True)", "StartTaskRunnerProcessThatDownloadsVideoWithSiteId" }, _callRecorder.Calls);
        }

        [Fact]
        public void SaveDownloadQualityChoiceOverridesOfVideoPassesTheVideoOnToTheRepositoryWithTheSameOverrides()
        {
            var video = VideoDto("v1");
            video.VideoCodecChoiceOverride = ServiceVideoCodecChoice.AnyCodec;
            video.MaximumVideoResolutionChoiceOverride = ServiceMaximumVideoResolutionChoice.UpTo720p;
            video.AudioQualityChoiceOverride = ServiceAudioQualityChoice.SmallestAac;

            _metadataServices.SaveDownloadQualityChoiceOverridesOfVideo(video);

            var videoPassedOn = Assert.Single(_fakeVideoMetadataRepository.VideosWhoseDownloadQualityChoiceOverridesWereUpdated);
            Assert.Equal("v1", videoPassedOn.SiteId);
            Assert.Equal(MediaManager.Dtos.Repositories.RepositoryVideoCodecChoice.AnyCodec, videoPassedOn.VideoCodecChoiceOverride);
            Assert.Equal(MediaManager.Dtos.Repositories.RepositoryMaximumVideoResolutionChoice.UpTo720p, videoPassedOn.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(MediaManager.Dtos.Repositories.RepositoryAudioQualityChoice.SmallestAac, videoPassedOn.AudioQualityChoiceOverride);
            Assert.Empty(_fakeLogRepository.RecordedErrors);
        }

        [Fact]
        public void SaveDownloadQualityChoiceOverridesOfVideoPassesOnOverridesThatWereClearedAsNull()
        {
            _metadataServices.SaveDownloadQualityChoiceOverridesOfVideo(VideoDto("v1"));

            var videoPassedOn = Assert.Single(_fakeVideoMetadataRepository.VideosWhoseDownloadQualityChoiceOverridesWereUpdated);
            Assert.Null(videoPassedOn.VideoCodecChoiceOverride);
            Assert.Null(videoPassedOn.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoPassedOn.AudioQualityChoiceOverride);
        }

        [Fact]
        public void AFailingRepositoryRecordsAnErrorWithTheSiteIdAndTitleOfTheVideoAndRethrowsTheSameException()
        {
            var exceptionOfTheRepository = new InvalidOperationException("the database is locked");
            _fakeVideoMetadataRepository.ExceptionToThrowWhenUpdatingDownloadQualityChoiceOverrides = exceptionOfTheRepository;

            var exceptionThrown = Assert.Throws<InvalidOperationException>(() => _metadataServices.SaveDownloadQualityChoiceOverridesOfVideo(VideoDto("v1")));

            Assert.Same(exceptionOfTheRepository, exceptionThrown);
            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("the database is locked", recordedError.Message);
            Assert.Equal("\"v1\"", recordedError.VariablesAsJsonText["videoSiteId"]);
            Assert.Equal("\"Video v1\"", recordedError.VariablesAsJsonText["videoTitle"]);
        }
    }
}
