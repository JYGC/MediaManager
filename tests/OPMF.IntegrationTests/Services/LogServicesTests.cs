using System;
using System.Linq;
using MediaManager.Dtos.Services;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using OPMF.Logging;
using Xunit;

namespace OPMF.IntegrationTests.Services
{
    public class LogServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;

        public LogServicesTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        private LogServices CreateLogServices() =>
            new LogServices(new LogRepository(_fixture.DatabaseConnectionOpener, new Logger(_fixture.DatabaseConnectionOpener), TimeProvider.System));

        [Fact]
        public void GetWarningsLoggedBetween_ReturnsOnlyTheWarningsInTheRange()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var w1 = new OPMFLog { Message = marker + "-w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var e1 = new OPMFLog { Message = marker + "-e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 1) };

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.OPMFLogDbCollection.InsertBulk(new[] { w1, e1 }));

            var got = CreateLogServices()
                .GetWarningsLoggedBetween(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

            Assert.Single(got);
            Assert.Equal(ServiceLogEntryType.Warning, got[0].LogType);
        }

        [Fact]
        public void GetErrorsLoggedBetween_ReturnsOnlyTheErrorsInTheRange()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var w1 = new OPMFLog { Message = marker + "-w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var e1 = new OPMFLog { Message = marker + "-e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 1) };

            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.OPMFLogDbCollection.InsertBulk(new[] { w1, e1 }));

            var got = CreateLogServices()
                .GetErrorsLoggedBetween(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

            Assert.Single(got);
            Assert.Equal(ServiceLogEntryType.Error, got[0].LogType);
        }

        [Fact]
        public void RecordError_StoresAnErrorThatGetErrorsLoggedBetweenReturns()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var logServices = CreateLogServices();

            logServices.RecordError(marker + " failed", "the full exception text");

            var recordedError = Assert.Single(
                logServices.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5)),
                error => error.Message == marker + " failed");
            Assert.Equal("the full exception text", recordedError.ExceptionText);
        }
    }
}
