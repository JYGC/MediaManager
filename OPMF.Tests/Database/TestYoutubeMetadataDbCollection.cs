//using OPMF.Database;
//using OPMF.Entities;
//using OPMF.Tests.TestData;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;
//using Xunit.Abstractions;
//using Xunit.Sdk;

//namespace OPMF.Tests.Database
//{
//    [Collection("DatabaseTestSequence")]
//    public class TestYoutubeMetadataDbCollectionGet : IClassFixture<AppFolderFixture>
//    {
//        public TestYoutubeMetadataDbCollectionGet()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                dbAdapter.YoutubeMetadataDbCollection.InsertNew(VideoMetadata.MetadataList1);
//                dbAdapter.YoutubeMetadataDbCollection.InsertNew(VideoMetadata.MetadataList2);
//            });
//        }

//        private void AssertMetadataStatus(Func<DatabaseAdapter, IEnumerable<Metadata>> DBCallToTest, Func<Metadata, bool> StatusComparer)
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                Dictionary<string, Metadata> storedNewMetadata = DBCallToTest(dbAdapter).ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                Dictionary<string, Metadata> metadataList1Dict = VideoMetadata.MetadataList1.ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                Dictionary<string, Metadata> metadataList2Dict = VideoMetadata.MetadataList2.ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                IEnumerable<string> expectedSiteIdsInDB = metadataList1Dict.Keys.Union(metadataList2Dict.Keys);
//                IEnumerable<Metadata> expectedMetadataStatus = expectedSiteIdsInDB.Select(
//                    // Duplicate SiteIds in MetadataList2 must not override MetadataList1
//                    siteId => metadataList1Dict.ContainsKey(siteId) ? metadataList1Dict[siteId] : metadataList2Dict[siteId]
//                ).Where(metadata => StatusComparer(metadata));
//                Dictionary<string, Metadata> expectedDict = expectedMetadataStatus.ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                foreach (string siteId in storedNewMetadata.Keys)
//                {
//                    Assert.True(StatusComparer(storedNewMetadata[siteId]));
//                }
//                Assert.Equal(expectedDict.Count(), storedNewMetadata.Count());
//            });
//        }

//        [Fact]
//        public void TestGetNew()
//        {
//            AssertMetadataStatus(dbAdapter => dbAdapter.YoutubeMetadataDbCollection.GetNew(0, 2147483647), metadata => metadata.Status == MetadataStatus.New);
//        }

//        [Fact]
//        public void TestGetToDownload()
//        {
//            AssertMetadataStatus(dbAdapter => dbAdapter.YoutubeMetadataDbCollection.GetToDownload(), metadata => metadata.Status == MetadataStatus.ToDownload);
//        }

//        [Fact]
//        public void TestGetToDownloadAndWait()
//        {
//            AssertMetadataStatus(
//                dbAdapter => dbAdapter.YoutubeMetadataDbCollection.GetToDownloadAndWait(0, 2147483647),
//                metadata => metadata.Status == MetadataStatus.ToDownload || metadata.Status == MetadataStatus.Wait
//            );
//        }
//    }

//    [Collection("DatabaseTestSequence")]
//    public class TestYoutubeMetadataDbCollectionInsertNew : IClassFixture<AppFolderFixture>, ITestCaseOrderer
//    {
//        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase => testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);

//        private void AssertMetadataPropertiesEqual(Metadata expectedMetadata, Metadata actualMetadata)
//        {
//            Assert.Equal(expectedMetadata.ChannelSiteId, actualMetadata.ChannelSiteId);
//            Assert.Equal(expectedMetadata.Description, actualMetadata.Description);
//            Assert.Equal(expectedMetadata.PublishedAt, actualMetadata.PublishedAt);
//            Assert.Equal(expectedMetadata.Status, actualMetadata.Status);
//            Assert.Equal(expectedMetadata.Title, actualMetadata.Title);
//            Assert.Equal(expectedMetadata.Url, actualMetadata.Url);
//        }

//        [Fact]
//        public void Test1InsertNew()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                dbAdapter.YoutubeMetadataDbCollection.InsertNew(VideoMetadata.MetadataList1);
//                Dictionary<string, Metadata> storedMetadataDict = dbAdapter.YoutubeMetadataDbCollection.GetAll().ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                Dictionary<string, Metadata> list1MetadataDict = VideoMetadata.MetadataList1.ToDictionary(metadata => metadata.SiteId, metadata => metadata);
//                Assert.Empty(list1MetadataDict.Keys.Except(storedMetadataDict.Keys).Union(storedMetadataDict.Keys.Except(list1MetadataDict.Keys)));
//                foreach (string siteId in list1MetadataDict.Keys)
//                {
//                    AssertMetadataPropertiesEqual(list1MetadataDict[siteId], storedMetadataDict[siteId]);
//                }
//            });
//        }

