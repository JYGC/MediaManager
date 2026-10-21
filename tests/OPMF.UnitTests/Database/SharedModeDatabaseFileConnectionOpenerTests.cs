using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Time.Testing;
using OPMF.Database;
using OPMF.Entities;
using OPMF.TextLogging;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Database
{
    public class SharedModeDatabaseFileConnectionOpenerTests
    {
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private static (SharedModeDatabaseFileConnectionOpener Opener, string DatabaseFilePath, string TextLogFilePath) CreateOpenerOnDatabaseAndTextLogFilesIn(string folderPath)
        {
            var databaseFilePath = Path.Combine(folderPath, "OPMF.db");
            var textLogFilePath = Path.Combine(folderPath, "log.txt");
            var textLog = new TextLog(textLogFilePath, new FakeTimeProvider(InstantTheFakeClockStartsAt));
            return (new SharedModeDatabaseFileConnectionOpener(databaseFilePath, textLog), databaseFilePath, textLogFilePath);
        }

        private static string ReadWholeFileWhileTheTextLogStillHasItOpen(string textLogFilePath)
        {
            using var fileStream = new FileStream(textLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        [Fact]
        public void TheActionGetsADatabaseAdapterThatStoresDocumentsAcrossCalls()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var (opener, _, _) = CreateOpenerOnDatabaseAndTextLogFilesIn(temporaryFolder.FolderPath);
            var logEntryToStore = new OPMFLog { Message = "stored through the opener" };

            opener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => databaseAdapter.OPMFLogDbCollection.InsertBulk(new List<OPMFLog> { logEntryToStore }));
            List<OPMFLog> logEntriesReadBack = null;
            opener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => logEntriesReadBack = databaseAdapter.OPMFLogDbCollection.GetAll().ToList());

            Assert.Equal("stored through the opener", Assert.Single(logEntriesReadBack).Message);
        }

        [Fact]
        public void TheConnectionIsClosedAfterTheCall()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var (opener, databaseFilePath, _) = CreateOpenerOnDatabaseAndTextLogFilesIn(temporaryFolder.FolderPath);

            opener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => databaseAdapter.OPMFLogDbCollection.InsertBulk(new List<OPMFLog> { new OPMFLog() }));

            using var fileOpenedWithNoSharing = new FileStream(databaseFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(fileOpenedWithNoSharing.CanWrite);
        }

        [Fact]
        public void WhenTheActionThrowsTheExceptionIsAppendedToTheTextLog()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var (opener, _, textLogFilePath) = CreateOpenerOnDatabaseAndTextLogFilesIn(temporaryFolder.FolderPath);
            var exceptionThrownByTheAction = new InvalidOperationException("the action failed");

            Assert.Throws<InvalidOperationException>(() => opener.RunWithDatabaseConnectionOpenForThisCallOnly(_ => throw exceptionThrownByTheAction));

            Assert.Contains("System.InvalidOperationException: the action failed", ReadWholeFileWhileTheTextLogStillHasItOpen(textLogFilePath));
        }

        [Fact]
        public void WhenTheActionThrowsTheSameExceptionIsRethrown()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var (opener, _, _) = CreateOpenerOnDatabaseAndTextLogFilesIn(temporaryFolder.FolderPath);
            var exceptionThrownByTheAction = new InvalidOperationException("the action failed");

            var exceptionRethrown = Assert.Throws<InvalidOperationException>(() => opener.RunWithDatabaseConnectionOpenForThisCallOnly(_ => throw exceptionThrownByTheAction));

            Assert.Same(exceptionThrownByTheAction, exceptionRethrown);
        }

        [Fact]
        public void WhenTheActionThrowsTheConnectionIsStillClosed()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var (opener, databaseFilePath, _) = CreateOpenerOnDatabaseAndTextLogFilesIn(temporaryFolder.FolderPath);

            Assert.Throws<InvalidOperationException>(() => opener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.OPMFLogDbCollection.InsertBulk(new List<OPMFLog> { new OPMFLog() });
                throw new InvalidOperationException("the action failed");
            }));

            using var fileOpenedWithNoSharing = new FileStream(databaseFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(fileOpenedWithNoSharing.CanWrite);
        }
    }
}
