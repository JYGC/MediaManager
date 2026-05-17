using System;
using System.Linq;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;

namespace OPMF.IntegrationTests.Database
{
    public class OPMFLogDbCollectionTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public OPMFLogDbCollectionTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetWarnings_FiltersByTypeAndDateRange_OrderedDescending()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var inside1 = new OPMFLog { Message = marker + "-w1", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 1) };
            var inside2 = new OPMFLog { Message = marker + "-w2", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 15) };
            var outside = new OPMFLog { Message = marker + "-w3", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 8, 1) };
            var wrongType = new OPMFLog { Message = marker + "-e", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.OPMFLogDbCollection.InsertBulk(new[] { inside1, inside2, outside, wrongType });

                var got = db.OPMFLogDbCollection
                    .GetWarnings(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                    .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

                Assert.Equal(2, got.Count);
                Assert.All(got, l => Assert.Equal(OPMFLogType.Warning, l.Type));
                Assert.Equal(inside2.Id, got[0].Id);
                Assert.Equal(inside1.Id, got[1].Id);
            });
        }

        [Fact]
        public void GetErrors_FiltersByTypeAndDateRange_OrderedDescending()
        {
            var marker = Guid.NewGuid().ToString("N")[..6];
            var inside = new OPMFLog { Message = marker + "-e1", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 6, 10) };
            var outside = new OPMFLog { Message = marker + "-e2", Type = OPMFLogType.Error, DateCreated = new DateTime(2024, 1, 1) };
            var wrongType = new OPMFLog { Message = marker + "-w", Type = OPMFLogType.Warning, DateCreated = new DateTime(2024, 6, 10) };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.OPMFLogDbCollection.InsertBulk(new[] { inside, outside, wrongType });

                var got = db.OPMFLogDbCollection
                    .GetErrors(new DateTime(2024, 5, 1), new DateTime(2024, 7, 1))
                    .Where(l => l.Message != null && l.Message.StartsWith(marker)).ToList();

                Assert.Single(got);
                Assert.Equal(inside.Id, got[0].Id);
            });
        }
    }
}
