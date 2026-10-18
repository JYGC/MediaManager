using System;
using OPMF.Entities;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Database
{
    public class OPMFLogDbCollectionOnAnInMemoryDatabaseTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();

        public void Dispose() => _databaseConnectionOpener.Dispose();

        [Fact]
        public void GetWarningsFiltersByTypeAndDateRangeOrderedByNewestFirst()
        {
            var insideRangeOlder = new OPMFLog { Message = "w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var insideRangeNewer = new OPMFLog { Message = "w2", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 15) };
            var outsideRange = new OPMFLog { Message = "w3", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 8, 1) };
            var wrongType = new OPMFLog { Message = "e", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) };

            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.OPMFLogDbCollection.InsertBulk(new[] { insideRangeOlder, insideRangeNewer, outsideRange, wrongType });

                var warnings = databaseAdapter.OPMFLogDbCollection.GetWarnings(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1));

                Assert.Equal(2, warnings.Count);
                Assert.All(warnings, warning => Assert.Equal(OPMFLogType.Warning, warning.Type));
                Assert.Equal(insideRangeNewer.Id, warnings[0].Id);
                Assert.Equal(insideRangeOlder.Id, warnings[1].Id);
            });
        }

        [Fact]
        public void GetErrorsFiltersByTypeAndDateRange()
        {
            var insideRange = new OPMFLog { Message = "e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) };
            var outsideRange = new OPMFLog { Message = "e2", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 1, 1) };
            var wrongType = new OPMFLog { Message = "w", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 10) };

            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.OPMFLogDbCollection.InsertBulk(new[] { insideRange, outsideRange, wrongType });

                var errors = databaseAdapter.OPMFLogDbCollection.GetErrors(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1));

                Assert.Equal(insideRange.Id, Assert.Single(errors).Id);
            });
        }
    }
}
