using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using OPMF.Database;
using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Database
{
    public class DatabaseAdapterOnAnInMemoryDatabaseTests
    {
        [Fact]
        public void LogEntriesInsertedThroughTheLogCollectionAreReadBackFromIt()
        {
            using var liteDatabase = new LiteDatabase(new MemoryStream());
            var databaseAdapter = new DatabaseAdapter(liteDatabase);

            databaseAdapter.OPMFLogDbCollection.InsertBulk(new List<OPMFLog> { new OPMFLog { Message = "first" }, new OPMFLog { Message = "second" } });

            Assert.Equal(new[] { "first", "second" }, databaseAdapter.OPMFLogDbCollection.GetAll().Select(log => log.Message).OrderBy(message => message));
        }

        [Fact]
        public void VideosInsertedThroughTheMetadataCollectionAreReadBackFromIt()
        {
            using var liteDatabase = new LiteDatabase(new MemoryStream());
            var databaseAdapter = new DatabaseAdapter(liteDatabase);

            databaseAdapter.YoutubeMetadataDbCollection.InsertBulk(new List<Metadata> { new YoutubeMetadata { SiteId = "video-1", Title = "A title" } });

            Assert.Equal("A title", databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("video-1").Title);
        }

        [Fact]
        public void ChannelsInsertedThroughTheChannelCollectionAreReadBackFromIt()
        {
            using var liteDatabase = new LiteDatabase(new MemoryStream());
            var databaseAdapter = new DatabaseAdapter(liteDatabase);

            databaseAdapter.YoutubeChannelDbCollection.InsertBulk(new List<Channel> { new YoutubeChannel { SiteId = "channel-1", Name = "A channel" } });

            Assert.Equal("A channel", databaseAdapter.YoutubeChannelDbCollection.GetBySiteId("channel-1").Name);
        }

        [Fact]
        public void DisposingTheAdapterDisposesTheLiteDatabaseItWasGiven()
        {
            var liteDatabase = new LiteDatabase(new MemoryStream());
            var databaseAdapter = new DatabaseAdapter(liteDatabase);

            databaseAdapter.Dispose();

            Assert.ThrowsAny<System.Exception>(() => liteDatabase.GetCollection<OPMFLog>("OPMFLog").Count());
        }
    }
}
