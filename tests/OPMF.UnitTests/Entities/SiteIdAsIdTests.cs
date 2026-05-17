using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class SiteIdAsIdTests
    {
        [Fact]
        public void SettingSiteId_AlsoSetsId()
        {
            var entity = new SiteIdAsId { SiteId = "abc123" };
            Assert.Equal("abc123", entity.Id);
        }

        [Fact]
        public void SettingId_AlsoSetsSiteId()
        {
            var entity = new SiteIdAsId { Id = "xyz789" };
            Assert.Equal("xyz789", entity.SiteId);
        }

        [Fact]
        public void DefaultSiteIdAndId_AreNull()
        {
            var entity = new SiteIdAsId();
            Assert.Null(entity.Id);
            Assert.Null(entity.SiteId);
        }
    }
}
