using OPMF.Entities;
using OPMF.Tests.TestData;
using Xunit;
using ChannelServices = MediaManager.Initializations.ChannelServicesComposition;
using MetadataServices = MediaManager.Initializations.MetadataServicesComposition;
using ChannelMetadataServices = MediaManager.Initializations.ChannelMetadataServicesComposition;
using System.Linq;

namespace OPMF.Tests.Services
{
    public class TestChannelMetadataServicesGetByTextSearch : IClassFixture<AppFolderFixture>
    {
        public TestChannelMetadataServicesGetByTextSearch()
        {
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList2);
            _ = MetadataServices.insertNew(MetadataTestData.MetadataList1);
            _ = MetadataServices.insertNew(MetadataTestData.MetadataList2);
        }

        [Fact]
        public void TestGetByChannelAndTitleContainingWord()
        {
            var wordInChannelName = "High";
            var wordInMetadataTitle = "Walkthrough";
            var result = ChannelMetadataServices.getByChannelAndTitleContainingWord(
                wordInChannelName, wordInMetadataTitle, 0, 10000);
            Assert.True(result.IsOk);
            var channelWithWordSiteIds = ChannelServices.getAll().ResultValue.Where(c =>
                    c.Name != null && c.Name.Contains(wordInChannelName)).Select(c => c.SiteId)
                .ToList();
            var metadatasWithWord = MetadataServices.getAll().ResultValue.Where(m =>
                    m.Title.Contains(wordInMetadataTitle) &&
                    channelWithWordSiteIds.Contains(m.ChannelSiteId)).ToList();
            Assert.Equal(metadatasWithWord.Count, result.ResultValue.Count);
            Assert.Equal(metadatasWithWord.Select(m => m.SiteId).OrderBy(s => s).ToList(),
                result.ResultValue.Select(cm => cm.Metadata.SiteId).OrderBy(s => s).ToList());
        }

        [Fact]
        public void TestGetByChannelAndTitleContainingWordMetadataNotInChannel()
        {
            var wordInChannelName = "Scishow";
            var wordInMetadataTitle = "Walkthrough";
            var result = ChannelMetadataServices.getByChannelAndTitleContainingWord(
                wordInChannelName, wordInMetadataTitle, 0, 10000);
            Assert.True(result.IsOk);
            Assert.Empty(result.ResultValue);
        }

        [Fact]
        public void TestGetByChannelAndTitleContainingWordEmptyMetadataTitle()
        {
            var wordInChannelName = "Tell";
            var wordInMetadataTitle = "";
            var result = ChannelMetadataServices.getByChannelAndTitleContainingWord(
                wordInChannelName, wordInMetadataTitle, 0, 10000);
            Assert.True(result.IsOk);
            var channelWithWordSiteIds = ChannelServices.getAll().ResultValue.Where(c =>
                    c.Name != null && c.Name.Contains(wordInChannelName)).Select(c => c.SiteId)
                .ToList();
            var metadatasWithWord = MetadataServices.getAll().ResultValue.Where(m =>
                    m.Title.Contains(wordInMetadataTitle) &&
                    channelWithWordSiteIds.Contains(m.ChannelSiteId)).ToList();
            Assert.Equal(metadatasWithWord.Count, result.ResultValue.Count);
            Assert.Equal(metadatasWithWord.Select(m => m.SiteId).OrderBy(s => s).ToList(),
                result.ResultValue.Select(cm => cm.Metadata.SiteId).OrderBy(s => s).ToList());
        }

        [Fact]
        public void TestGetByTitleContainingWord()
        {
            var wordInMetadataTitle = "Game";
            var result = ChannelMetadataServices.getByTitleContainingWord(
                wordInMetadataTitle, 0, 10000);
            Assert.True(result.IsOk);
            var metadatasWithWord = MetadataServices.getAll().ResultValue.Where(m =>
                    m.Title.Contains(wordInMetadataTitle)).ToList();
            Assert.Equal(metadatasWithWord.Count, result.ResultValue.Count);
            Assert.Equal(metadatasWithWord.Select(m => m.SiteId).OrderBy(s => s).ToList(),
                result.ResultValue.Select(cm => cm.Metadata.SiteId).OrderBy(s => s).ToList());
        }

        [Fact]
        public void TestGetNew()
        {
            var result = ChannelMetadataServices.getNew(0, 10000);
            Assert.True(result.IsOk);
            var newMetadatas = MetadataServices.getAll().ResultValue.Where(m =>
                    m.Status == MetadataStatus.New).ToList();
            Assert.Equal(newMetadatas.Count, result.ResultValue.Count);
            Assert.Equal(newMetadatas.Select(m => m.SiteId).OrderBy(s => s).ToList(),
                result.ResultValue.Select(cm => cm.Metadata.SiteId).OrderBy(s => s).ToList());
        }

        [Fact]
        public void TestGetToDownloadAndWait()
        {
            var result = ChannelMetadataServices.getToDownloadAndWait(0, 10000);
            Assert.True(result.IsOk);
            var newMetadatas = MetadataServices.getAll().ResultValue.Where(m =>
                    m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait).ToList();
            Assert.Equal(newMetadatas.Count, result.ResultValue.Count);
            Assert.Equal(newMetadatas.Select(m => m.SiteId).OrderBy(s => s).ToList(),
                result.ResultValue.Select(cm => cm.Metadata.SiteId).OrderBy(s => s).ToList());
        }
    }

    [TestCaseOrderer(ordererTypeName: "OPMF.Tests.PriorityOrderer", ordererAssemblyName: "OPMF.Tests")]
    public class TestChannelMetadataServicesGetVideoByUrl : IClassFixture<AppFolderFixture>
    {
        public TestChannelMetadataServicesGetVideoByUrl()
        {
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
        }

        [Fact, TestPriority(1)]
        public void Test1GetVideoByUrl()
        {
            var newMetadataSiteId = "IDcuiZznRVM";
            var result = ChannelMetadataServices.getVideoByUrl($"https://www.youtube.com/watch?v={newMetadataSiteId}");
            Assert.True(result.IsOk);
            var newMetadata = MetadataServices.getAll().ResultValue.Where(m => m.SiteId == newMetadataSiteId).ToList();
            Assert.Single(newMetadata);
        }

        [Fact, TestPriority(2)]
        public void Test2GetVideoByUrlExistingVideo()
        {
            var result = ChannelMetadataServices.getVideoByUrl("https://www.youtube.com/watch?v=IDcuiZznRVM");
            Assert.True(result.IsOk);
        }

        [Fact, TestPriority(3)]
        public void Test3GetVideoBYUrlNewChannel()
        {
            var newMetadataSiteId = "ZBpOYnP72RU";
            var result = ChannelMetadataServices.getVideoByUrl($"https://www.youtube.com/watch?v={newMetadataSiteId}");
            Assert.True(result.IsOk);
            var newMetadata = MetadataServices.getAll().ResultValue.Where(m => m.SiteId == newMetadataSiteId).ToList();
            Assert.Single(newMetadata);

        }

        [Fact, TestPriority(4)]
        public void Test4GetVideoBYUrlInvalidUrl()
        {
            var result = ChannelMetadataServices.getVideoByUrl("HonkBlarge");
            Assert.True(result.IsError);
        }

        [Fact, TestPriority(5)]
        public void Test5GetVideoBYUrlInvalidSiteId()
        {
            var result = ChannelMetadataServices.getVideoByUrl("https://www.youtube.com/watch?v=NotExists");
            Assert.True(result.IsError);
        }
    }
}
