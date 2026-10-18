using System;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using OPMF.SiteAdapter.Youtube;

namespace OPMF.UnitTests.Support
{
    public record ActivitiesPageRequest(string ChannelSiteId, DateTime PublishedAfter, string PageToken);

    public class FakeYoutubeDataApi : IYoutubeDataApi
    {
        private readonly Queue<ActivityListResponse> _activitiesPagesToReturnInOrder = new Queue<ActivityListResponse>();

        public List<ActivitiesPageRequest> ActivitiesPageRequests { get; } = new List<ActivitiesPageRequest>();
        public Dictionary<string, VideoListResponse> VideoResponsesByVideoSiteId { get; } = new Dictionary<string, VideoListResponse>();
        public Dictionary<string, ChannelListResponse> ChannelResponsesByChannelSiteId { get; } = new Dictionary<string, ChannelListResponse>();
        public List<string> RequestedVideoSiteIds { get; } = new List<string>();
        public List<string> RequestedChannelSiteIds { get; } = new List<string>();
        public Action RunWhenAnActivitiesPageIsRequested { get; set; } = () => { };

        public void ReturnActivitiesPage(ActivityListResponse activitiesPage)
        {
            _activitiesPagesToReturnInOrder.Enqueue(activitiesPage);
        }

        public ActivityListResponse ListOnePageOfActivitiesOfChannelPublishedAfter(string channelSiteId, DateTime publishedAfter, string pageToken)
        {
            ActivitiesPageRequests.Add(new ActivitiesPageRequest(channelSiteId, publishedAfter, pageToken));
            RunWhenAnActivitiesPageIsRequested();
            return _activitiesPagesToReturnInOrder.Dequeue();
        }

        public VideoListResponse ListVideoWithSiteId(string videoSiteId)
        {
            RequestedVideoSiteIds.Add(videoSiteId);
            return VideoResponsesByVideoSiteId[videoSiteId];
        }

        public ChannelListResponse ListChannelWithSiteId(string channelSiteId)
        {
            RequestedChannelSiteIds.Add(channelSiteId);
            return ChannelResponsesByChannelSiteId[channelSiteId];
        }
    }
}
