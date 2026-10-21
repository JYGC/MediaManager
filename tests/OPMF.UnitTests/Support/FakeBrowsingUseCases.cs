using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace OPMF.UnitTests.Support
{
    public class FakeVideoBrowsingUseCases : IVideoBrowsingUseCases
    {
        public Func<int, int, List<ApplicationVideoWithChannelDto>> OnGetNewVideosWithChannels { get; set; } = (_, _) => new List<ApplicationVideoWithChannelDto>();
        public Func<int, int, List<ApplicationVideoWithChannelDto>> OnGetVideosWithChannelsQueuedToDownloadOrWaiting { get; set; } = (_, _) => new List<ApplicationVideoWithChannelDto>();
        public Func<string, int, int, List<ApplicationVideoWithChannelDto>> OnGetVideosWithChannelsWhereVideoTitleContainsWord { get; set; } = (_, _, _) => new List<ApplicationVideoWithChannelDto>();
        public Func<string, string, int, int, List<ApplicationVideoWithChannelDto>> OnGetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords { get; set; } = (_, _, _, _) => new List<ApplicationVideoWithChannelDto>();
        public Func<string, ApplicationVideoWithChannelDto> OnGetVideoWithChannelByUrl { get; set; } = _ => new ApplicationVideoWithChannelDto();
        public Func<ApplicationVideoMetadataDto, ApplicationVideoMetadataDto> OnClearBeingDownloadedFlagOfVideo { get; set; } =
            video => new ApplicationVideoMetadataDto { SiteId = video.SiteId, IsBeingDownloaded = false };
        public Func<ApplicationVideoMetadataDto, ApplicationVideoMetadataDto> OnMarkVideoAsBeingDownloaded { get; set; } =
            video => new ApplicationVideoMetadataDto { SiteId = video.SiteId, IsBeingDownloaded = true };

        public List<string> CallsMade { get; } = new List<string>();
        public List<List<ApplicationVideoMetadataDto>> DownloadStatusUpdates { get; } = new List<List<ApplicationVideoMetadataDto>>();

        public List<ApplicationVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize)
        {
            CallsMade.Add($"GetNewVideosWithChannels({skip}, {pageSize})");
            return OnGetNewVideosWithChannels(skip, pageSize);
        }

        public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsQueuedToDownloadOrWaiting({skip}, {pageSize})");
            return OnGetVideosWithChannelsQueuedToDownloadOrWaiting(skip, pageSize);
        }

        public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsWhereVideoTitleContainsWord({wordInVideoTitle}, {skip}, {pageSize})");
            return OnGetVideosWithChannelsWhereVideoTitleContainsWord(wordInVideoTitle, skip, pageSize);
        }

        public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(string wordInChannelName, string wordInVideoTitle, int skip, int pageSize)
        {
            CallsMade.Add($"GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords({wordInChannelName}, {wordInVideoTitle}, {skip}, {pageSize})");
            return OnGetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(wordInChannelName, wordInVideoTitle, skip, pageSize);
        }

        public ApplicationVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl)
        {
            CallsMade.Add($"GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored({videoUrl})");
            return OnGetVideoWithChannelByUrl(videoUrl);
        }

        public void UpdateDownloadStatusOfVideos(IEnumerable<ApplicationVideoMetadataDto> videos) => DownloadStatusUpdates.Add(videos.ToList());

        public ApplicationVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ApplicationVideoMetadataDto video)
        {
            CallsMade.Add($"ClearBeingDownloadedFlagOfVideo({video.SiteId})");
            return OnClearBeingDownloadedFlagOfVideo(video);
        }

        public ApplicationVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ApplicationVideoMetadataDto video)
        {
            CallsMade.Add($"MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt({video.SiteId})");
            return OnMarkVideoAsBeingDownloaded(video);
        }

        public Action<ApplicationVideoMetadataDto> OnSaveDownloadQualityChoiceOverridesOfVideo { get; set; } = _ => { };
        public List<ApplicationVideoMetadataDto> VideosWhoseDownloadQualityChoiceOverridesWereSaved { get; } = new List<ApplicationVideoMetadataDto>();

        public void SaveDownloadQualityChoiceOverridesOfVideo(ApplicationVideoMetadataDto video)
        {
            CallsMade.Add($"SaveDownloadQualityChoiceOverridesOfVideo({video.SiteId})");
            OnSaveDownloadQualityChoiceOverridesOfVideo(video);
            VideosWhoseDownloadQualityChoiceOverridesWereSaved.Add(video);
        }
    }

    public class FakeDownloadQualitySettingsUseCases : IDownloadQualitySettingsUseCases
    {
        public ApplicationDownloadQualityDto AppWideDownloadQualityToReturn { get; set; } = new ApplicationDownloadQualityDto
        {
            VideoCodecToPreferWhenDownloading = ApplicationVideoCodecChoice.H264,
            MaximumVideoResolutionOnTheShorterSide = ApplicationMaximumVideoResolutionChoice.NoLimit,
            AudioQualityToDownload = ApplicationAudioQualityChoice.BestAac,
        };
        public Action OnAnySave { get; set; } = () => { };
        public Exception ExceptionToThrowWhenReading { get; set; }
        public List<string> CallsMade { get; } = new List<string>();

        public ApplicationDownloadQualityDto GetAppWideDownloadQuality()
        {
            CallsMade.Add("GetAppWideDownloadQuality");
            if (ExceptionToThrowWhenReading != null)
            {
                throw ExceptionToThrowWhenReading;
            }
            return AppWideDownloadQualityToReturn;
        }

        public void SaveAppWideVideoCodecChoice(ApplicationVideoCodecChoice videoCodecChoice)
        {
            CallsMade.Add($"SaveAppWideVideoCodecChoice({videoCodecChoice})");
            OnAnySave();
        }

        public void SaveAppWideMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice maximumVideoResolutionChoice)
        {
            CallsMade.Add($"SaveAppWideMaximumVideoResolutionChoice({maximumVideoResolutionChoice})");
            OnAnySave();
        }

        public void SaveAppWideAudioQualityChoice(ApplicationAudioQualityChoice audioQualityChoice)
        {
            CallsMade.Add($"SaveAppWideAudioQualityChoice({audioQualityChoice})");
            OnAnySave();
        }
    }

    public class FakeChannelBrowsingUseCases : IChannelBrowsingUseCases
    {
        public List<ApplicationChannelDto> ChannelsToReturn { get; } = new List<ApplicationChannelDto>();
        public List<List<ApplicationChannelDto>> BlacklistUpdates { get; } = new List<List<ApplicationChannelDto>>();
        public Func<List<ApplicationChannelDto>> OnGetAllChannels { get; set; }

        public List<ApplicationChannelDto> GetAllChannels() => OnGetAllChannels != null ? OnGetAllChannels() : ChannelsToReturn.ToList();

        public void UpdateBlacklistFlagOfChannels(IEnumerable<ApplicationChannelDto> channels) => BlacklistUpdates.Add(channels.ToList());
    }

    public class FakeLogBrowsingUseCases : ILogBrowsingUseCases
    {
        public List<ApplicationLogEntryDto> WarningsToReturn { get; } = new List<ApplicationLogEntryDto>();
        public List<ApplicationLogEntryDto> ErrorsToReturn { get; } = new List<ApplicationLogEntryDto>();
        public List<string> CallsMade { get; } = new List<string>();
        public Func<List<ApplicationLogEntryDto>> OnGetErrors { get; set; }

        public List<ApplicationLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            CallsMade.Add($"GetWarningsLoggedBetween({startDateTime:O}, {endDateTime:O})");
            return WarningsToReturn.ToList();
        }

        public List<ApplicationLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            CallsMade.Add($"GetErrorsLoggedBetween({startDateTime:O}, {endDateTime:O})");
            return OnGetErrors != null ? OnGetErrors() : ErrorsToReturn.ToList();
        }
    }
}
