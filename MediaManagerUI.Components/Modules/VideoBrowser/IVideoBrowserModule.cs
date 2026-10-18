using MediaManager.Dtos.Application;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public interface IVideoBrowserModule
    {
        bool IsLoading { get; }
        List<ApplicationVideoWithChannelDto> Results { get; }
        ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses { get; }
        Task GetResultsAsync();
        void ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus newStatus, ApplicationVideoMetadataDto video);
        void RemoveIsBeingDownloadedStatus(ApplicationVideoWithChannelDto videoWithChannel);
        void DownloadVideoNow(ApplicationVideoWithChannelDto videoWithChannel);
    }
}
