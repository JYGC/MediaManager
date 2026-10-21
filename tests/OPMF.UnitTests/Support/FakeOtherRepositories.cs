using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;

namespace OPMF.UnitTests.Support
{
    public class FakeLogRepository : ILogRepository
    {
        private readonly CallRecorder _callRecorder;

        public FakeLogRepository(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public List<RepositoryLogEntryDto> Warnings { get; } = new List<RepositoryLogEntryDto>();
        public List<RepositoryLogEntryDto> Errors { get; } = new List<RepositoryLogEntryDto>();
        public List<(DateTime Start, DateTime End)> RangesRequested { get; } = new List<(DateTime, DateTime)>();
        public List<RepositoryErrorLogEntryToRecordDto> RecordedErrors { get; } = new List<RepositoryErrorLogEntryToRecordDto>();
        public Exception ExceptionToThrowWhenRecording { get; set; }

        public List<RepositoryLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            _callRecorder.Calls.Add("GetWarningsLoggedBetween");
            RangesRequested.Add((startDateTime, endDateTime));
            return Warnings.ToList();
        }

        public List<RepositoryLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            _callRecorder.Calls.Add("GetErrorsLoggedBetween");
            RangesRequested.Add((startDateTime, endDateTime));
            return Errors.ToList();
        }

        public void RecordErrorLogEntry(RepositoryErrorLogEntryToRecordDto errorLogEntry)
        {
            _callRecorder.Calls.Add("RecordErrorLogEntry");
            RecordedErrors.Add(errorLogEntry);
            if (ExceptionToThrowWhenRecording != null)
            {
                throw ExceptionToThrowWhenRecording;
            }
        }
    }

    public class FakeVideoSiteRepository : IVideoSiteRepository
    {
        private readonly CallRecorder _callRecorder;

        public FakeVideoSiteRepository(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public Func<List<RepositoryChannelDto>, RepositoryNewVideoMetadataFetchResultDto> FetchResult { get; set; } =
            channels => new RepositoryNewVideoMetadataFetchResultDto { ChannelsWithUpdatedCheckDates = channels };
        public Func<string, string> SiteIdFromUrl { get; set; } = url => "site-id-of-" + url;
        public Func<string, RepositoryVideoWithChannelDto> VideoWithChannelBySiteId { get; set; } = _ => new RepositoryVideoWithChannelDto();
        public List<List<RepositoryChannelDto>> ChannelsFetchedFor { get; } = new List<List<RepositoryChannelDto>>();
        public List<string> VideoSiteIdsFetched { get; } = new List<string>();

        public RepositoryNewVideoMetadataFetchResultDto FetchNewVideoMetadataForChannels(List<RepositoryChannelDto> channelsToCheckForNewVideos)
        {
            _callRecorder.Calls.Add("FetchNewVideoMetadataForChannels");
            ChannelsFetchedFor.Add(channelsToCheckForNewVideos);
            return FetchResult(channelsToCheckForNewVideos);
        }

        public string GetVideoSiteIdFromVideoUrl(string videoUrl)
        {
            _callRecorder.Calls.Add("GetVideoSiteIdFromVideoUrl");
            return SiteIdFromUrl(videoUrl);
        }

        public RepositoryVideoWithChannelDto FetchVideoWithItsChannelFromVideoSite(string videoSiteId)
        {
            _callRecorder.Calls.Add("FetchVideoWithItsChannelFromVideoSite");
            VideoSiteIdsFetched.Add(videoSiteId);
            return VideoWithChannelBySiteId(videoSiteId);
        }
    }

    public class FakeVideoDownloaderRepository : IVideoDownloaderRepository
    {
        private readonly CallRecorder _callRecorder;

        public FakeVideoDownloaderRepository(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public Exception ExceptionToThrowWhenPreparing { get; set; }
        public Exception ExceptionToThrowWhenDownloading { get; set; }
        public List<List<RepositoryVideoMetadataDto>> QueuesDownloaded { get; } = new List<List<RepositoryVideoMetadataDto>>();
        public Func<List<RepositoryVideoMetadataDto>, List<RepositoryVideoMetadataDto>> ResultOfDownloading { get; set; } =
            videos => videos.Select(video => new RepositoryVideoMetadataDto { SiteId = video.SiteId, DownloadStatus = RepositoryVideoDownloadStatus.Downloaded }).ToList();

        public void PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded()
        {
            _callRecorder.Calls.Add("PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded");
            if (ExceptionToThrowWhenPreparing != null)
            {
                throw ExceptionToThrowWhenPreparing;
            }
        }

        public List<RepositoryVideoMetadataDto> DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(List<RepositoryVideoMetadataDto> videosToDownload)
        {
            _callRecorder.Calls.Add("DownloadVideosAndReturnThemWithTheirResultingDownloadStatus");
            QueuesDownloaded.Add(videosToDownload);
            if (ExceptionToThrowWhenDownloading != null)
            {
                throw ExceptionToThrowWhenDownloading;
            }
            return ResultOfDownloading(videosToDownload);
        }
    }

    public class FakeTaskRunnerProcessRepository : ITaskRunnerProcessRepository
    {
        public List<string> VideoSiteIdsTheTaskRunnerWasStartedFor { get; } = new List<string>();

        public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId)
        {
            VideoSiteIdsTheTaskRunnerWasStartedFor.Add(videoSiteId);
        }
    }
}
