using System;
using System.Linq;
using OPMF.Entities;
using OPMF.UnitTests.Support;
using Xunit;
using static OPMF.UnitTests.Support.DatabaseTestData;

namespace OPMF.UnitTests.Database
{
    public class YoutubeMetadataDbCollectionOnAnInMemoryDatabaseTests : IDisposable
    {
        private readonly InMemoryDatabaseConnectionOpener _databaseConnectionOpener = new InMemoryDatabaseConnectionOpener();

        public void Dispose() => _databaseConnectionOpener.Dispose();

        [Fact]
        public void InsertNewPersistsAllItems()
        {
            var videos = new[] { Metadata("v1"), Metadata("v2"), Metadata("v3") };

            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(videos);

                foreach (var video in videos)
                {
                    var storedVideo = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId(video.SiteId);
                    Assert.NotNull(storedVideo);
                    Assert.Equal(video.Title, storedVideo.Title);
                    Assert.Equal(video.Url, storedVideo.Url);
                    Assert.Equal(video.ChannelSiteId, storedVideo.ChannelSiteId);
                    Assert.Equal(video.Status, storedVideo.Status);
                }
            });
        }

        [Fact]
        public void InsertNewSkipsDuplicateSiteIdsAndKeepsTheOriginal()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata("dup", title: "Original", status: MetadataStatus.New) });
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata("dup", title: "Should Not Win", status: MetadataStatus.Ignore) });

                var storedVideo = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("dup");
                Assert.Equal("Original", storedVideo.Title);
                Assert.Equal(MetadataStatus.New, storedVideo.Status);
            });
        }

        [Fact]
        public void UpdateStatusAppliesToTheStoredVideos()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata("a"), Metadata("b") });
                var videoA = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("a");
                var videoB = databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("b");
                videoA.Status = MetadataStatus.ToDownload;
                videoB.Status = MetadataStatus.Ignore;

                databaseAdapter.YoutubeMetadataDbCollection.UpdateStatus(new[] { videoA, videoB });

                Assert.Equal(MetadataStatus.ToDownload, databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("a").Status);
                Assert.Equal(MetadataStatus.Ignore, databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("b").Status);
            });
        }

        [Fact]
        public void UpdateIsBeingProcessedWithTrueAndThenFalseRoundTrips()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata("i") });

                databaseAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("i") }, true);
                Assert.True(databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("i").IsBeingDownloaded);

                databaseAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("i") }, false);
                Assert.False(databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("i").IsBeingDownloaded);
            });
        }

        [Fact]
        public void UpdateIsBeingProcessedWithNoFlagPersistsTheValueOfTheItem()
        {
            var video = Metadata("nf");
            video.IsBeingDownloaded = true;

            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { video });

                databaseAdapter.YoutubeMetadataDbCollection.UpdateIsBeingProcessed(new[] { video });

                Assert.True(databaseAdapter.YoutubeMetadataDbCollection.GetBySiteId("nf").IsBeingDownloaded);
            });
        }

        [Fact]
        public void GetToDownloadReturnsOnlyVideosWithTheToDownloadStatus()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[]
                {
                    Metadata("n", status: MetadataStatus.New),
                    Metadata("td1", status: MetadataStatus.ToDownload),
                    Metadata("td2", status: MetadataStatus.ToDownload),
                    Metadata("w", status: MetadataStatus.Wait),
                    Metadata("i", status: MetadataStatus.Ignore),
                    Metadata("d", status: MetadataStatus.Downloaded),
                });

                var videos = databaseAdapter.YoutubeMetadataDbCollection.GetToDownload().ToList();

                Assert.Equal(2, videos.Count);
                Assert.All(videos, video => Assert.Equal(MetadataStatus.ToDownload, video.Status));
            });
        }

        [Fact]
        public void GetNewReturnsOnlyNewVideos()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[]
                {
                    Metadata("n1", status: MetadataStatus.New),
                    Metadata("n2", status: MetadataStatus.New),
                    Metadata("n3", status: MetadataStatus.New),
                    Metadata("td", status: MetadataStatus.ToDownload),
                });

                var videos = databaseAdapter.YoutubeMetadataDbCollection.GetNew(0, int.MaxValue).ToList();

                Assert.Equal(3, videos.Count);
                Assert.All(videos, video => Assert.Equal(MetadataStatus.New, video.Status));
            });
        }

        [Fact]
        public void GetNewSkipsAndLimitsToTheRequestedPage()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata("n1"), Metadata("n2"), Metadata("n3") });

                var videos = databaseAdapter.YoutubeMetadataDbCollection.GetNew(1, 1).ToList();

                Assert.Single(videos);
            });
        }

        [Fact]
        public void GetToDownloadAndWaitReturnsVideosWithEitherStatus()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[]
                {
                    Metadata("n", status: MetadataStatus.New),
                    Metadata("td", status: MetadataStatus.ToDownload),
                    Metadata("w", status: MetadataStatus.Wait),
                    Metadata("d", status: MetadataStatus.Downloaded),
                });

                var videos = databaseAdapter.YoutubeMetadataDbCollection.GetToDownloadAndWait(0, int.MaxValue).ToList();

                Assert.Equal(2, videos.Count);
                Assert.All(videos, video => Assert.True(video.Status == MetadataStatus.ToDownload || video.Status == MetadataStatus.Wait));
            });
        }

        [Fact]
        public void GetManyByWordInTitleOrdersByPublishedAtNewestFirst()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[]
                {
                    Metadata("a", title: "keyword older", publishedAt: new DateTime(2020, 1, 1)),
                    Metadata("b", title: "keyword newer", publishedAt: new DateTime(2024, 1, 1)),
                    Metadata("c", title: "no match", publishedAt: new DateTime(2025, 1, 1)),
                });

                var videos = databaseAdapter.YoutubeMetadataDbCollection.GetManyByWordInTitle("keyword", 0, int.MaxValue).ToList();

                Assert.Equal(new[] { "b", "a" }, videos.Select(video => video.SiteId));
            });
        }

        [Fact]
        public void GetManyByChannelSiteIdAndWordInTitleFiltersByChannelAndOptionallyByTitle()
        {
            _databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            {
                databaseAdapter.YoutubeMetadataDbCollection.InsertNew(new[]
                {
                    Metadata("a1", channelSiteId: "chA", title: "alpha"),
                    Metadata("a2", channelSiteId: "chA", title: "beta"),
                    Metadata("b1", channelSiteId: "chB", title: "alpha"),
                });

                var videosOfOneChannelWithNoWord = databaseAdapter.YoutubeMetadataDbCollection
                    .GetManyByChannelSiteIdAndWordInTitle(new[] { "chA" }, null, 0, int.MaxValue).ToList();
                var videosOfTwoChannelsWithAWord = databaseAdapter.YoutubeMetadataDbCollection
                    .GetManyByChannelSiteIdAndWordInTitle(new[] { "chA", "chB" }, "alpha", 0, int.MaxValue).ToList();

                Assert.Equal(2, videosOfOneChannelWithNoWord.Count);
                Assert.Equal(2, videosOfTwoChannelsWithAWord.Count);
            });
        }
    }
}
