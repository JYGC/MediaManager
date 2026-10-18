using System;
using System.IO;
using LiteDB;
using OPMF.Database;

namespace OPMF.UnitTests.Support
{
    public class InMemoryDatabaseConnectionOpener : IDatabaseConnectionOpener, IDisposable
    {
        private readonly DatabaseAdapter _databaseAdapterHandedToEveryCall;

        public LiteDatabase LiteDatabaseKeptOpenBetweenCalls { get; }

        public InMemoryDatabaseConnectionOpener(BsonMapper bsonMapper = null)
        {
            LiteDatabaseKeptOpenBetweenCalls = new LiteDatabase(new MemoryStream(), bsonMapper);
            _databaseAdapterHandedToEveryCall = new DatabaseAdapter(LiteDatabaseKeptOpenBetweenCalls);
        }

        public void RunWithDatabaseConnectionOpenForThisCallOnly(Action<DatabaseAdapter> useDatabaseAdapter)
        {
            useDatabaseAdapter(_databaseAdapterHandedToEveryCall);
        }

        public void Dispose()
        {
            LiteDatabaseKeptOpenBetweenCalls.Dispose();
        }
    }
}
