using System;
using System.Text.Json;
using Xunit;

using OPMF.Tests.TestData;
using ChannelServices = MediaManager.Initializations.ChannelServicesComposition;
using System.Linq;
using OPMF.Entities;
using System.Collections.Generic;
using OPMF.Tests.Helpers;

namespace OPMF.Tests.Services
{
    [TestCaseOrderer(ordererTypeName: "OPMF.Tests.PriorityOrderer", ordererAssemblyName: "OPMF.Tests")]
    public class TestChannelServicesInsertOrUpdate : IClassFixture<AppFolderFixture>
    {
        [Fact, TestPriority(1)]
        public void Test1InsertNew()
        {
            var channel45List = ChannelTestData.ChannelList1.Take(ChannelTestData.ChannelList1.Length * 4 / 5).ToList();
            var result = ChannelServices.insertOrUpdate(channel45List);
            Assert.True(result.IsOk);
            var (insertNumber, updateNumber) = result.ResultValue;
            Assert.Equal(channel45List.Count, insertNumber);
            Assert.Equal(0, updateNumber);
            var siteIdToChannelsFromDb = ChannelServices.getAll().ResultValue.ToDictionary(c => c.SiteId, c => c);

            Assert.All(channel45List, c =>
            {
                Assert.Contains(c.SiteId, (IDictionary<string, Channel>)siteIdToChannelsFromDb);
                TestChannelServiceHelpers.AssertChannelIsEqual(c, siteIdToChannelsFromDb[c.SiteId]);
            });
        }

        [Fact, TestPriority(2)]
        public void Test2UpdateExisting()
        {
            var channel23List = ChannelTestData.ChannelList1.Take(ChannelTestData.ChannelList1.Length * 2 / 3).ToList();
            var modifiedChannels = new List<Channel>();
            for (int i = 0; i < channel23List.Count; i++)
            {
                var copiedChannel = JsonSerializer.Deserialize<Channel>(JsonSerializer.Serialize(channel23List[i]));
                TestChannelServiceHelpers.ModifyChannel(copiedChannel, i);
                modifiedChannels.Add(copiedChannel);
            }

            var result = ChannelServices.insertOrUpdate(modifiedChannels);
            Assert.True(result.IsOk);
            var (insertNumber, updateNumber) = result.ResultValue;
            Assert.Equal(0, insertNumber);
            Assert.Equal(modifiedChannels.Count, updateNumber);
            var siteIdToChannelsFromDb = ChannelServices.getAll().ResultValue.ToDictionary(c => c.SiteId, c => c);

            Assert.All(modifiedChannels, c =>
            {
                Assert.Contains(c.SiteId, (IDictionary<string, Channel>)siteIdToChannelsFromDb);
                TestChannelServiceHelpers.AssertChannelIsEqual(c, siteIdToChannelsFromDb[c.SiteId]);
            });
        }

        [Fact, TestPriority(3)]
        public void Test3InsertAndUpdate()
        {
            var result = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
            Assert.True(result.IsOk);
            var (insertNumber, updateNumber) = result.ResultValue;
            var channelListCount = ChannelTestData.ChannelList1.Length;
            var channel45ListCount = ChannelTestData.ChannelList1.Take(channelListCount * 4 / 5).Count();
            Assert.Equal(channelListCount - channel45ListCount, insertNumber);
            Assert.Equal(channel45ListCount, updateNumber);
            var siteIdToChannelsFromDb = ChannelServices.getAll().ResultValue.ToDictionary(c => c.SiteId, c => c);

            Assert.All(ChannelTestData.ChannelList1, c =>
            {
                Assert.Contains(c.SiteId, (IDictionary<string, Channel>)siteIdToChannelsFromDb);
                TestChannelServiceHelpers.AssertChannelIsEqual(c, siteIdToChannelsFromDb[c.SiteId]);
            });
        }
    }

