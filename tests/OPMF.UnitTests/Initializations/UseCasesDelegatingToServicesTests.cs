using System;
using System.Linq;
using MediaManager.Dtos.Application;
using MediaManager.Dtos.Services;
using MediaManager.Initializations;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Initializations
{
    public class UnhandledErrorRecordingUseCaseTests
    {
        [Fact]
        public void RecordUnhandledErrorPassesTheMessageAndTheFullTextOfTheException()
        {
            var fakeLogServices = new FakeLogServices();
            var unhandledException = new InvalidOperationException("it failed");

            new UnhandledErrorRecordingUseCase(fakeLogServices).RecordUnhandledError(unhandledException);

            var (errorMessage, fullExceptionText) = Assert.Single(fakeLogServices.ErrorsRecorded);
            Assert.Equal("it failed", errorMessage);
            Assert.Equal(unhandledException.ToString(), fullExceptionText);
        }
    }

    public class CommandLineTaskUseCasesTests
    {
        private readonly RecordingNewVideoMetadataFetchServices _newVideoMetadataFetchServices = new RecordingNewVideoMetadataFetchServices();
        private readonly RecordingVideoDownloadServices _videoDownloadServices = new RecordingVideoDownloadServices();
        private readonly CommandLineTaskUseCases _commandLineTaskUseCases;

        public CommandLineTaskUseCasesTests()
        {
            _commandLineTaskUseCases = new CommandLineTaskUseCases(_newVideoMetadataFetchServices, _videoDownloadServices);
        }

        [Fact]
        public void FetchingNewVideoMetadataIsPassedToTheService()
        {
            _commandLineTaskUseCases.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();

            Assert.Equal(1, _newVideoMetadataFetchServices.TimesFetchedAndStored);
        }

        [Fact]
        public void DownloadingEveryQueuedVideoIsPassedToTheService()
        {
            _commandLineTaskUseCases.DownloadEveryVideoQueuedToDownload();

            Assert.Equal(1, _videoDownloadServices.TimesEveryQueuedVideoWasDownloaded);
        }

        [Fact]
        public void DownloadingOneVideoPassesTheSiteIdOnToTheService()
        {
            _commandLineTaskUseCases.DownloadFirstVideoWhoseSiteIdContains("abc123");

            Assert.Equal(new[] { "abc123" }, _videoDownloadServices.SiteIdsOfVideosDownloadedOnTheirOwn);
        }
    }

    public class ChannelBrowsingUseCasesTests
    {
        private readonly FakeChannelServices _fakeChannelServices = new FakeChannelServices();
        private readonly ChannelBrowsingUseCases _channelBrowsingUseCases;

        public ChannelBrowsingUseCasesTests()
        {
            _channelBrowsingUseCases = new ChannelBrowsingUseCases(_fakeChannelServices);
        }

        [Fact]
        public void GetAllChannelsReturnsTheServicesChannelsAsApplicationDtos()
        {
            _fakeChannelServices.ChannelsToReturn.Add(new ServiceChannelDto { SiteId = "c1", Name = "One", IsBlacklisted = true });

            var channels = _channelBrowsingUseCases.GetAllChannels();

            var channel = Assert.Single(channels);
            Assert.Equal("One", channel.Name);
            Assert.True(channel.IsBlacklisted);
        }

        [Fact]
        public void UpdateBlacklistFlagOfChannelsPassesTheChannelsToTheServiceAsServiceDtos()
        {
            _channelBrowsingUseCases.UpdateBlacklistFlagOfChannels(new[]
            {
                new ApplicationChannelDto { SiteId = "c1", IsBlacklisted = true },
                new ApplicationChannelDto { SiteId = "c2", IsBlacklisted = false },
            });

            Assert.Equal(new[] { ("c1", true), ("c2", false) }, _fakeChannelServices.ChannelsWhoseBlacklistFlagWasUpdated.Select(channel => (channel.SiteId, channel.IsBlacklisted)));
        }
    }

    public class VideoBrowsingUseCasesTests
    {
        private readonly FakeChannelMetadataServices _fakeChannelMetadataServices = new FakeChannelMetadataServices();
        private readonly FakeMetadataServices _fakeMetadataServices = new FakeMetadataServices();
        private readonly VideoBrowsingUseCases _videoBrowsingUseCases;

        public VideoBrowsingUseCasesTests()
        {
            _videoBrowsingUseCases = new VideoBrowsingUseCases(_fakeChannelMetadataServices, _fakeMetadataServices);
            _fakeChannelMetadataServices.VideosWithChannelsToReturn.Add(new ServiceVideoWithChannelDto
            {
                Video = new ServiceVideoMetadataDto { SiteId = "v1", Title = "The video", DownloadStatus = ServiceVideoDownloadStatus.Wait },
                Channel = new ServiceChannelDto { SiteId = "c1", Name = "The channel" },
            });
        }

        private static void AssertConvertedFromTheService(ApplicationVideoWithChannelDto videoWithChannel)
        {
            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Equal(ApplicationVideoDownloadStatus.Wait, videoWithChannel.Video.DownloadStatus);
            Assert.Equal("The channel", videoWithChannel.Channel.Name);
        }

        [Fact]
        public void GetNewVideosWithChannelsPassesThePageOnAndConvertsTheResult()
        {
            var videosWithChannels = _videoBrowsingUseCases.GetNewVideosWithChannels(20, 10);

            Assert.Equal(new[] { "GetNewVideosWithChannels(20, 10)" }, _fakeChannelMetadataServices.CallsMade);
            AssertConvertedFromTheService(Assert.Single(videosWithChannels));
        }

        [Fact]
        public void GetVideosWithChannelsQueuedToDownloadOrWaitingPassesThePageOnAndConvertsTheResult()
        {
            var videosWithChannels = _videoBrowsingUseCases.GetVideosWithChannelsQueuedToDownloadOrWaiting(20, 10);

            Assert.Equal(new[] { "GetVideosWithChannelsQueuedToDownloadOrWaiting(20, 10)" }, _fakeChannelMetadataServices.CallsMade);
            AssertConvertedFromTheService(Assert.Single(videosWithChannels));
        }

        [Fact]
        public void GetVideosWithChannelsWhereVideoTitleContainsWordPassesTheWordAndPageOnAndConvertsTheResult()
        {
            var videosWithChannels = _videoBrowsingUseCases.GetVideosWithChannelsWhereVideoTitleContainsWord("keyword", 20, 10);

            Assert.Equal(new[] { "GetVideosWithChannelsWhereVideoTitleContainsWord(keyword, 20, 10)" }, _fakeChannelMetadataServices.CallsMade);
            AssertConvertedFromTheService(Assert.Single(videosWithChannels));
        }

        [Fact]
        public void GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWordsPassesTheWordsAndPageOnAndConvertsTheResult()
        {
            var videosWithChannels = _videoBrowsingUseCases.GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords("channelWord", "titleWord", 20, 10);

            Assert.Equal(new[] { "GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(channelWord, titleWord, 20, 10)" }, _fakeChannelMetadataServices.CallsMade);
            AssertConvertedFromTheService(Assert.Single(videosWithChannels));
        }

        [Fact]
        public void GetVideoWithChannelByUrlPassesTheUrlOnAndConvertsTheResult()
        {
            var videoWithChannel = _videoBrowsingUseCases.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored("https://www.youtube.com/watch?v=v1");

            Assert.Equal(new[] { "GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(https://www.youtube.com/watch?v=v1)" }, _fakeChannelMetadataServices.CallsMade);
            AssertConvertedFromTheService(videoWithChannel);
        }

        [Fact]
        public void UpdateDownloadStatusOfVideosPassesTheVideosToTheServiceAsServiceDtos()
        {
            _videoBrowsingUseCases.UpdateDownloadStatusOfVideos(new[]
            {
                new ApplicationVideoMetadataDto { SiteId = "v1", DownloadStatus = ApplicationVideoDownloadStatus.ToDownload },
                new ApplicationVideoMetadataDto { SiteId = "v2", DownloadStatus = ApplicationVideoDownloadStatus.Ignore },
            });

            Assert.Equal(
                new[] { ("v1", ServiceVideoDownloadStatus.ToDownload), ("v2", ServiceVideoDownloadStatus.Ignore) },
                _fakeMetadataServices.VideosWhoseStatusWasUpdated.Select(video => (video.SiteId, video.DownloadStatus)));
        }

        [Fact]
        public void ClearBeingDownloadedFlagOfVideoReturnsTheVideoTheServiceReturnedAsAnApplicationDto()
        {
            var returnedVideo = _videoBrowsingUseCases.ClearBeingDownloadedFlagOfVideo(new ApplicationVideoMetadataDto { SiteId = "v1", IsBeingDownloaded = true });

            Assert.Equal("v1", Assert.Single(_fakeMetadataServices.VideosWhoseFlagWasCleared).SiteId);
            Assert.Equal("returned by the service", returnedVideo.Title);
            Assert.False(returnedVideo.IsBeingDownloaded);
        }

        [Fact]
        public void MarkVideoAsBeingDownloadedReturnsTheVideoTheServiceReturnedAsAnApplicationDto()
        {
            var returnedVideo = _videoBrowsingUseCases.MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(new ApplicationVideoMetadataDto { SiteId = "v1" });

            Assert.Equal("v1", Assert.Single(_fakeMetadataServices.VideosMarkedAsBeingDownloaded).SiteId);
            Assert.Equal("returned by the service", returnedVideo.Title);
            Assert.True(returnedVideo.IsBeingDownloaded);
        }
    }

    public class LogBrowsingUseCasesTests
    {
        private static readonly DateTime StartOfRange = new DateTime(2026, 9, 1);
        private static readonly DateTime EndOfRange = new DateTime(2026, 10, 1);

        private readonly FakeLogServices _fakeLogServices = new FakeLogServices();
        private readonly LogBrowsingUseCases _logBrowsingUseCases;

        public LogBrowsingUseCasesTests()
        {
            _logBrowsingUseCases = new LogBrowsingUseCases(_fakeLogServices);
        }

        [Fact]
        public void GetWarningsLoggedBetweenPassesTheRangeOnAndConvertsTheResult()
        {
            _fakeLogServices.WarningsToReturn.Add(new ServiceLogEntryDto { Id = "w1", Message = "a warning", LogType = ServiceLogEntryType.Warning });

            var warnings = _logBrowsingUseCases.GetWarningsLoggedBetween(StartOfRange, EndOfRange);

            Assert.Equal((StartOfRange, EndOfRange), Assert.Single(_fakeLogServices.RangesRequested));
            var warning = Assert.Single(warnings);
            Assert.Equal("a warning", warning.Message);
            Assert.Equal(ApplicationLogEntryType.Warning, warning.LogType);
        }

        [Fact]
        public void GetErrorsLoggedBetweenPassesTheRangeOnAndConvertsTheResult()
        {
            _fakeLogServices.ErrorsToReturn.Add(new ServiceLogEntryDto { Id = "e1", Message = "an error", LogType = ServiceLogEntryType.Error });

            var errors = _logBrowsingUseCases.GetErrorsLoggedBetween(StartOfRange, EndOfRange);

            Assert.Equal((StartOfRange, EndOfRange), Assert.Single(_fakeLogServices.RangesRequested));
            var error = Assert.Single(errors);
            Assert.Equal("an error", error.Message);
            Assert.Equal(ApplicationLogEntryType.Error, error.LogType);
        }
    }

    public class VideoBrowsingUseCasesSavingDownloadQualityChoicesTests
    {
        private readonly FakeMetadataServices _fakeMetadataServices = new FakeMetadataServices();
        private readonly VideoBrowsingUseCases _videoBrowsingUseCases;

        public VideoBrowsingUseCasesSavingDownloadQualityChoicesTests()
        {
            _videoBrowsingUseCases = new VideoBrowsingUseCases(new FakeChannelMetadataServices(), _fakeMetadataServices);
        }

        [Fact]
        public void SaveDownloadQualityChoiceOverridesOfVideoPassesTheVideoOnToTheServiceWithItsOverrides()
        {
            var video = new ApplicationVideoMetadataDto
            {
                SiteId = "v1",
                Title = "The video",
                VideoCodecChoiceOverride = ApplicationVideoCodecChoice.AnyCodec,
                MaximumVideoResolutionChoiceOverride = ApplicationMaximumVideoResolutionChoice.UpTo720p,
                AudioQualityChoiceOverride = null,
            };

            _videoBrowsingUseCases.SaveDownloadQualityChoiceOverridesOfVideo(video);

            var videoPassedOn = Assert.Single(_fakeMetadataServices.VideosWhoseDownloadQualityChoiceOverridesWereSaved);
            Assert.Equal("v1", videoPassedOn.SiteId);
            Assert.Equal(ServiceVideoCodecChoice.AnyCodec, videoPassedOn.VideoCodecChoiceOverride);
            Assert.Equal(ServiceMaximumVideoResolutionChoice.UpTo720p, videoPassedOn.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoPassedOn.AudioQualityChoiceOverride);
        }
    }

    public class DownloadQualitySettingsUseCasesTests
    {
        private readonly FakeDownloadQualitySettingsServices _fakeDownloadQualitySettingsServices = new FakeDownloadQualitySettingsServices();
        private readonly DownloadQualitySettingsUseCases _downloadQualitySettingsUseCases;

        public DownloadQualitySettingsUseCasesTests()
        {
            _downloadQualitySettingsUseCases = new DownloadQualitySettingsUseCases(_fakeDownloadQualitySettingsServices);
        }

        [Fact]
        public void GetAppWideDownloadQualityConvertsTheServicesResultToAnApplicationDto()
        {
            _fakeDownloadQualitySettingsServices.AppWideDownloadQualityToReturn = new ServiceDownloadQualityDto
            {
                VideoCodecToPreferWhenDownloading = ServiceVideoCodecChoice.AnyCodec,
                MaximumVideoResolutionOnTheShorterSide = ServiceMaximumVideoResolutionChoice.UpTo1080p,
                AudioQualityToDownload = ServiceAudioQualityChoice.SmallestAac,
            };

            var downloadQuality = _downloadQualitySettingsUseCases.GetAppWideDownloadQuality();

            Assert.Equal(new[] { "GetAppWideDownloadQualityFromConfigFile" }, _fakeDownloadQualitySettingsServices.CallsMade);
            Assert.Equal(ApplicationVideoCodecChoice.AnyCodec, downloadQuality.VideoCodecToPreferWhenDownloading);
            Assert.Equal(ApplicationMaximumVideoResolutionChoice.UpTo1080p, downloadQuality.MaximumVideoResolutionOnTheShorterSide);
            Assert.Equal(ApplicationAudioQualityChoice.SmallestAac, downloadQuality.AudioQualityToDownload);
        }

        [Fact]
        public void SaveAppWideVideoCodecChoicePassesTheConvertedChoiceOnToTheService()
        {
            _downloadQualitySettingsUseCases.SaveAppWideVideoCodecChoice(ApplicationVideoCodecChoice.AnyCodec);

            Assert.Equal(new[] { "SaveAppWideVideoCodecChoice(AnyCodec)" }, _fakeDownloadQualitySettingsServices.CallsMade);
        }

        [Fact]
        public void SaveAppWideMaximumVideoResolutionChoicePassesTheConvertedChoiceOnToTheService()
        {
            _downloadQualitySettingsUseCases.SaveAppWideMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice.UpTo720p);

            Assert.Equal(new[] { "SaveAppWideMaximumVideoResolutionChoice(UpTo720p)" }, _fakeDownloadQualitySettingsServices.CallsMade);
        }

        [Fact]
        public void SaveAppWideAudioQualityChoicePassesTheConvertedChoiceOnToTheService()
        {
            _downloadQualitySettingsUseCases.SaveAppWideAudioQualityChoice(ApplicationAudioQualityChoice.BestOfAnyCodec);

            Assert.Equal(new[] { "SaveAppWideAudioQualityChoice(BestOfAnyCodec)" }, _fakeDownloadQualitySettingsServices.CallsMade);
        }
    }
}
