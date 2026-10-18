using System;
using System.Collections.Generic;
using MediaManager.Dtos.Services;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.Entities;
using OPMF.IntegrationTests.Support;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class MetadataServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;

        public MetadataServicesTests(IsolatedAppFolderFixture fixture) { _fixture = fixture; }

        private class FakeTaskRunner : ITaskRunnerServices
        {
            public readonly List<string> VideoSiteIdsStarted = new();
            public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId) => VideoSiteIdsStarted.Add(videoSiteId);
        }

        private MetadataServices CreateMetadataServices(ITaskRunnerServices taskRunnerServices) =>
            new MetadataServices(taskRunnerServices, new VideoMetadataRepository(_fixture.DatabaseConnectionOpener));

        private Metadata StoredVideoWithSiteId(string siteId)
        {
            Metadata stored = null;
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => stored = db.YoutubeMetadataDbCollection.GetBySiteId(siteId));
            return stored;
        }

        private static ServiceVideoMetadataDto ServiceVideoDtoOf(Metadata video) => new ServiceVideoMetadataDto
        {
            SiteId = video.SiteId,
            Title = video.Title,
            IsBeingDownloaded = video.IsBeingDownloaded,
            DownloadStatus = ServiceVideoDownloadStatus.New,
        };

        [Fact]
        public void UpdateDownloadStatusOfVideos_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_s", status: MetadataStatus.New);
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            var videoDto = ServiceVideoDtoOf(item);
            videoDto.DownloadStatus = ServiceVideoDownloadStatus.ToDownload;
            CreateMetadataServices(new FakeTaskRunner()).UpdateDownloadStatusOfVideos(new[] { videoDto });

            Assert.Equal(MetadataStatus.ToDownload, StoredVideoWithSiteId(item.SiteId).Status);
        }

        [Fact]
        public void ClearBeingDownloadedFlagOfVideo_PersistsThroughService()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_p");
            item.IsBeingDownloaded = true;
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            var returnedVideo = CreateMetadataServices(new FakeTaskRunner()).ClearBeingDownloadedFlagOfVideo(ServiceVideoDtoOf(item));

            Assert.False(returnedVideo.IsBeingDownloaded);
            Assert.False(StoredVideoWithSiteId(item.SiteId).IsBeingDownloaded);
        }

        [Fact]
        public void MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt_PersistsTheFlagAndStartsTheTaskRunner()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            var item = Metadata(prefix + "_dn");
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => db.YoutubeMetadataDbCollection.InsertNew(new[] { item }));

            var runner = new FakeTaskRunner();
            var returnedVideo = CreateMetadataServices(runner).MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceVideoDtoOf(item));

            Assert.True(returnedVideo.IsBeingDownloaded);
            Assert.True(StoredVideoWithSiteId(item.SiteId).IsBeingDownloaded);
            Assert.Equal(new[] { item.SiteId }, runner.VideoSiteIdsStarted);
        }
    }
}
