using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class ChannelMetadataTests
    {
        [Fact]
        public void Default_HasNullChannelAndMetadata()
        {
            var cm = new ChannelMetadata();
            Assert.Null(cm.Channel);
            Assert.Null(cm.Metadata);
        }

        [Fact]
        public void Properties_RoundTrip()
        {
            var channel = new YoutubeChannel { SiteId = "C1" };
            var metadata = new YoutubeMetadata { SiteId = "V1", ChannelSiteId = "C1" };
            var cm = new ChannelMetadata { Channel = channel, Metadata = metadata };

            Assert.Same(channel, cm.Channel);
            Assert.Same(metadata, cm.Metadata);
        }
    }
}
