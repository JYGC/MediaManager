using System;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace OPMF.SiteAdapter.Youtube
{
    public interface IYoutubeDataApi
    {
        ActivityListResponse ListOnePageOfActivitiesOfChannelPublishedAfter(string channelSiteId, DateTime publishedAfter, string pageToken);
        VideoListResponse ListVideoWithSiteId(string videoSiteId);
        ChannelListResponse ListChannelWithSiteId(string channelSiteId);
    }

    public class YoutubeDataApi : IYoutubeDataApi
    {
        private const string __videoInfoParts = "snippet,contentDetails";
        private const string __channelParts = "snippet";

        private readonly YouTubeService __youtubeService;

        public YoutubeDataApi(UserCredential googleUserCredential, string applicationNameSentToGoogle)
        {
            __youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = applicationNameSentToGoogle,
                HttpClientInitializer = googleUserCredential
            });
        }

        public ActivityListResponse ListOnePageOfActivitiesOfChannelPublishedAfter(string channelSiteId, DateTime publishedAfter, string pageToken)
        {
            ActivitiesResource.ListRequest request = __youtubeService.Activities.List(__videoInfoParts);
            request.ChannelId = channelSiteId;
            request.PublishedAfter = publishedAfter;
            request.PageToken = pageToken;
            return request.Execute();
        }

        public VideoListResponse ListVideoWithSiteId(string videoSiteId)
        {
            VideosResource.ListRequest request = __youtubeService.Videos.List(__videoInfoParts);
            request.Id = videoSiteId;
            return request.Execute();
        }

        public ChannelListResponse ListChannelWithSiteId(string channelSiteId)
        {
            ChannelsResource.ListRequest request = __youtubeService.Channels.List(__channelParts);
            request.Id = channelSiteId;
            return request.Execute();
        }
    }
}
