using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class ChannelTests
    {
        [Fact]
        public void DefaultBlacklisted_IsFalse()
        {
            var channel = new Channel();
            Assert.False(channel.Blacklisted);
        }

        [Fact]
        public void DefaultIsAddedBySingleVideo_IsFalse()
        {
            var channel = new Channel();
            Assert.False(channel.IsAddedBySingleVideo);
        }

        [Fact]
        public void DefaultNotFound_IsFalse()
        {
            var channel = new Channel();
            Assert.False(channel.NotFound);
        }

        [Fact]
        public void DefaultThumbnail_IsInitialized()
        {
            var channel = new Channel();
            Assert.NotNull(channel.Thumbnail);
            Assert.Null(channel.Thumbnail.Url);
            Assert.Equal(0, channel.Thumbnail.Width);
            Assert.Equal(0, channel.Thumbnail.Height);
        }

        [Fact]
        public void DefaultDateProperties_AreNull()
        {
            var channel = new Channel();
            Assert.Null(channel.LastCheckedOut);
            Assert.Null(channel.LastActivityDate);
        }

        [Fact]
        public void YoutubeChannel_InheritsChannelDefaults()
        {
            var channel = new YoutubeChannel();
            Assert.False(channel.Blacklisted);
            Assert.False(channel.NotFound);
            Assert.NotNull(channel.Thumbnail);
        }
    }
}
