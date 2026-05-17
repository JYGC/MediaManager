using System;
using System.Collections.Generic;
using System.Linq;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Database
{
    public class YoutubeMetadataDbCollectionInsertTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeMetadataDbCollectionInsertTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void InsertNew_PersistsAllItems()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var items = new[]
            {
                Metadata(prefix + "_v1"),
                Metadata(prefix + "_v2"),
                Metadata(prefix + "_v3"),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);
                foreach (var item in items)
                {
                    var stored = db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId);
                    Assert.NotNull(stored);
                    Assert.Equal(item.Title, stored.Title);
                    Assert.Equal(item.Url, stored.Url);
                    Assert.Equal(item.ChannelSiteId, stored.ChannelSiteId);
                    Assert.Equal(item.Status, stored.Status);
                }
            });
        }

        [Fact]
        public void InsertNew_SkipsDuplicateSiteIds_AndKeepsOriginal()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var original = Metadata(prefix + "_dup", title: "Original", status: MetadataStatus.New);
            var duplicate = Metadata(prefix + "_dup", title: "Should Not Win", status: MetadataStatus.Ignore);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(new[] { original });
                db.YoutubeMetadataDbCollection.InsertNew(new[] { duplicate });

                var stored = db.YoutubeMetadataDbCollection.GetBySiteId(prefix + "_dup");
                Assert.Equal("Original", stored.Title);
                Assert.Equal(MetadataStatus.New, stored.Status);
            });
        }
    }

    public class YoutubeMetadataDbCollectionUpdateStatusTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeMetadataDbCollectionUpdateStatusTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void UpdateStatus_AppliesToStoredRows()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var a = Metadata(prefix + "_a", status: MetadataStatus.New);
            var b = Metadata(prefix + "_b", status: MetadataStatus.New);

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(new[] { a, b });

                var fromDb = new[]
                {
                    db.YoutubeMetadataDbCollection.GetBySiteId(a.SiteId),
                    db.YoutubeMetadataDbCollection.GetBySiteId(b.SiteId),
                };
                fromDb[0].Status = MetadataStatus.ToDownload;
                fromDb[1].Status = MetadataStatus.Ignore;
                db.YoutubeMetadataDbCollection.UpdateStatus(fromDb);

                Assert.Equal(MetadataStatus.ToDownload, db.YoutubeMetadataDbCollection.GetBySiteId(a.SiteId).Status);
                Assert.Equal(MetadataStatus.Ignore, db.YoutubeMetadataDbCollection.GetBySiteId(b.SiteId).Status);
            });
        }
    }

    public class YoutubeMetadataDbCollectionUpdateIsBeingProcessedTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeMetadataDbCollectionUpdateIsBeingProcessedTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void UpdateIsBeingProcessed_TrueAndFalse_RoundTrip()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_i");

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(new[] { item });

                db.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId) }, true);
                Assert.True(db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId).IsBeingDownloaded);

                db.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId) }, false);
                Assert.False(db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId).IsBeingDownloaded);
            });
        }

        [Fact]
        public void UpdateIsBeingProcessed_NoFlag_PersistsItemValue()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_nf");
            item.IsBeingDownloaded = true;

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(new[] { item });
                db.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { item });
                Assert.True(db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId).IsBeingDownloaded);
            });
        }
    }

    public class YoutubeMetadataDbCollectionQueryTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public YoutubeMetadataDbCollectionQueryTests(IsolatedAppFolderFixture _) { }

        [Fact]
        public void GetToDownload_ReturnsOnlyToDownload()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var items = new[]
            {
                Metadata(prefix + "_n", status: MetadataStatus.New),
                Metadata(prefix + "_td1", status: MetadataStatus.ToDownload),
                Metadata(prefix + "_td2", status: MetadataStatus.ToDownload),
                Metadata(prefix + "_w", status: MetadataStatus.Wait),
                Metadata(prefix + "_i", status: MetadataStatus.Ignore),
                Metadata(prefix + "_d", status: MetadataStatus.Downloaded),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);
                var got = db.YoutubeMetadataDbCollection.GetToDownload()
                    .Where(m => m.SiteId.StartsWith(prefix))
                    .ToList();

                Assert.Equal(2, got.Count);
                Assert.All(got, m => Assert.Equal(MetadataStatus.ToDownload, m.Status));
            });
        }

        [Fact]
        public void GetNew_ReturnsOnlyNewWithPaging()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var items = new[]
            {
                Metadata(prefix + "_n1", status: MetadataStatus.New),
                Metadata(prefix + "_n2", status: MetadataStatus.New),
                Metadata(prefix + "_n3", status: MetadataStatus.New),
                Metadata(prefix + "_td", status: MetadataStatus.ToDownload),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);

                var all = db.YoutubeMetadataDbCollection.GetNew(0, int.MaxValue)
                    .Where(m => m.SiteId.StartsWith(prefix)).ToList();
                Assert.Equal(3, all.Count);
                Assert.All(all, m => Assert.Equal(MetadataStatus.New, m.Status));
            });
        }

        [Fact]
        public void GetToDownloadAndWait_ReturnsToDownloadOrWait()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var items = new[]
            {
                Metadata(prefix + "_n", status: MetadataStatus.New),
                Metadata(prefix + "_td", status: MetadataStatus.ToDownload),
                Metadata(prefix + "_w", status: MetadataStatus.Wait),
                Metadata(prefix + "_d", status: MetadataStatus.Downloaded),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);
                var got = db.YoutubeMetadataDbCollection.GetToDownloadAndWait(0, int.MaxValue)
                    .Where(m => m.SiteId.StartsWith(prefix)).ToList();

                Assert.Equal(2, got.Count);
                Assert.All(got, m => Assert.True(m.Status == MetadataStatus.ToDownload || m.Status == MetadataStatus.Wait));
            });
        }

        [Fact]
        public void GetManyByWordInTitle_OrdersByPublishedAtDescending()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var keyword = "Unique" + prefix;
            var items = new[]
            {
                Metadata(prefix + "_a", title: keyword + " older", publishedAt: new DateTime(2020, 1, 1)),
                Metadata(prefix + "_b", title: keyword + " newer", publishedAt: new DateTime(2024, 1, 1)),
                Metadata(prefix + "_c", title: "no match", publishedAt: new DateTime(2025, 1, 1)),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);
                var got = db.YoutubeMetadataDbCollection.GetManyByWordInTitle(keyword, 0, int.MaxValue).ToList();
                Assert.Equal(2, got.Count);
                Assert.Equal(prefix + "_b", got[0].SiteId);
                Assert.Equal(prefix + "_a", got[1].SiteId);
            });
        }

        [Fact]
        public void GetManyByChannelSiteIdAndWordInTitle_FiltersByChannel()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var chanA = prefix + "_chA";
            var chanB = prefix + "_chB";
            var items = new[]
            {
                Metadata(prefix + "_a1", channelSiteId: chanA, title: "alpha"),
                Metadata(prefix + "_a2", channelSiteId: chanA, title: "beta"),
                Metadata(prefix + "_b1", channelSiteId: chanB, title: "alpha"),
            };

            DatabaseAdapter.AccessDbAdapter(db =>
            {
                db.YoutubeMetadataDbCollection.InsertNew(items);

                var noWord = db.YoutubeMetadataDbCollection
                    .GetManyByChannelSiteIdAndWordInTitle(new[] { chanA }, null, 0, int.MaxValue).ToList();
                Assert.Equal(2, noWord.Count);

                var withWord = db.YoutubeMetadataDbCollection
                    .GetManyByChannelSiteIdAndWordInTitle(new[] { chanA, chanB }, "alpha", 0, int.MaxValue).ToList();
                Assert.Equal(2, withWord.Count);
            });
        }
    }
}
