using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Linq;
using System.Xml;

namespace OPMF.SiteAdapter.Youtube
{
    public class YoutubeVideoMetadataGetter : ISiteVideoMetadataGetter
    {
        private const string __videoInfoParts = "snippet,contentDetails";
        private const string __channelParts = "snippet";
        private const string __urlSaffolding = "https://www.youtube.com/watch?v={0}";
        private const string __channelUrlScaffolding = "https://www.youtube.com/channel/{0}";

        private YouTubeService __youtubeService;
        private readonly string[] __apiScope = new string[] { YouTubeService.Scope.YoutubeReadonly };
        private readonly string[] __siteIdExtractorSeperator = new string[] { "watch?v=", "&" };

        public YoutubeVideoMetadataGetter()
        {
            UserCredential credential = GoogleAuthentication.GetCredential(__apiScope);
            __youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = GetType().ToString()
                ,
                HttpClientInitializer = credential
            });
        }

        public string GetSiteIdFromURL(string videoURL)
        {
            if (__siteIdExtractorSeperator.All(seperator => !videoURL.Contains(seperator)))
            {
                throw new Exception("URL format is not recognized. Cannot get site ID.");
            }
            return videoURL.Split(__siteIdExtractorSeperator, StringSplitOptions.None)[1];
        }

        public (Entities.Metadata, Entities.Channel) GetVideoByURL(string siteId)
        {
            Entities.Metadata videoMetaData;
            Entities.Channel channelMetaData;

            VideosResource.ListRequest videoRequest = __youtubeService.Videos.List(__videoInfoParts);
            videoRequest.Id = siteId;
            VideoListResponse videoResponse = videoRequest.Execute();
            if (videoResponse.Items == null || videoResponse.Items.Count == 0)
            {
                return (null, null);
            }

            videoMetaData = new Entities.YoutubeMetadata
            {
                SiteId = siteId,
                Url = string.Format(__urlSaffolding, siteId),
                Title = videoResponse.Items[0].Snippet.Title,
                Thumbnail = new Entities.EntityThumbnail
                {
                    Url = videoResponse.Items[0].Snippet.Thumbnails.Default__.Url,
                    Width = videoResponse.Items[0].Snippet.Thumbnails.Default__.Width.Value,
                    Height = videoResponse.Items[0].Snippet.Thumbnails.Default__.Height.Value,
                },
                Description = videoResponse.Items[0].Snippet.Description,
                ChannelSiteId = videoResponse.Items[0].Snippet.ChannelId,
                PublishedAt = Convert.ToDateTime(videoResponse.Items[0].Snippet.PublishedAt),
                Duration = XmlConvert.ToTimeSpan(videoResponse.Items[0].ContentDetails.Duration),
                OriginalResponse = videoResponse,
            };

            ChannelsResource.ListRequest channelRequest = __youtubeService.Channels.List(__channelParts);
            channelRequest.Id = videoMetaData.ChannelSiteId;
            ChannelListResponse channelResponse = channelRequest.Execute();

            channelMetaData = new Entities.YoutubeChannel
            {
                SiteId = videoMetaData.ChannelSiteId,
                Url = string.Format(__channelUrlScaffolding, videoMetaData.ChannelSiteId),
                Name = channelResponse.Items[0].Snippet.Title,
                Thumbnail = new Entities.EntityThumbnail
                {
                    Url = channelResponse.Items[0].Snippet.Thumbnails.Default__.Url,
                    Width = channelResponse.Items[0].Snippet.Thumbnails.Default__.Width.Value,
                    Height = channelResponse.Items[0].Snippet.Thumbnails.Default__.Height.Value,
                },
                Description = channelResponse.Items[0].Snippet.Description,
                Blacklisted = true,
                IsAddedBySingleVideo = true,
                OriginalResponse = channelResponse,
            };

            return (videoMetaData, channelMetaData);
        }
    }
}
