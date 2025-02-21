using OPMF.Entities;
using OPMF.Tests.TestData;
using System.Linq;
using Xunit;

using ChannelServices = MediaManager.Initializations.ChannelServicesComposition;
using MetadataServices = MediaManager.Initializations.MetadataServicesComposition;
using MediaManager.Types;
using System;

namespace OPMF.Tests.Services
{
    public class TestMetadataServicesGetResults : IClassFixture<AppFolderFixture>
    {
        public TestMetadataServicesGetResults()
        {
            _ = ChannelServices.InsertOrUpdate(ChannelTestData.ChannelList1);
            _ = ChannelServices.InsertOrUpdate(ChannelTestData.ChannelList2);
            _ = MetadataServices.InsertNew(MetadataTestData.MetadataList1);
        }

        [Fact]
        public void TestGetToDownload()
        {
            var result = MetadataServices.GetToDownload();
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Equal(MetadataStatus.ToDownload, m.Status));
            var allToDownload = MetadataServices.GetAll().ResultValue.Where(m => m.Status == MetadataStatus.ToDownload).ToList();
            Assert.Equal(allToDownload.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetNew()
        {
            var result = MetadataServices.GetNew(0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Equal(MetadataStatus.New, m.Status));
            var allNew = MetadataServices.GetAll().ResultValue.Where(m => m.Status == MetadataStatus.New).ToList();
            Assert.Equal(allNew.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetToDownloadAndWait()
        {
            var result = MetadataServices.GetToDownloadAndWait(0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.True(m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait));
            var allToDownloadAndWait = MetadataServices.GetAll().ResultValue
                .Where(m => m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait).ToList();
            Assert.Equal(allToDownloadAndWait.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetManyByWordInTitle()
        {
            var wordInTitle = "NASA";
            var result = MetadataServices.GetManyByWordInTitle(wordInTitle, 0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Contains(wordInTitle, m.Title));
            var titlesContainingWord = MetadataServices.GetAll().ResultValue.Where(m => m.Title.Contains(wordInTitle)).ToList();
            Assert.Equal(titlesContainingWord.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetManyByChannelSiteIdAndWordInTitle()
        {
            var wordInChannelName = "Sm";
            var channelSiteIds = ChannelServices.GetManyByWordInName(wordInChannelName).ResultValue.Select(c => c.SiteId).ToList();
            var wordInTitle = "Rocket";
            var result = MetadataServices.GetManyByChannelSiteIdAndWordInTitle(channelSiteIds, wordInTitle, 0, MetadataTestData.MetadataList1.Length);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, m => Assert.Contains(wordInTitle, m.Title));
            var titlesContainingWordAndHasChannelSiteIds = MetadataServices.GetAll().ResultValue
                .Where(m => m.Title.Contains(wordInTitle) && channelSiteIds.Contains(m.ChannelSiteId)).ToList();
            Assert.Equal(titlesContainingWordAndHasChannelSiteIds.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetNonExistentSiteId()
        {
            var siteId = "DoesNotExist";
            var result = MetadataServices.GetBySiteId(siteId);
            Assert.True(result.IsOk);
            Assert.Throws<NullReferenceException>(() => result.ResultValue.Value);
        }
    }
}
