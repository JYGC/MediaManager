using System;
using System.Collections.Generic;
using OPMF.Entities;
using OPMF.SiteAdapter;

namespace OPMF.UnitTests.Support
{
    public class FakeMetadataFetcher : IMetadataFetcher<Channel, Metadata>
    {
        public List<Channel> ChannelsReceived { get; private set; }
        public List<Metadata> VideosToReturn { get; set; } = new List<Metadata>();
        public Action<Channel> ChangeEachChannelWhenFetching { get; set; } = _ => { };

        public List<Metadata> FetchMetadata(ref List<Channel> channels)
        {
            ChannelsReceived = channels;
            foreach (var channel in channels)
            {
                ChangeEachChannelWhenFetching(channel);
            }
            return VideosToReturn;
        }
    }

    public class FakeSiteVideoMetadataGetter : ISiteVideoMetadataGetter
    {
        public Func<string, string> SiteIdFromUrl { get; set; } = url => "site-id-of-" + url;
        public Func<string, (Metadata, Channel)> VideoAndChannelBySiteId { get; set; } = _ => (null, null);

        public string GetSiteIdFromURL(string videoURL) => SiteIdFromUrl(videoURL);

        public (Metadata, Channel) GetVideoByURL(string siteId) => VideoAndChannelBySiteId(siteId);
    }
}
