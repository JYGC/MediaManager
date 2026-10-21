using Google.Apis.YouTube.v3.Data;
using System;
using System.Linq;
using System.Xml;

namespace OPMF.SiteAdapter.Youtube
{
    public class YoutubeVideoMetadataGetter : ISiteVideoMetadataGetter
    {
        private const string __videoUrlFormat = "https://www.youtube.com/watch?v={0}";
        private const string __channelUrlFormat = "https://www.youtube.com/channel/{0}";

        private readonly IYoutubeDataApi __youtubeDataApi;
        private readonly string[] __siteIdSeparators = new string[] { "watch?v=", "&" };

        public YoutubeVideoMetadataGetter(IYoutubeDataApi youtubeDataApi)
        {
            __youtubeDataApi = youtubeDataApi;
        }

        public string GetSiteIdFromURL(string videoURL)
        {
            if (__siteIdSeparators.All(seperator => !videoURL.Contains(seperator)))
            {
                throw new Exception("URL format is not recognized. Cannot get site ID.");
            }
            return videoURL.Split(__siteIdSeparators, StringSplitOptions.None)[1];
        }

        public (Entities.Metadata, Entities.Channel) GetVideoByURL(string siteId)
        {
            Entities.Metadata videoMetadata;
            Entities.Channel channelMetadata;

            VideoListResponse videoResponse = __youtubeDataApi.ListVideoWithSiteId(siteId);
            if (videoResponse.Items == null)
            {
                return (null, null);
            }

            videoMetadata = new Entities.YoutubeMetadata
            {
                SiteId = siteId,
                Url = string.Format(__videoUrlFormat, siteId),
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

            ChannelListResponse channelResponse = __youtubeDataApi.ListChannelWithSiteId(videoMetadata.ChannelSiteId);

            channelMetadata = new Entities.YoutubeChannel
            {
                SiteId = videoMetadata.ChannelSiteId,
                Url = string.Format(__channelUrlFormat, videoMetadata.ChannelSiteId),
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

            return (videoMetadata, channelMetadata);
        }
    }
}
