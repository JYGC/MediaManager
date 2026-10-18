using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class VideoBrowserModuleBase
    {
        protected readonly IVideoBrowsingUseCases _videoBrowsingUseCases;

        protected const int _pageSize = 10000;
        protected int _skip;

        public VideoBrowserModuleBase(IVideoBrowsingUseCases videoBrowsingUseCases, TimeProvider timeProvider)
        {
            _videoBrowsingUseCases = videoBrowsingUseCases;

            _timer = timeProvider.CreateTimer(UpdateVideoDownloadStatuses, null, _timeBetweenInterval, _timeBetweenInterval);
        }

        public bool IsLoading { get; protected set; } = false;
        public List<ApplicationVideoWithChannelDto> Results { get; protected set; } = [];

        private readonly List<ApplicationVideoMetadataDto> _videoDownloadStatusUpdateQueue = [];

        public void ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus newStatus, ApplicationVideoMetadataDto video)
        {
            video.DownloadStatus = newStatus;
            _videoDownloadStatusUpdateQueue.Add(video);
        }

        public void RemoveIsBeingDownloadedStatus(ApplicationVideoWithChannelDto videoWithChannel)
        {
            ApplicationVideoMetadataDto videoAfterTheFlagWasCleared = _videoBrowsingUseCases.ClearBeingDownloadedFlagOfVideo(videoWithChannel.Video);
            videoWithChannel.Video.IsBeingDownloaded = videoAfterTheFlagWasCleared.IsBeingDownloaded;
        }

        public void DownloadVideoNow(ApplicationVideoWithChannelDto videoWithChannel)
        {
            ApplicationVideoMetadataDto videoAfterItWasMarkedAsBeingDownloaded =
                _videoBrowsingUseCases.MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(videoWithChannel.Video);
            videoWithChannel.Video.IsBeingDownloaded = videoAfterItWasMarkedAsBeingDownloaded.IsBeingDownloaded;
        }

        private void UpdateVideoDownloadStatuses(object? state)
        {
            if (_videoDownloadStatusUpdateQueue.Count == 0)
            {
                return;
            }
            else
            {
                var videoDownloadStatusUpdateQueueSize = _videoDownloadStatusUpdateQueue.Count;
                var videos = _videoDownloadStatusUpdateQueue.GetRange(0, videoDownloadStatusUpdateQueueSize);
                _videoDownloadStatusUpdateQueue.RemoveRange(0, videoDownloadStatusUpdateQueueSize);
                _videoBrowsingUseCases.UpdateDownloadStatusOfVideos(videos);
            }
        }
        private static readonly TimeSpan _timeBetweenInterval = TimeSpan.FromSeconds(1);
        private readonly ITimer _timer;
    }
}
