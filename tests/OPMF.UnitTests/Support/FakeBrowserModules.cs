using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaManager.Dtos.Application;
using MediaManagerUI.Modules.ChannelBrowser;
using MediaManagerUI.Modules.LogBrowser;
using MediaManagerUI.Modules.VideoBrowser;

namespace OPMF.UnitTests.Support
{
    public class FakeVideoBrowserModule : IVideoBrowserModule
    {
        public bool IsLoading { get; set; }
        public List<ApplicationVideoWithChannelDto> Results { get; set; } = new List<ApplicationVideoWithChannelDto>();
        public ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses { get; set; } = Array.Empty<ApplicationVideoDownloadStatus>();
        public List<(ApplicationVideoDownloadStatus Status, string SiteId)> ScheduledStatusUpdates { get; } = new List<(ApplicationVideoDownloadStatus, string)>();
        public List<string> SiteIdsWhoseBeingDownloadedStatusWasRemoved { get; } = new List<string>();
        public List<string> SiteIdsDownloadedNow { get; } = new List<string>();

        public Task GetResultsAsync() => Task.CompletedTask;

        public void ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus newStatus, ApplicationVideoMetadataDto video) => ScheduledStatusUpdates.Add((newStatus, video.SiteId));

        public void RemoveIsBeingDownloadedStatus(ApplicationVideoWithChannelDto videoWithChannel) => SiteIdsWhoseBeingDownloadedStatusWasRemoved.Add(videoWithChannel.Video.SiteId);

        public void DownloadVideoNow(ApplicationVideoWithChannelDto videoWithChannel) => SiteIdsDownloadedNow.Add(videoWithChannel.Video.SiteId);

        public List<ApplicationVideoMetadataDto> VideosWhoseDownloadQualityChoiceOverridesWereSaved { get; } = new List<ApplicationVideoMetadataDto>();
        public Exception ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides { get; set; }

        public void SaveDownloadQualityChoiceOverridesOfVideo(ApplicationVideoMetadataDto video)
        {
            if (ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides != null)
            {
                throw ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides;
            }
            VideosWhoseDownloadQualityChoiceOverridesWereSaved.Add(video);
        }
    }

    public class FakeChannelBrowserModule : IChannelBrowserModule
    {
        public bool IsLoading { get; set; }
        public List<ApplicationChannelDto> Results { get; set; } = new List<ApplicationChannelDto>();
        public List<string> SiteIdsOfChannelsWhoseBlacklistChangeWasScheduled { get; } = new List<string>();

        public void ScheduleChannelBacklistChange(ApplicationChannelDto channel) => SiteIdsOfChannelsWhoseBlacklistChangeWasScheduled.Add(channel.SiteId);

        public Task GetResultsAsync() => Task.CompletedTask;
    }

    public class FakeLogBrowserModule : ILogBrowserModule
    {
        public bool IsLoading { get; set; }
        public List<ApplicationLogEntryDto> Results { get; set; } = new List<ApplicationLogEntryDto>();

        public Task GetResultsAsync(DateTime startDateTime, DateTime endDateTime) => Task.CompletedTask;
    }
}
