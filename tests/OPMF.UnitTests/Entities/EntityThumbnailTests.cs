using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class EntityThumbnailTests
    {
        [Fact]
        public void DefaultProperties_AreUnset()
        {
            var thumb = new EntityThumbnail();
            Assert.Null(thumb.Url);
            Assert.Equal(0, thumb.Width);
            Assert.Equal(0, thumb.Height);
        }

        [Fact]
        public void Properties_RoundTrip()
        {
            var thumb = new EntityThumbnail { Url = "http://example.com/a.jpg", Width = 120, Height = 90 };
            Assert.Equal("http://example.com/a.jpg", thumb.Url);
            Assert.Equal(120, thumb.Width);
            Assert.Equal(90, thumb.Height);
        }
    }
}