//        [Fact]
//        public void Test2InsertNewAndDuplicates()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                dbAdapter.YoutubeMetadataDbCollection.InsertNew(VideoMetadata.MetadataList2);
//                // Check for duplicate SiteId - will Except if duplicate SiteIds found
//                Dictionary<string, Metadata> storedMetadata = dbAdapter.YoutubeMetadataDbCollection.GetAll().ToDictionary(
//                    metadata => metadata.SiteId,
//                    metadata => metadata
//                );
//                Dictionary<string, Metadata> list2Dict = VideoMetadata.MetadataList2.ToDictionary(metadata => metadata.SiteId, metadata => metadata);
//                Dictionary<string, Metadata> list1Dict = VideoMetadata.MetadataList1.ToDictionary(metadata => metadata.SiteId, metadata => metadata);
//                Assert.Equal(list1Dict.Keys.Union(list2Dict.Keys).Count(), storedMetadata.Keys.Count());
//                foreach (string siteId in list1Dict.Keys)
//                {
//                    // Duplicate SiteIds must not be altered
//                    AssertMetadataPropertiesEqual(list1Dict[siteId], storedMetadata[siteId]);
//                }
//                string[] newList2SiteId = list2Dict.Keys.Except(list1Dict.Keys).ToArray();
//                foreach (string siteId in newList2SiteId)
//                {
//                    AssertMetadataPropertiesEqual(list2Dict[siteId], storedMetadata[siteId]);
//                }
//            });
//        }
//    }

//    [Collection("DatabaseTestSequence")]
//    public class TestYoutubeMetadataDbCollectionUpdate : IClassFixture<AppFolderFixture>
//    {
//        public TestYoutubeMetadataDbCollectionUpdate()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                dbAdapter.YoutubeMetadataDbCollection.InsertNew(VideoMetadata.MetadataList2);
//            });
//        }

//        private void TestChangingMetadataStatus(IEnumerable<int> metadataIndexesToTest, MetadataStatus metadataStatus)
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                List<string> metadataIdsToTest = metadataIndexesToTest.Select(index => VideoMetadata.MetadataList2[index].SiteId).ToList();
//                IEnumerable<Metadata> metadatasToTest = metadataIdsToTest.Select(siteId =>
//                {
//                    Metadata metadata = dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(siteId);
//                    metadata.Status = metadataStatus;
//                    return metadata;
//                });
//                dbAdapter.YoutubeMetadataDbCollection.UpdateStatus(metadatasToTest);
//                for (int i = 0; i < metadataIdsToTest.Count(); i++)
//                {
//                    Metadata changedMetadata = dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(metadataIdsToTest[i]);
//                    Assert.Equal(metadataStatus, changedMetadata.Status);
//                }
//            });
//        }

//        [Fact]
//        public void TestUpdateStatus()
//        {
//            TestChangingMetadataStatus(new int[] { 0, 3, 7, 11, 18 }, MetadataStatus.New);
//            TestChangingMetadataStatus(new int[] { 1, 2, 5, 10 }, MetadataStatus.ToDownload);
//            TestChangingMetadataStatus(new int[] { 13, 9, 10, 5 }, MetadataStatus.Wait);
//            TestChangingMetadataStatus(new int[] { 3, 6, 7, 15, 4 }, MetadataStatus.Ignore);
//            TestChangingMetadataStatus(new int[] { 0, 1, 2, 17, 13 }, MetadataStatus.Downloaded);
//        }

//        [Fact]
//        public void TestUpdateIsBeingProcessed()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                List<string> metadataIdsToSetToBeingProcessed = (new int[] { 0, 1, 2, 5, 8, 11, 14, 18 }).Select(index => VideoMetadata.MetadataList2[index].SiteId).ToList();
//                dbAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(metadataIdsToSetToBeingProcessed.Select(siteId => dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(siteId)), true);
//                for (int i = 0; i < metadataIdsToSetToBeingProcessed.Count(); i++)
//                {
//                    Metadata changedMetadata = dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(metadataIdsToSetToBeingProcessed[i]);
//                    Assert.True(changedMetadata.IsBeingDownloaded);
//                }
//                List<string> metadataIdsToUnsetFromBeingProcessed = (new int[] { 0, 1, 8, 11, 18 }).Select(index => VideoMetadata.MetadataList2[index].SiteId).ToList();
//                dbAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(metadataIdsToUnsetFromBeingProcessed.Select(siteId => dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(siteId)), false);
//                for (int i = 0; i < metadataIdsToUnsetFromBeingProcessed.Count(); i++)
//                {
//                    Metadata changedMetadata = dbAdapter.YoutubeMetadataDbCollection.GetBySiteId(metadataIdsToUnsetFromBeingProcessed[i]);
//                    Assert.False(changedMetadata.IsBeingDownloaded);
//                }
//            });
//        }
//    }
//}
