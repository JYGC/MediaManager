using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using Microsoft.Extensions.Time.Testing;
using OPMF.Entities;
using OPMF.Logging;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Repositories
{
    public class LogRepositoryTests : IDisposable
    {
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
        private readonly LogRepository _logRepository;

        public LogRepositoryTests()
        {
            _logRepository = new LogRepository(_databaseConnectionOpener, new Logger(_databaseConnectionOpener), _fakeTimeProvider);
        }

        public void Dispose() => _databaseConnectionOpener.Dispose();

        private void StoreLogs(params OPMFLog[] logs)
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => databaseAdapter.OPMFLogDbCollection.InsertBulk(logs));
        }

        [Fact]
        public void GetWarningsLoggedBetweenReturnsTheWarningsInTheRangeNewestFirstAsDtos()
        {
            StoreLogs(
                new OPMFLog { Id = "older", Message = "w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) },
                new OPMFLog { Id = "newer", Message = "w2", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 15) },
                new OPMFLog { Id = "outside", Message = "w3", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 8, 1) },
                new OPMFLog { Id = "error", Message = "e", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) });

            var warnings = _logRepository.GetWarningsLoggedBetween(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1));

            Assert.Equal(new[] { "newer", "older" }, warnings.Select(warning => warning.Id));
            Assert.All(warnings, warning => Assert.Equal(RepositoryLogEntryType.Warning, warning.LogType));
        }

        [Fact]
        public void GetErrorsLoggedBetweenReturnsTheErrorsInTheRangeAsDtos()
        {
            StoreLogs(
                new OPMFLog { Id = "inside", Message = "e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) },
                new OPMFLog { Id = "outside", Message = "e2", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 1, 1) },
                new OPMFLog { Id = "warning", Message = "w", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 10) });

            var errors = _logRepository.GetErrorsLoggedBetween(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1));

            var error = Assert.Single(errors);
            Assert.Equal("inside", error.Id);
            Assert.Equal(RepositoryLogEntryType.Error, error.LogType);
        }

        [Fact]
        public void TheVariablesOfAReadLogEntryAreIndentedJson()
        {
            StoreLogs(new OPMFLog { Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10), Variables = new Dictionary<string, object> { { "key", "value" } } });

            var error = Assert.Single(_logRepository.GetErrorsLoggedBetween(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1)));

            Assert.Equal("{\n  \"key\": \"value\"\n}", error.VariablesAsIndentedJson.ReplaceLineEndings("\n"));
        }

        [Fact]
        public void RecordErrorLogEntryStoresAnOPMFErrorWithTheClocksTimeAndTheVariablesAsStrings()
        {
            var errorToRecord = new RepositoryErrorLogEntryToRecordDto
            {
                Message = "it failed",
                ExceptionText = "the full exception text",
                VariablesAsJsonText = new Dictionary<string, string> { { "channels", "[]" } },
            };

            _logRepository.RecordErrorLogEntry(errorToRecord);

            OPMFLog storedEntry = null;
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter => storedEntry = databaseAdapter.OPMFLogDbCollection.GetAll().Single());
            Assert.Equal(OPMFLogType.Error, storedEntry.Type);
            Assert.Equal("it failed", storedEntry.Message);
            Assert.Equal("the full exception text", storedEntry.ExceptionObject);
            Assert.Equal(_fakeTimeProvider.GetLocalNow().LocalDateTime, storedEntry.DateCreated);
            Assert.Equal("[]", storedEntry.Variables["channels"]);
        }

        [Fact]
        public void ARecordedErrorIsStoredWithTheOPMFErrorTypeLabel()
        {
            _logRepository.RecordErrorLogEntry(new RepositoryErrorLogEntryToRecordDto { Message = "it failed" });

            var storedDocument = _databaseConnectionOpener.LiteDatabaseKeptOpenBetweenCalls.GetCollection("OPMFLog").FindAll().Single();
            Assert.Equal("OPMF.Entities.OPMFError, OPMF.Entities", storedDocument["_type"].AsString);
        }
    }
}
