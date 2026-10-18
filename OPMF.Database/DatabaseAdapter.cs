using LiteDB;
using System;

namespace OPMF.Database
{
    public class DatabaseAdapter : IDisposable
    {
        private const string SharedConnectionMode = "shared";

        private readonly LiteDatabase _liteDatabase;

        public YoutubeMetadataDbCollection YoutubeMetadataDbCollection { get; } = null;
        public YoutubeChannelDbCollection YoutubeChannelDbCollection { get; } = null;
        public OPMFLogDbCollection OPMFLogDbCollection { get; } = null;

        public DatabaseAdapter(string databaseFilePath) : this(new LiteDatabase(string.Format(@"Filename={0};connection={1}", databaseFilePath, SharedConnectionMode)))
        {
        }

        public DatabaseAdapter(LiteDatabase liteDatabase)
        {
            _liteDatabase = liteDatabase;

            YoutubeMetadataDbCollection = new YoutubeMetadataDbCollection(_liteDatabase);
            YoutubeChannelDbCollection = new YoutubeChannelDbCollection(_liteDatabase);
            OPMFLogDbCollection = new OPMFLogDbCollection(_liteDatabase);
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}
