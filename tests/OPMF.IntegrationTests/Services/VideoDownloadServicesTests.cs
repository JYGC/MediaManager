using System;
using System.IO;
using System.Linq;
using MediaManager.Repositories;
using MediaManager.Services2;
using OPMF.Entities;
using OPMF.Filesystem;
using OPMF.IntegrationTests.Support;
using OPMF.Logging;
using OPMF.Settings;
using Xunit;
using static OPMF.IntegrationTests.Support.TestData;

namespace OPMF.IntegrationTests.Services
{
    public class VideoDownloadServicesTests : IClassFixture<IsolatedAppFolderFixture>
    {
        private readonly IsolatedAppFolderFixture _fixture;
        private readonly FakeVideoDownloaderRepository _fakeVideoDownloaderRepository = new FakeVideoDownloaderRepository();
        private readonly LogRepository _logRepository;
        private readonly AppConfig _appConfig = new AppConfig();

        public VideoDownloadServicesTests(IsolatedAppFolderFixture fixture)
        {
            _fixture = fixture;
            _logRepository = new LogRepository(fixture.DatabaseConnectionOpener, new Logger(fixture.DatabaseConnectionOpener), TimeProvider.System);
            _appConfig.VideoOutputFolderPath = Path.Combine(fixture.ReadonlySettings.GetLocalAppFolderPath(), "VideoOutput");
        }

        private VideoDownloadServices CreateVideoDownloadServices() =>
            new VideoDownloadServices(
                _fakeVideoDownloaderRepository,
                new VideoMetadataRepository(_fixture.DatabaseConnectionOpener),
                _logRepository,
                new MissingFolderCreator(),
                new DownloadedVideoFileMover(),
                _fixture.ReadonlySettings,
                _appConfig,
                new StringWriter());

        private Metadata StoredVideoWithSiteId(string siteId)
        {
            Metadata stored = null;
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db => stored = db.YoutubeMetadataDbCollection.GetBySiteId(siteId));
            return stored;
        }

        [Fact]
        public void TheQueuedVideosEndUpDownloadedWithTheirFlagClearedAndTheirFilesMovedToTheOutputFolder()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(prefix + "_queued", status: MetadataStatus.ToDownload) }));
            var fileNameOfTheDownloadedVideo = prefix + "_video.mp4";
            _fakeVideoDownloaderRepository.RunWhileDownloading = () =>
                File.WriteAllText(Path.Combine(_fixture.ReadonlySettings.GetDownloadFolderPath(), fileNameOfTheDownloadedVideo), "the video bytes");
            bool flagWasSetWhileDownloading = false;
            var downloadingAction = _fakeVideoDownloaderRepository.RunWhileDownloading;
            _fakeVideoDownloaderRepository.RunWhileDownloading = () =>
            {
                flagWasSetWhileDownloading = StoredVideoWithSiteId(prefix + "_queued").IsBeingDownloaded;
                downloadingAction();
            };

            CreateVideoDownloadServices().DownloadEveryVideoQueuedToDownload();

            Assert.True(flagWasSetWhileDownloading);
            var storedVideo = StoredVideoWithSiteId(prefix + "_queued");
            Assert.Equal(MetadataStatus.Downloaded, storedVideo.Status);
            Assert.False(storedVideo.IsBeingDownloaded);
            Assert.Equal("the video bytes", File.ReadAllText(Path.Combine(_appConfig.VideoOutputFolderPath, fileNameOfTheDownloadedVideo)));
        }

        [Fact]
        public void DownloadingTheFirstVideoWhoseSiteIdContainsAPartOfItDownloadsThatVideo()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(prefix + "_FullSiteId", status: MetadataStatus.New) }));

            CreateVideoDownloadServices().DownloadFirstVideoWhoseSiteIdContains(prefix + "_Full");

            Assert.Equal(MetadataStatus.Downloaded, StoredVideoWithSiteId(prefix + "_FullSiteId").Status);
        }

        [Fact]
        public void AFailureRecordsAnErrorEntryWithTheQueuedVideosAsJsonTextAndRethrowsTheOriginalException()
        {
            var prefix = Guid.NewGuid().ToString("N")[..6];
            _fixture.DatabaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(db =>
                db.YoutubeMetadataDbCollection.InsertNew(new[] { Metadata(prefix + "_failing", status: MetadataStatus.ToDownload) }));
            var exceptionOfTheDownloader = new InvalidOperationException(prefix + " downloading failed");
            _fakeVideoDownloaderRepository.ExceptionToThrowWhenDownloading = exceptionOfTheDownloader;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(() => CreateVideoDownloadServices().DownloadEveryVideoQueuedToDownload());

            Assert.Same(exceptionOfTheDownloader, exceptionThatPropagated);
            var recordedErrors = _logRepository.GetErrorsLoggedBetween(DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(5));
            var recordedError = Assert.Single(recordedErrors, error => error.Message == prefix + " downloading failed");
            Assert.Contains("\"SiteId\": \"" + prefix + "_failing\"", recordedError.VariablesAsIndentedJson);
        }
    }
}
