using System;
using System.Collections.Generic;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Entities;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Repositories
{
    public class VideoDownloaderRepositoryTests
    {
        private readonly FakeDownloader _fakeDownloader = new FakeDownloader();
        private int _downloadersCreated;
        private Exception _exceptionTheNextDownloaderCreationThrows;

        private VideoDownloaderRepository CreateVideoDownloaderRepository()
        {
            return new VideoDownloaderRepository(() =>
            {
                _downloadersCreated++;
                var exception = _exceptionTheNextDownloaderCreationThrows;
                _exceptionTheNextDownloaderCreationThrows = null;
                if (exception != null)
                {
                    throw exception;
                }
                return _fakeDownloader;
            });
        }

        private static RepositoryVideoMetadataDto VideoDto(string siteId) =>
            new RepositoryVideoMetadataDto { SiteId = siteId, Title = "Video " + siteId, Url = "https://www.youtube.com/watch?v=" + siteId };

        [Fact]
        public void NothingIsCreatedWhenTheRepositoryIsCreated()
        {
            CreateVideoDownloaderRepository();

            Assert.Equal(0, _downloadersCreated);
        }

        [Fact]
        public void PreparingCreatesTheDownloaderOnceHoweverOftenItIsCalled()
        {
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();
            videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();

            Assert.Equal(1, _downloadersCreated);
        }

        [Fact]
        public void PreparingCreatesTheDownloaderAgainAfterItsCreationThrew()
        {
            _exceptionTheNextDownloaderCreationThrows = new InvalidOperationException("the executable could not be downloaded");
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            Assert.Throws<InvalidOperationException>(videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded);
            videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();

            Assert.Equal(2, _downloadersCreated);
        }

        [Fact]
        public void DownloadingPreparesTheDownloaderWhenPreparingWasNotCalledFirst()
        {
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(new List<RepositoryVideoMetadataDto> { VideoDto("v1") });

            Assert.Equal(1, _downloadersCreated);
            Assert.Equal(1, _fakeDownloader.TimesDownloadingWasStarted);
        }

        [Fact]
        public void DownloadingUsesTheDownloaderThatPreparingCreated()
        {
            var videoDownloaderRepository = CreateVideoDownloaderRepository();
            videoDownloaderRepository.PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded();

            videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(new List<RepositoryVideoMetadataDto> { VideoDto("v1") });

            Assert.Equal(1, _downloadersCreated);
        }

        [Fact]
        public void TheVideosAreHandedToTheDownloaderAsYoutubeMetadataInTheSameOrder()
        {
            List<Metadata> queueTheDownloaderReceived = null;
            _fakeDownloader.RunWhileDownloading = queue => queueTheDownloaderReceived = queue.ToList();
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(new List<RepositoryVideoMetadataDto> { VideoDto("v1"), VideoDto("v2") });

            Assert.All(queueTheDownloaderReceived, video => Assert.IsType<YoutubeMetadata>(video));
            Assert.Equal(new[] { "v1", "v2" }, queueTheDownloaderReceived.Select(video => video.SiteId));
        }

        [Fact]
        public void TheResultHoldsTheVideosWithTheDownloadStatusTheDownloaderSet()
        {
            _fakeDownloader.RunWhileDownloading = queue => queue.Single(video => video.SiteId == "v2").Status = MetadataStatus.Downloaded;
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            var videos = videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(new List<RepositoryVideoMetadataDto> { VideoDto("v1"), VideoDto("v2") });

            Assert.Equal(new[] { RepositoryVideoDownloadStatus.New, RepositoryVideoDownloadStatus.Downloaded }, videos.Select(video => video.DownloadStatus));
            Assert.Equal(new[] { "v1", "v2" }, videos.Select(video => video.SiteId));
        }

        [Fact]
        public void AnExceptionOfTheDownloaderPropagatesUnchanged()
        {
            var exceptionOfTheDownloader = new InvalidOperationException("downloading failed");
            _fakeDownloader.RunWhileDownloading = _ => throw exceptionOfTheDownloader;
            var videoDownloaderRepository = CreateVideoDownloaderRepository();

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(
                () => videoDownloaderRepository.DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(new List<RepositoryVideoMetadataDto> { VideoDto("v1") }));

            Assert.Same(exceptionOfTheDownloader, exceptionThatPropagated);
        }
    }
}
