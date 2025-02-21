using OPMF.Tests.TestData;
using Xunit;
using ChannelServices = MediaManager.Initializations.ChannelServicesComposition;
using MetadataServices = MediaManager.Initializations.MetadataServicesComposition;
using ChannelMetadataServices = MediaManager.Initializations.ChannelMetadataServicesComposition;
using System.Linq;

namespace OPMF.Tests.Services
{
    [TestCaseOrderer(ordererTypeName: "OPMF.Tests.PriorityOrderer", ordererAssemblyName: "OPMF.Tests")]
    public class TestChannelMetadataServicesGetVideoByUrl : IClassFixture<AppFolderFixture>
    {
        public TestChannelMetadataServicesGetVideoByUrl()
        {
            _ = ChannelServices.InsertOrUpdate(ChannelTestData.ChannelList1);
        }

        [Fact, TestPriority(1)]
        public void Test1GetVideoByUrl()
        {
            var newMetadataSiteId = "IDcuiZznRVM";
            var result = ChannelMetadataServices.GetVideoByUrl($"https://www.youtube.com/watch?v={newMetadataSiteId}");
            Assert.True(result.IsOk);
            var newMetadata = MetadataServices.GetAll().ResultValue.Where(m => m.SiteId == newMetadataSiteId).ToList();
            Assert.Single(newMetadata);
        }

        [Fact, TestPriority(2)]
        public void Test2GetVideoByUrlExistingVideo()
        {
            var result = ChannelMetadataServices.GetVideoByUrl("https://www.youtube.com/watch?v=IDcuiZznRVM");
            Assert.True(result.IsOk);
        }

        [Fact, TestPriority(3)]
        public void Test3GetVideoBYUrlNewChannel()
        {
            var newMetadataSiteId = "ZBpOYnP72RU";
            var result = ChannelMetadataServices.GetVideoByUrl($"https://www.youtube.com/watch?v={newMetadataSiteId}");
            Assert.True(result.IsOk);
            var newMetadata = MetadataServices.GetAll().ResultValue.Where(m => m.SiteId == newMetadataSiteId).ToList();
            Assert.Single(newMetadata);

        }

        [Fact, TestPriority(4)]
        public void Test4GetVideoBYUrlInvalidUrl()
        {
            var result = ChannelMetadataServices.GetVideoByUrl("HonkBlarge");
            Assert.True(result.IsError);
        }

        [Fact, TestPriority(5)]
        public void Test5GetVideoBYUrlInvalidSiteId()
        {
            var result = ChannelMetadataServices.GetVideoByUrl("https://www.youtube.com/watch?v=NotExists");
            Assert.True(result.IsError);
        }
    }
}