    public class TestChannelServicesInsertOrUpdateDuplicates : IClassFixture<AppFolderFixture>
    {
        [Fact]
        public void Test1InsertDuplicate()
        {
            var result = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList2);
            Assert.True(result.IsError);
            Assert.IsType<LiteDB.LiteException>(result.ErrorValue);
            Assert.Contains("Cannot insert duplicate key in unique index", result.ErrorValue.Message);
            var channelsFromDb = ChannelServices.getAll().ResultValue;
            Assert.Empty(channelsFromDb);
        }
    }

    public class TestChannelServicesGetResults : IClassFixture<AppFolderFixture>
    {
        public TestChannelServicesGetResults()
        {
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList2);
        }

        [Fact]
        public void TestGetBySiteId()
        {
            var channelList2Index = ChannelTestData.ChannelList2.Length - 8;
            var channelList2Channel = ChannelTestData.ChannelList2[channelList2Index];
            var result = ChannelServices.getBySiteId(channelList2Channel.SiteId);
            Assert.True(result.IsOk);
            TestChannelServiceHelpers.AssertChannelIsEqual(channelList2Channel, result.ResultValue.Value);
        }

        [Fact]
        public void TestGetNonExistingBySiteId()
        {
            var result = ChannelServices.getBySiteId("sg dghds ghdsdsf");
            Assert.True(result.IsOk);
            Assert.Throws<NullReferenceException>(() => result.ResultValue.Value);
        }

        [Fact]
        public void TestGetManyBySiteIds()
        {
            var channelSiteIds = ChannelTestData.ChannelList1.Select(c => c.SiteId).Distinct().ToList();
            var result = ChannelServices.getManyBySiteIds(channelSiteIds);
            Assert.True(result.IsOk);
            Assert.Equal(channelSiteIds.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetNotBacklisted()
        {
            var result = ChannelServices.getNotBacklisted();
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, c => Assert.False(c.Blacklisted));
            var allNotBlacklist = ChannelServices.getAll().ResultValue.Where(c => !c.Blacklisted).ToList();
            Assert.Equal(allNotBlacklist.Count, result.ResultValue.Count);
        }

        [Fact]
        public void TestGetManyByWordInName()
        {
            var wordInChannelName = "LetsPlay";
            var result = ChannelServices.getManyByWordInName(wordInChannelName);
            Assert.True(result.IsOk);
            Assert.All(result.ResultValue, c => Assert.Contains(wordInChannelName, c.Name));
            var allResultsWithWordInChannelName = ChannelServices.getAll().ResultValue
                .Where(c => c.Name != null && c.Name.Contains(wordInChannelName)).ToList();
            Assert.Equal(allResultsWithWordInChannelName.Count, result.ResultValue.Count);
        }
    }

    [TestCaseOrderer(ordererTypeName: "OPMF.Tests.PriorityOrderer", ordererAssemblyName: "OPMF.Tests")]
    public class TestChannelServicesUpdates : IClassFixture<AppFolderFixture>
    {
        public TestChannelServicesUpdates()
        {
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList1);
            _ = ChannelServices.insertOrUpdate(ChannelTestData.ChannelList2);
        }

        [Fact, TestPriority(1)]
        public void TestUpdateLastCheckedOutAndActivity()
        {
            var updatedChannels = ChannelTestData.ChannelList1.Select(c =>
            {
                c.LastActivityDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                c.LastCheckedOut = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                return c;
            }).ToList();
            var result = ChannelServices.updateLastCheckedOutAndActivity(updatedChannels);
            Assert.True(result.IsOk);
            Assert.Equal(updatedChannels.Count, result.ResultValue);
            var siteIdToChannelMap = ChannelServices.getAll().ResultValue
                .ToDictionary(c => c.SiteId, c => c);
            Assert.All(updatedChannels, expectedChannel =>
                TestChannelServiceHelpers.AssertChannelIsEqual(
                    expectedChannel, siteIdToChannelMap[expectedChannel.SiteId]));
        }

        [Fact, TestPriority(2)]
        public void TestUpdateBlackListStatus()
        {
            var updatedChannels = ChannelTestData.ChannelList1.Select(c =>
            {
                c.Blacklisted = true;
                return c;
            }).ToList();
            var result = ChannelServices.updateBlackListStatus(updatedChannels);
            Assert.True(result.IsOk);
            Assert.Equal(updatedChannels.Count, result.ResultValue);
            var siteIdToChannelMap = ChannelServices.getAll().ResultValue
                .ToDictionary(c => c.SiteId, c => c);
            Assert.All(updatedChannels, expectedChannel =>
                TestChannelServiceHelpers.AssertChannelIsEqual(
                    expectedChannel, siteIdToChannelMap[expectedChannel.SiteId]));
        }
    }
}
