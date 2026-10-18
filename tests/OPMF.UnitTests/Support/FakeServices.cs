using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Services;
using MediaManager.Services2;

namespace OPMF.UnitTests.Support
{
    public class FakeChannelServices : IChannelServices
    {
        public List<ServiceChannelDto> ChannelsToReturn { get; } = new List<ServiceChannelDto>();
        public List<ServiceChannelDto> ChannelsWhoseBlacklistFlagWasUpdated { get; } = new List<ServiceChannelDto>();

        public List<ServiceChannelDto> GetAllChannels() => ChannelsToReturn.ToList();

        public void UpdateBlacklistFlagOfChannels(IEnumerable<ServiceChannelDto> channels) => ChannelsWhoseBlacklistFlagWasUpdated.AddRange(channels);
    }

    public class FakeMetadataServices : IMetadataServices
    {
        public List<ServiceVideoMetadataDto> VideosWhoseStatusWasUpdated { get; } = new List<ServiceVideoMetadataDto>();
        public List<ServiceVideoMetadataDto> VideosWhoseFlagWasCleared { get; } = new List<ServiceVideoMetadataDto>();
        public List<ServiceVideoMetadataDto> VideosMarkedAsBeingDownloaded { get; } = new List<ServiceVideoMetadataDto>();

        public void UpdateDownloadStatusOfVideos(IEnumerable<ServiceVideoMetadataDto> videos) => VideosWhoseStatusWasUpdated.AddRange(videos);

        public ServiceVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ServiceVideoMetadataDto video)
        {
            VideosWhoseFlagWasCleared.Add(video);
            return new ServiceVideoMetadataDto { SiteId = video.SiteId, Title = "returned by the service", IsBeingDownloaded = false };
        }

        public ServiceVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceVideoMetadataDto video)
        {
            VideosMarkedAsBeingDownloaded.Add(video);
            return new ServiceVideoMetadataDto { SiteId = video.SiteId, Title = "returned by the service", IsBeingDownloaded = true };
        }
    }

    public class FakeChannelMetadataServices : IChannelMetadataServices
    {
        public List<ServiceVideoWithChannelDto> VideosWithChannelsToReturn { get; } = new List<ServiceVideoWithChannelDto>();
        public List<string> CallsMade { get; } = new List<string>();

        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(string wordInChannelName, string wordInVideoTitle, int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords({wordInChannelName}, {wordInVideoTitle}, {skip}, {pageSize})");
            return VideosWithChannelsToReturn.ToList();
        }

        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsWhereVideoTitleContainsWord({wordInVideoTitle}, {skip}, {pageSize})");
            return VideosWithChannelsToReturn.ToList();
        }

        public List<ServiceVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize)
        {
            CallsMade.Add($"GetNewVideosWithChannels({skip}, {pageSize})");
            return VideosWithChannelsToReturn.ToList();
        }

        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsQueuedToDownloadOrWaiting({skip}, {pageSize})");
            return VideosWithChannelsToReturn.ToList();
        }

        public ServiceVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl)
        {
            CallsMade.Add($"GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored({videoUrl})");
            return VideosWithChannelsToReturn.Single();
        }
    }

    public class FakeLogServices : ILogServices
    {
        public List<ServiceLogEntryDto> WarningsToReturn { get; } = new List<ServiceLogEntryDto>();
        public List<ServiceLogEntryDto> ErrorsToReturn { get; } = new List<ServiceLogEntryDto>();
        public List<(DateTime Start, DateTime End)> RangesRequested { get; } = new List<(DateTime, DateTime)>();
        public List<(string ErrorMessage, string FullExceptionText)> ErrorsRecorded { get; } = new List<(string, string)>();
        public Exception ExceptionToThrowWhenRecording { get; set; }

        public List<ServiceLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            RangesRequested.Add((startDateTime, endDateTime));
            return WarningsToReturn.ToList();
        }

        public List<ServiceLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            RangesRequested.Add((startDateTime, endDateTime));
            return ErrorsToReturn.ToList();
        }

        public void RecordError(string errorMessage, string fullExceptionText)
        {
            ErrorsRecorded.Add((errorMessage, fullExceptionText));
            if (ExceptionToThrowWhenRecording != null)
            {
                throw ExceptionToThrowWhenRecording;
            }
        }
    }

    public class RecordingNewVideoMetadataFetchServices : INewVideoMetadataFetchServices
    {
        public int TimesFetchedAndStored { get; private set; }

        public void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted() => TimesFetchedAndStored++;
    }

    public class RecordingVideoDownloadServices : IVideoDownloadServices
    {
        public int TimesEveryQueuedVideoWasDownloaded { get; private set; }
        public List<string> SiteIdsOfVideosDownloadedOnTheirOwn { get; } = new List<string>();

        public void DownloadEveryVideoQueuedToDownload() => TimesEveryQueuedVideoWasDownloaded++;

        public void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt) => SiteIdsOfVideosDownloadedOnTheirOwn.Add(siteIdOrPartOfIt);
    }
}
