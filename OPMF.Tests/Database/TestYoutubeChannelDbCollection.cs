//using Xunit;
//using OPMF.Database;
//using OPMF.Tests.TestData;
//using OPMF.Entities;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;

//namespace OPMF.Tests.Database
//{
//    [Collection("DatabaseTestSequence2")]
//    public class TestYoutubeChannelDbCollectionGet : IClassFixture<AppFolderFixture>
//    {
//        public TestYoutubeChannelDbCollectionGet()
//        {
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                dbAdapter.YoutubeChannelDbCollection.InsertBulk(ChannelMetadata.ChannelList1);
//            });
//        }

//        [Fact]
//        public void TestInsertNewAndGetAll()
//        {
//            IDictionary<string, IChannel> channelsFromDb = null;
//            DatabaseAdapter.AccessDbAdapter(dbAdapter =>
//            {
//                channelsFromDb = dbAdapter.YoutubeChannelDbCollection.GetAll().ToDictionary(
//                    m => m.SiteId,
//                    m => m
//                );
//            });
//            IDictionary<string, IChannel> channelList1Dict = ChannelMetadata.ChannelList1.ToDictionary(
//                m => m.SiteId,
//                m => m
//            );
//            foreach (string siteId in channelList1Dict.Keys)
//            {
//                Assert.Contains(siteId, channelsFromDb);
//                Assert.Equal(JsonConvert.SerializeObject(channelList1Dict[siteId]), JsonConvert.SerializeObject(channelsFromDb[siteId]));
//            }
//        }
//    }
//}
