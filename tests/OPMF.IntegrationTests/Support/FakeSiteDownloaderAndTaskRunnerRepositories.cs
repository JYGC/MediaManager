using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;

namespace OPMF.IntegrationTests.Support
{
    public class FakeVideoSiteRepository : IVideoSiteRepository
    {
        public Func<List<RepositoryChannelDto>, RepositoryNewVideoMetadataFetchResultDto> FetchResult { get; set; } =
            channels => new RepositoryNewVideoMetadataFetchResultDto { ChannelsWithUpdatedCheckDates = channels };
        public Func<string, string> SiteIdFromUrl { get; set; } = url => url;
        public Func<string, RepositoryVideoWithChannelDto> VideoWithChannelBySiteId { get; set; } = _ => new RepositoryVideoWithChannelDto();

        public RepositoryNewVideoMetadataFetchResultDto FetchNewVideoMetadataForChannels(List<RepositoryChannelDto> channelsToCheckForNewVideos) =>
            FetchResult(channelsToCheckForNewVideos);

        public string GetVideoSiteIdFromVideoUrl(string videoUrl) => SiteIdFromUrl(videoUrl);

        public RepositoryVideoWithChannelDto FetchVideoWithItsChannelFromVideoSite(string videoSiteId) => VideoWithChannelBySiteId(videoSiteId);
    }

    public class FakeVideoDownloaderRepository : IVideoDownloaderRepository
    {
        public Exception ExceptionToThrowWhenDownloading { get; set; }
        public Action RunWhileDownloading { get; set; } = () => { };

        public void PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded()
        {
        }

        public List<RepositoryVideoMetadataDto> DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(List<RepositoryVideoMetadataDto> videosToDownload)
        {
            RunWhileDownloading();
            if (ExceptionToThrowWhenDownloading != null)
            {
                throw ExceptionToThrowWhenDownloading;
            }
            return videosToDownload.Select(video => new RepositoryVideoMetadataDto { SiteId = video.SiteId, DownloadStatus = RepositoryVideoDownloadStatus.Downloaded }).ToList();
        }
    }

    public class FakeTaskRunnerProcessRepository : ITaskRunnerProcessRepository
    {
        public List<string> VideoSiteIdsTheTaskRunnerWasStartedFor { get; } = new List<string>();

        public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId) => VideoSiteIdsTheTaskRunnerWasStartedFor.Add(videoSiteId);
    }
}
