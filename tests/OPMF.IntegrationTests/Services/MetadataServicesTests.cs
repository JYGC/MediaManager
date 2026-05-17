using System;
using System.Collections.Generic;
using MediaManager.Services2;
using OPMF.Database;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class MetadataServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        public MetadataServicesTests(IsolatedAppFolderFixture _) { }

        private class FakeTaskRunner : ITaskRunnerServices
        {
            public readonly List<Metadata> Calls = new();
            public void DownloadOneVideo(Metadata metadata) => Calls.Add(metadata);
        }

        [Fact]
        public void UpdateStatus_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_s", status: MetadataStatus.New);

            DatabaseAdapter.AccessDbAdapter(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            var svc = new MetadataServices(new FakeTaskRunner());
            Metadata stored = null;
            DatabaseAdapter.AccessDbAdapter(db =>
                stored = db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId));
            stored.Status = MetadataStatus.ToDownload;
            svc.UpdateStatus(new[] { stored });

            DatabaseAdapter.AccessDbAdapter(db =>
                Assert.Equal(MetadataStatus.ToDownload, db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId).Status));
        }

        [Fact]
        public void UpdateIsBeingProcessed_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_p");
            item.IsBeingDownloaded = true;

            DatabaseAdapter.AccessDbAdapter(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            new MetadataServices(new FakeTaskRunner()).UpdateIsBeingProcessed(new[] { item });

            DatabaseAdapter.AccessDbAdapter(db =>
                Assert.True(db.YoutubeMetadataDbCollection.GetBySiteId(item.SiteId).IsBeingDownloaded));
        }

        [Fact]
        public void DownloadNow_InvokesTaskRunnerWithMetadata()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_dn");

            DatabaseAdapter.AccessDbAdapter(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            var runner = new FakeTaskRunner();
            new MetadataServices(runner).DownloadNow(item);

            Assert.Single(runner.Calls);
            Assert.Equal(item.SiteId, runner.Calls[0].SiteId);
        }
    }
}
