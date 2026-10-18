using System;
using OPMF.TextLogging;

namespace OPMF.Database
{
    public interface IDatabaseConnectionOpener
    {
        void RunWithDatabaseConnectionOpenForThisCallOnly(Action<DatabaseAdapter> useDatabaseAdapter);
    }

    public class SharedModeDatabaseFileConnectionOpener : IDatabaseConnectionOpener
    {
        private readonly string _databaseFilePath;
        private readonly TextLog _textLog;

        public SharedModeDatabaseFileConnectionOpener(string databaseFilePath, TextLog textLog)
        {
            _databaseFilePath = databaseFilePath;
            _textLog = textLog;
        }

        public void RunWithDatabaseConnectionOpenForThisCallOnly(Action<DatabaseAdapter> useDatabaseAdapter)
        {
            try
            {
                using DatabaseAdapter databaseAdapter = new DatabaseAdapter(_databaseFilePath);
                useDatabaseAdapter(databaseAdapter);
            }
            catch (Exception e)
            {
                _textLog.LogEntry(e.ToString());
                throw;
            }
        }
    }
}
