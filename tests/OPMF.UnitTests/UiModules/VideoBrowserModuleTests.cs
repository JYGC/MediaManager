using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaManager.Dtos.Application;
using MediaManagerUI.Modules.VideoBrowser;
using Microsoft.Extensions.Time.Testing;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.UiModules
{
    public abstract class VideoBrowserModuleTests
    {
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        protected const int PageSizeOfTheModules = 10000;

        protected readonly FakeVideoBrowsingUseCases VideoBrowsingUseCases = new FakeVideoBrowsingUseCases();
        protected readonly FakeTimeProvider FakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);

        protected abstract IVideoBrowserModule CreateModuleToBrowseVideos();
        protected abstract void ServeFromTheUseCasesTheChunks(Func<int, int, List<ApplicationVideoWithChannelDto>> chunkForSkipAndPageSize);
        protected abstract string PrefixOfTheUseCaseCallsThatLoadVideos { get; }
        protected abstract ApplicationVideoDownloadStatus[] ExpectedUnselectableDownloadStatuses { get; }

        protected static List<ApplicationVideoWithChannelDto> ChunkOfVideos(string idPrefix, int count)
        {
            return Enumerable.Range(0, count)
                .Select(number => new ApplicationVideoWithChannelDto
                {
                    Video = new ApplicationVideoMetadataDto { SiteId = idPrefix + number, Title = "Video " + number, ChannelSiteId = "c1" },
                    Channel = new ApplicationChannelDto { SiteId = "c1", Name = "Channel" },
                })
                .ToList();
        }

        private static ApplicationVideoWithChannelDto VideoWithChannel(string siteId, bool isBeingDownloaded = false) =>
            new ApplicationVideoWithChannelDto
            {
                Video = new ApplicationVideoMetadataDto { SiteId = siteId, IsBeingDownloaded = isBeingDownloaded },
                Channel = new ApplicationChannelDto { SiteId = "c1" },
            };

        [Fact]
        public async Task TheVideosAreLoadedInChunksOfTenThousandUntilAChunkIsShorter()
        {
            ServeFromTheUseCasesTheChunks((skip, _) => skip switch
            {
                0 => ChunkOfVideos("first_", PageSizeOfTheModules),
                PageSizeOfTheModules => ChunkOfVideos("second_", PageSizeOfTheModules),
                _ => ChunkOfVideos("third_", 5),
            });
            var module = CreateModuleToBrowseVideos();

            await module.GetResultsAsync();

            Assert.Equal(2 * PageSizeOfTheModules + 5, module.Results.Count);
            var chunksAsked = VideoBrowsingUseCases.CallsMade.Where(call => call.StartsWith(PrefixOfTheUseCaseCallsThatLoadVideos)).Count();
            Assert.Equal(3, chunksAsked);
        }

        [Fact]
        public async Task ALoadThatReturnsLessThanAFullChunkAsksTheUseCaseOnce()
        {
            ServeFromTheUseCasesTheChunks((_, _) => ChunkOfVideos("only_", 3));
            var module = CreateModuleToBrowseVideos();

            await module.GetResultsAsync();

            Assert.Equal(3, module.Results.Count);
            Assert.Single(VideoBrowsingUseCases.CallsMade, call => call.StartsWith(PrefixOfTheUseCaseCallsThatLoadVideos));
        }

        [Fact]
        public async Task LoadingAgainReplacesTheEarlierResults()
        {
            ServeFromTheUseCasesTheChunks((_, _) => ChunkOfVideos("a_", 3));
            var module = CreateModuleToBrowseVideos();
            await module.GetResultsAsync();

            ServeFromTheUseCasesTheChunks((_, _) => ChunkOfVideos("b_", 2));
            await module.GetResultsAsync();

            Assert.Equal(new[] { "b_0", "b_1" }, module.Results.Select(videoWithChannel => videoWithChannel.Video.SiteId));
        }

        [Fact]
        public async Task IsLoadingIsTrueWhileLoadingAndFalseAfterwards()
        {
            using var theUseCaseWasAsked = new ManualResetEventSlim();
            using var theTestChecked = new ManualResetEventSlim();
            ServeFromTheUseCasesTheChunks((_, _) =>
            {
                theUseCaseWasAsked.Set();
                theTestChecked.Wait(TimeSpan.FromSeconds(10));
                return ChunkOfVideos("v_", 1);
            });
            var module = CreateModuleToBrowseVideos();

            var loading = module.GetResultsAsync();
            Assert.True(theUseCaseWasAsked.Wait(TimeSpan.FromSeconds(10)));
            var isLoadingWhileTheUseCaseWasWorking = module.IsLoading;
            theTestChecked.Set();
            await loading;

            Assert.True(isLoadingWhileTheUseCaseWasWorking);
            Assert.False(module.IsLoading);
        }

        [Fact]
        public async Task IsLoadingIsFalseAgainAfterTheUseCaseFailedAndTheExceptionPropagates()
        {
            var exceptionOfTheUseCase = new InvalidOperationException("the database is locked");
            ServeFromTheUseCasesTheChunks((_, _) => throw exceptionOfTheUseCase);
            var module = CreateModuleToBrowseVideos();

            var exceptionThatPropagated = await Assert.ThrowsAsync<InvalidOperationException>(module.GetResultsAsync);

            Assert.Same(exceptionOfTheUseCase, exceptionThatPropagated);
            Assert.False(module.IsLoading);
        }

        [Fact]
        public void TheUnselectableStatusesAreThoseOfTheModule()
        {
            Assert.Equal(ExpectedUnselectableDownloadStatuses, CreateModuleToBrowseVideos().UnselectableMetadataStatuses);
        }

        [Fact]
        public void SchedulingAStatusChangeSetsItOnTheVideoAtOnce()
        {
            var module = CreateModuleToBrowseVideos();
            var video = new ApplicationVideoMetadataDto { SiteId = "v1", DownloadStatus = ApplicationVideoDownloadStatus.New };

            module.ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus.ToDownload, video);

            Assert.Equal(ApplicationVideoDownloadStatus.ToDownload, video.DownloadStatus);
            Assert.Empty(VideoBrowsingUseCases.DownloadStatusUpdates);
        }

        [Fact]
        public void AfterOneSecondEveryScheduledStatusChangeIsSentInOneCall()
        {
            var module = CreateModuleToBrowseVideos();
            var firstVideo = new ApplicationVideoMetadataDto { SiteId = "v1" };
            var secondVideo = new ApplicationVideoMetadataDto { SiteId = "v2" };
            module.ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus.ToDownload, firstVideo);
            module.ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus.Ignore, secondVideo);

            FakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

            var oneCall = Assert.Single(VideoBrowsingUseCases.DownloadStatusUpdates);
            Assert.Equal(new[] { "v1", "v2" }, oneCall.Select(video => video.SiteId));
        }

        [Fact]
        public void ScheduledStatusChangesAreSentOnlyOnce()
        {
            var module = CreateModuleToBrowseVideos();
            module.ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus.ToDownload, new ApplicationVideoMetadataDto { SiteId = "v1" });

            FakeTimeProvider.Advance(TimeSpan.FromSeconds(3));

            Assert.Single(VideoBrowsingUseCases.DownloadStatusUpdates);
        }

        [Fact]
        public void NothingIsSentWhenNoStatusChangeIsScheduled()
        {
            CreateModuleToBrowseVideos();

            FakeTimeProvider.Advance(TimeSpan.FromSeconds(5));

            Assert.Empty(VideoBrowsingUseCases.DownloadStatusUpdates);
        }

        [Fact]
        public void NothingIsSentBeforeOneSecondPassed()
        {
            var module = CreateModuleToBrowseVideos();
            module.ScheduleMetadataStatusUpdate(ApplicationVideoDownloadStatus.ToDownload, new ApplicationVideoMetadataDto { SiteId = "v1" });

            FakeTimeProvider.Advance(TimeSpan.FromMilliseconds(999));

            Assert.Empty(VideoBrowsingUseCases.DownloadStatusUpdates);
        }

        [Fact]
        public void DownloadingAVideoNowShowsTheFlagTheUseCaseReturns()
        {
            var module = CreateModuleToBrowseVideos();
            var videoWithChannel = VideoWithChannel("v1");
            VideoBrowsingUseCases.OnMarkVideoAsBeingDownloaded = video => new ApplicationVideoMetadataDto { SiteId = video.SiteId, IsBeingDownloaded = true };

            module.DownloadVideoNow(videoWithChannel);

            Assert.True(videoWithChannel.Video.IsBeingDownloaded);
            Assert.Equal(new[] { "MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(v1)" }, VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void SavingTheDownloadQualityChoiceOverridesOfAVideoPassesTheVideoOnToTheUseCase()
        {
            var module = CreateModuleToBrowseVideos();
            var video = VideoWithChannel("v1").Video;
            video.MaximumVideoResolutionChoiceOverride = ApplicationMaximumVideoResolutionChoice.UpTo720p;

            module.SaveDownloadQualityChoiceOverridesOfVideo(video);

            Assert.Equal(new[] { "SaveDownloadQualityChoiceOverridesOfVideo(v1)" }, VideoBrowsingUseCases.CallsMade);
            Assert.Same(video, Assert.Single(VideoBrowsingUseCases.VideosWhoseDownloadQualityChoiceOverridesWereSaved));
        }

        [Fact]
        public void AFailureToSaveTheDownloadQualityChoiceOverridesOfAVideoReachesTheCaller()
        {
            var module = CreateModuleToBrowseVideos();
            VideoBrowsingUseCases.OnSaveDownloadQualityChoiceOverridesOfVideo = _ => throw new InvalidOperationException("the database is locked");

            var exception = Assert.Throws<InvalidOperationException>(() => module.SaveDownloadQualityChoiceOverridesOfVideo(VideoWithChannel("v1").Video));

            Assert.Equal("the database is locked", exception.Message);
        }

        [Fact]
        public void DownloadingAVideoNowDoesNotShowTheFlagWhenTheUseCaseReturnsItNotSet()
        {
            var module = CreateModuleToBrowseVideos();
            var videoWithChannel = VideoWithChannel("v1");
            VideoBrowsingUseCases.OnMarkVideoAsBeingDownloaded = video => new ApplicationVideoMetadataDto { SiteId = video.SiteId, IsBeingDownloaded = false };

            module.DownloadVideoNow(videoWithChannel);

            Assert.False(videoWithChannel.Video.IsBeingDownloaded);
        }

        [Fact]
        public void RemovingTheBeingDownloadedStatusShowsTheFlagTheUseCaseReturns()
        {
            var module = CreateModuleToBrowseVideos();
            var videoWithChannel = VideoWithChannel("v1", isBeingDownloaded: true);
            VideoBrowsingUseCases.OnClearBeingDownloadedFlagOfVideo = video => new ApplicationVideoMetadataDto { SiteId = video.SiteId, IsBeingDownloaded = false };

            module.RemoveIsBeingDownloadedStatus(videoWithChannel);

            Assert.False(videoWithChannel.Video.IsBeingDownloaded);
            Assert.Equal(new[] { "ClearBeingDownloadedFlagOfVideo(v1)" }, VideoBrowsingUseCases.CallsMade);
        }
    }

    public class NewVideosModuleTests : VideoBrowserModuleTests
    {
        protected override IVideoBrowserModule CreateModuleToBrowseVideos() => new NewVideosModule(VideoBrowsingUseCases, FakeTimeProvider);

        protected override void ServeFromTheUseCasesTheChunks(Func<int, int, List<ApplicationVideoWithChannelDto>> chunkForSkipAndPageSize) =>
            VideoBrowsingUseCases.OnGetNewVideosWithChannels = chunkForSkipAndPageSize;

        protected override string PrefixOfTheUseCaseCallsThatLoadVideos => "GetNewVideosWithChannels(";

        protected override ApplicationVideoDownloadStatus[] ExpectedUnselectableDownloadStatuses => new[] { ApplicationVideoDownloadStatus.Downloaded };
    }

    public class DownloadQueueModuleTests : VideoBrowserModuleTests
    {
        protected override IVideoBrowserModule CreateModuleToBrowseVideos() => new DownloadQueueModule(VideoBrowsingUseCases, FakeTimeProvider);

        protected override void ServeFromTheUseCasesTheChunks(Func<int, int, List<ApplicationVideoWithChannelDto>> chunkForSkipAndPageSize) =>
            VideoBrowsingUseCases.OnGetVideosWithChannelsQueuedToDownloadOrWaiting = chunkForSkipAndPageSize;

        protected override string PrefixOfTheUseCaseCallsThatLoadVideos => "GetVideosWithChannelsQueuedToDownloadOrWaiting(";

        protected override ApplicationVideoDownloadStatus[] ExpectedUnselectableDownloadStatuses =>
            new[] { ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded };
    }

    public class FindVideosModuleByTitleOnlyTests : VideoBrowserModuleTests
    {
        protected override IVideoBrowserModule CreateModuleToBrowseVideos() =>
            new FindVideosModule(VideoBrowsingUseCases, FakeTimeProvider) { SearchVideoTitle = "keyword" };

        protected override void ServeFromTheUseCasesTheChunks(Func<int, int, List<ApplicationVideoWithChannelDto>> chunkForSkipAndPageSize) =>
            VideoBrowsingUseCases.OnGetVideosWithChannelsWhereVideoTitleContainsWord = (_, skip, pageSize) => chunkForSkipAndPageSize(skip, pageSize);

        protected override string PrefixOfTheUseCaseCallsThatLoadVideos => "GetVideosWithChannelsWhereVideoTitleContainsWord(";

        protected override ApplicationVideoDownloadStatus[] ExpectedUnselectableDownloadStatuses =>
            new[] { ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded };
    }

    public class FindVideosModuleSearchTests
    {
        private readonly FakeVideoBrowsingUseCases _videoBrowsingUseCases = new FakeVideoBrowsingUseCases();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero));

        private FindVideosModule CreateModule() => new FindVideosModule(_videoBrowsingUseCases, _fakeTimeProvider);

        private static List<ApplicationVideoWithChannelDto> ChunkOfVideos(int count) =>
            Enumerable.Range(0, count)
                .Select(number => new ApplicationVideoWithChannelDto
                {
                    Video = new ApplicationVideoMetadataDto { SiteId = "v" + number },
                    Channel = new ApplicationChannelDto { SiteId = "c1" },
                })
                .ToList();

        [Fact]
        public async Task ASearchWithOnlyAVideoTitleSearchesByTitleOnly()
        {
            var module = CreateModule();
            module.SearchVideoTitle = "keyword";

            await module.GetResultsAsync();

            Assert.Equal(new[] { "GetVideosWithChannelsWhereVideoTitleContainsWord(keyword, 0, 10000)" }, _videoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public async Task ASearchWithAChannelNameSearchesByChannelAndTitle()
        {
            var module = CreateModule();
            module.SearchChannelName = "channelWord";
            module.SearchVideoTitle = "titleWord";

            await module.GetResultsAsync();

            Assert.Equal(new[] { "GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(channelWord, titleWord, 0, 10000)" }, _videoBrowsingUseCases.CallsMade);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("   ", "  ")]
        public async Task ASearchWithBlankTitleAndBlankChannelNameDoesNothing(string videoTitle, string channelName)
        {
            var module = CreateModule();
            module.SearchVideoTitle = videoTitle;
            module.SearchChannelName = channelName;

            await module.GetResultsAsync();

            Assert.Empty(_videoBrowsingUseCases.CallsMade);
            Assert.False(module.IsLoading);
        }

        [Fact]
        public async Task ABlankSearchKeepsTheEarlierResults()
        {
            _videoBrowsingUseCases.OnGetVideosWithChannelsWhereVideoTitleContainsWord = (_, _, _) => ChunkOfVideos(2);
            var module = CreateModule();
            module.SearchVideoTitle = "keyword";
            await module.GetResultsAsync();
            module.SearchVideoTitle = " ";

            await module.GetResultsAsync();

            Assert.Equal(2, module.Results.Count);
        }
    }

    public class GetSingleModuleTests
    {
        private readonly FakeVideoBrowsingUseCases _videoBrowsingUseCases = new FakeVideoBrowsingUseCases();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero));

        private GetSingleModule CreateModule() => new GetSingleModule(_videoBrowsingUseCases, _fakeTimeProvider);

        [Fact]
        public async Task TheVideoIsLoadedByTheUrl()
        {
            _videoBrowsingUseCases.OnGetVideoWithChannelByUrl = _ => new ApplicationVideoWithChannelDto
            {
                Video = new ApplicationVideoMetadataDto { SiteId = "v1" },
                Channel = new ApplicationChannelDto { SiteId = "c1" },
            };
            var module = CreateModule();
            module.SearchVideoUrl = "https://www.youtube.com/watch?v=v1";

            await module.GetResultsAsync();

            Assert.Equal(new[] { "GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(https://www.youtube.com/watch?v=v1)" }, _videoBrowsingUseCases.CallsMade);
            Assert.Equal("v1", Assert.Single(module.Results).Video.SiteId);
        }

        [Fact]
        public async Task LoadingAgainReplacesTheEarlierResult()
        {
            _videoBrowsingUseCases.OnGetVideoWithChannelByUrl = url => new ApplicationVideoWithChannelDto { Video = new ApplicationVideoMetadataDto { SiteId = url } };
            var module = CreateModule();
            module.SearchVideoUrl = "first";
            await module.GetResultsAsync();
            module.SearchVideoUrl = "second";

            await module.GetResultsAsync();

            Assert.Equal("second", Assert.Single(module.Results).Video.SiteId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ABlankUrlDoesNothing(string videoUrl)
        {
            var module = CreateModule();
            module.SearchVideoUrl = videoUrl;

            await module.GetResultsAsync();

            Assert.Empty(_videoBrowsingUseCases.CallsMade);
            Assert.False(module.IsLoading);
        }

        [Fact]
        public void TheOnlyUnselectableStatusIsDownloaded()
        {
            Assert.Equal(new[] { ApplicationVideoDownloadStatus.Downloaded }, CreateModule().UnselectableMetadataStatuses);
        }

        [Fact]
        public async Task IsLoadingIsFalseAgainAfterTheUseCaseFailed()
        {
            _videoBrowsingUseCases.OnGetVideoWithChannelByUrl = _ => throw new Exception("Failed to get video");
            var module = CreateModule();
            module.SearchVideoUrl = "https://www.youtube.com/watch?v=v1";

            await Assert.ThrowsAsync<Exception>(module.GetResultsAsync);

            Assert.False(module.IsLoading);
        }
    }
}
