using System;
using OPMF.Entities;

namespace OPMF.IntegrationTests.Support
{
    public static class TestData
    {
        public static YoutubeChannel Channel(string siteId, string name = null, bool blacklisted = false)
            => new YoutubeChannel
            {
                SiteId = siteId,
                Name = name ?? ("Channel " + siteId),
                Url = "https://www.youtube.com/channel/" + siteId,
                Description = "desc-" + siteId,
                Blacklisted = blacklisted,
            };

        public static YoutubeMetadata Metadata(
            string siteId,
            string channelSiteId = "C1",
            MetadataStatus status = MetadataStatus.New,
            string title = null,
            DateTime? publishedAt = null)
            => new YoutubeMetadata
            {
                SiteId = siteId,
                Url = "https://www.youtube.com/watch?v=" + siteId,
                Title = title ?? ("Video " + siteId),
                Description = "desc-" + siteId,
                ChannelSiteId = channelSiteId,
                Status = status,
                PublishedAt = publishedAt ?? new DateTime(2024, 1, 1, 12, 0, 0),
            };
    }
}
