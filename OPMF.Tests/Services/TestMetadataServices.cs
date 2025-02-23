using OPMF.Entities;
using OPMF.Tests.TestData;
using System.Linq;
using Xunit;

using ChannelServices = MediaManager.Initializations.ChannelServicesComposition;
using MetadataServices = MediaManager.Initializations.MetadataServicesComposition;
using System;

namespace OPMF.Tests.Services
{
    public class TestMetadataServicesGetResults : IClassFixture<AppFolderFixture>
    {
        public TestMetadataServicesGetResults()
        {
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList2);
            _ = MetadataServices.insertNew(MetadataTestData.MetadataList1);
        }

        [Fact]
        public void TestGetToDownload()
        {
            var result = MetadataServices.getToDownload();
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Equal(MetadataStatus.ToDownload, m.Status));
            var allToDownload = MetadataServices.getAll().ResultValue.Where(m => m.Status == MetadataStatus.ToDownload).ToList();
            Assert.Equal(allToDownload.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetNew()
        {
            var result = MetadataServices.getNew(0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Equal(MetadataStatus.New, m.Status));
            var allNew = MetadataServices.getAll().ResultValue.Where(m => m.Status == MetadataStatus.New).ToList();
            Assert.Equal(allNew.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetToDownloadAndWait()
        {
            var result = MetadataServices.getToDownloadAndWait(0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.True(m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait));
            var allToDownloadAndWait = MetadataServices.getAll().ResultValue
                .Where(m => m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait).ToList();
            Assert.Equal(allToDownloadAndWait.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetManyByWordInTitle()
        {
            var wordInTitle = "NASA";
            var result = MetadataServices.getManyByWordInTitle(wordInTitle, 0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Contains(wordInTitle, m.Title));
            var titlesContainingWord = MetadataServices.getAll().ResultValue.Where(m => m.Title.Contains(wordInTitle)).ToList();
            Assert.Equal(titlesContainingWord.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetManyByChannelSiteIdAndWordInTitle()
        {
            var wordInChannelName = "Sm";
            var channelSiteIds = ChannelServices.getManyByWordInName(wordInChannelName).ResultValue.Select(c => c.SiteId).ToList();
            var wordInTitle = "Rocket";
            var result = MetadataServices.getManyByChannelSiteIdAndWordInTitle(channelSiteIds, wordInTitle, 0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Contains(wordInTitle, m.Title));
            var titlesContainingWordAndHasChannelSiteIds = MetadataServices.getAll().ResultValue
                .Where(m => m.Title.Contains(wordInTitle) && channelSiteIds.Contains(m.ChannelSiteId)).ToList();
            Assert.Equal(titlesContainingWordAndHasChannelSiteIds.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetNonExistentSiteId()
        {
            var siteId = "DoesNotExist";
            var result = MetadataServices.getBySiteId(siteId);
            Assert.True(result.IsOk);
            Assert.Throws<NullReferenceException>(() => result.ResultValue.Value);
        }
    }
}
