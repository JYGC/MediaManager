using System;
using System.Linq;
using MediaManager.Services2;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;

namespace OPMF.IntegrationTests.Services
{
    public class LogServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public LogServicesTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetWarnings_DelegatesToDbCollection()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var w1 = new OPMFLog { Message = marker + "-w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var e1 = new OPMFLog { Message = marker + "-e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 1) };

            DatabaseAdapter.AccessDbAdapter(db => db.OPMFLogDbCollection.InsertBulk(new[] { w1, e1 }));

            var got = new LogServices()
                .GetWarnings(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

            Assert.Single(got);
            Assert.Equal(OPMFLogType.Warning, got[0].Type);
        }

        [Fact]
        public void GetErrors_DelegatesToDbCollection()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var w1 = new OPMFLog { Message = marker + "-w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var e1 = new OPMFLog { Message = marker + "-e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 1) };

            DatabaseAdapter.AccessDbAdapter(db => db.OPMFLogDbCollection.InsertBulk(new[] { w1, e1 }));

            var got = new LogServices()
                .GetErrors(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

            Assert.Single(got);
            Assert.Equal(OPMFLogType.Error, got[0].Type);
        }
    }
}
