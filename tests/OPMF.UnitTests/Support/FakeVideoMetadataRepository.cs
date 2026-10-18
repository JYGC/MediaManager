using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;

namespace OPMF.UnitTests.Support
{
    public record BeingDownloadedFlagChange(List<string> VideoSiteIds, bool IsBeingDownloaded);

    public class FakeVideoMetadataRepository : IVideoMetadataRepository
    {
        private readonly CallRecorder _callRecorder;

        public FakeVideoMetadataRepository(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public List<RepositoryVideoMetadataDto> StoredVideos { get; } = new List<RepositoryVideoMetadataDto>();
        public List<RepositoryVideoMetadataDto> VideosInserted { get; } = new List<RepositoryVideoMetadataDto>();
        public List<RepositoryVideoMetadataDto> VideosWhoseDownloadStatusWasUpdated { get; } = new List<RepositoryVideoMetadataDto>();
        public List<BeingDownloadedFlagChange> BeingDownloadedFlagChanges { get; } = new List<BeingDownloadedFlagChange>();
        public List<(int Skip, int PageSize)> PagesRequested { get; } = new List<(int Skip, int PageSize)>();
        public List<string> SiteIdSearches { get; } = new List<string>();
        public List<(List<string> ChannelSiteIds, string Word)> VideosOfChannelsSearches { get; } = new List<(List<string>, string)>();

        public List<RepositoryVideoMetadataDto> GetVideosQueuedToDownload()
        {
            _callRecorder.Calls.Add("GetVideosQueuedToDownload");
            return StoredVideos.Where(video => video.DownloadStatus == RepositoryVideoDownloadStatus.ToDownload).ToList();
        }

        public RepositoryVideoMetadataDto GetFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt)
        {
            _callRecorder.Calls.Add("GetFirstVideoWhoseSiteIdContains");
            SiteIdSearches.Add(siteIdOrPartOfIt);
            return StoredVideos.FirstOrDefault(video => video.SiteId.Contains(siteIdOrPartOfIt));
        }

        public List<RepositoryVideoMetadataDto> GetNewVideos(int skip, int pageSize)
        {
            _callRecorder.Calls.Add("GetNewVideos");
            PagesRequested.Add((skip, pageSize));
            return StoredVideos.Where(video => video.DownloadStatus == RepositoryVideoDownloadStatus.New).Skip(skip).Take(pageSize).ToList();
        }

        public List<RepositoryVideoMetadataDto> GetVideosQueuedToDownloadOrWaiting(int skip, int pageSize)
        {
            _callRecorder.Calls.Add("GetVideosQueuedToDownloadOrWaiting");
            PagesRequested.Add((skip, pageSize));
            return StoredVideos
                .Where(video => video.DownloadStatus == RepositoryVideoDownloadStatus.ToDownload || video.DownloadStatus == RepositoryVideoDownloadStatus.Wait)
                .Skip(skip).Take(pageSize).ToList();
        }

        public List<RepositoryVideoMetadataDto> GetVideosWhoseTitleContains(string wordInVideoTitle, int skip, int pageSize)
        {
            _callRecorder.Calls.Add("GetVideosWhoseTitleContains");
            PagesRequested.Add((skip, pageSize));
            return StoredVideos.Where(video => video.Title.Contains(wordInVideoTitle)).Skip(skip).Take(pageSize).ToList();
        }

        public List<RepositoryVideoMetadataDto> GetVideosOfChannelsWhoseTitleContains(IEnumerable<string> channelSiteIds, string wordInVideoTitle, int skip, int pageSize)
        {
            _callRecorder.Calls.Add("GetVideosOfChannelsWhoseTitleContains");
            var requestedChannelSiteIds = channelSiteIds.ToList();
            VideosOfChannelsSearches.Add((requestedChannelSiteIds, wordInVideoTitle));
            PagesRequested.Add((skip, pageSize));
            return StoredVideos
                .Where(video => requestedChannelSiteIds.Contains(video.ChannelSiteId) && (string.IsNullOrWhiteSpace(wordInVideoTitle) || video.Title.Contains(wordInVideoTitle)))
                .Skip(skip).Take(pageSize).ToList();
        }

        public virtual void InsertVideosThatAreNotStoredYet(IEnumerable<RepositoryVideoMetadataDto> videos)
        {
            _callRecorder.Calls.Add("InsertVideosThatAreNotStoredYet");
            VideosInserted.AddRange(videos);
        }

        public void UpdateDownloadStatusOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos)
        {
            _callRecorder.Calls.Add("UpdateDownloadStatusOfVideos");
            VideosWhoseDownloadStatusWasUpdated.AddRange(videos);
        }

        public void SetBeingDownloadedFlagOfVideos(IEnumerable<RepositoryVideoMetadataDto> videos, bool isBeingDownloaded)
        {
            _callRecorder.Calls.Add("SetBeingDownloadedFlagOfVideos(" + isBeingDownloaded + ")");
            BeingDownloadedFlagChanges.Add(new BeingDownloadedFlagChange(videos.Select(video => video?.SiteId).ToList(), isBeingDownloaded));
        }
    }
}
