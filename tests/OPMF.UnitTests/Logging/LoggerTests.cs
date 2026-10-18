using System;
using System.Linq;
using LiteDB;
using OPMF.Entities;
using OPMF.Logging;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Logging
{
    public class LoggerTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();

        public void Dispose() => _databaseConnectionOpener.Dispose();

        [Fact]
        public void LogEntryStoresTheEntryInTheOPMFLogCollection()
        {
            var logger = new Logger(_databaseConnectionOpener);
            var logEntry = new OPMFLog { Message = "something happened", Type = OPMFLogType.Warning };

            logger.LogEntry(logEntry);

            OPMFLog storedLogEntry = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => storedLogEntry = databaseAdapter.OPMFLogDbCollection.GetAll().Single());
            Assert.Equal(logEntry.Id, storedLogEntry.Id);
            Assert.Equal("something happened", storedLogEntry.Message);
            Assert.Equal(OPMFLogType.Warning, storedLogEntry.Type);
        }

        [Fact]
        public void LogEntryOfAnOPMFErrorKeepsItsTypeLabelInTheStoredDocument()
        {
            var logger = new Logger(_databaseConnectionOpener);

            logger.LogEntry(new OPMFError(new InvalidOperationException("it failed")));

            var storedDocument = _databaseConnectionOpener.LiteDatabaseKeptOpenBetweenCalls.GetCollection("OPMFLog").FindAll().Single();
            Assert.Equal("OPMF.Entities.OPMFError, OPMF.Entities", storedDocument["_type"].AsString);
        }
    }
}
