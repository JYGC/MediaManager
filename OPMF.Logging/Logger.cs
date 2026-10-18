using OPMF.Database;
using OPMF.Entities;
using System.Collections.Generic;

namespace OPMF.Logging
{
    public class Logger
    {
        private readonly IDatabaseConnectionOpener _databaseConnectionOpener;

        public Logger(IDatabaseConnectionOpener databaseConnectionOpener)
        {
            _databaseConnectionOpener = databaseConnectionOpener;
        }

        public void LogEntry(OPMFLog logEntry)
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(dbAdapter =>
            {
                dbAdapter.OPMFLogDbCollection.InsertBulk(new List<OPMFLog> { logEntry });
            });
        }
    }
}
