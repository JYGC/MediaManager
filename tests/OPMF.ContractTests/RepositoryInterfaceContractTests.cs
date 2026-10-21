using System;
using System.Linq;
using MediaManager.Repositories;
using Xunit;

namespace OPMF.ContractTests
{
    public class RepositoryInterfaceContractTests
    {
        private static string[] InOrdinalOrder(params string[] signatures)
        {
            return signatures.OrderBy(signature => signature, StringComparer.Ordinal).ToArray();
        }

        [Fact]
        public void IChannelRepository_Implementation_IsChannelRepository()
        {
            Assert.Contains(typeof(IChannelRepository), typeof(ChannelRepository).GetInterfaces());
        }

        [Fact]
        public void IChannelRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<RepositoryChannelDto> GetAllChannels()",
                    "List<RepositoryChannelDto> GetChannelsThatAreNotBlacklisted()",
                    "List<RepositoryChannelDto> GetChannelsWhoseNameContains(String)",
                    "List<RepositoryChannelDto> GetChannelsWithSiteIds(IEnumerable<String>)",
                    "RepositoryChannelDto GetChannelWithSiteId(String)",
                    "Void InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels(IEnumerable<RepositoryChannelDto>)",
                    "Void UpdateLastCheckedForNewVideosAtAndLatestVideoPublishedAtOfChannels(IEnumerable<RepositoryChannelDto>)",
                    "Void UpdateBlacklistFlagOfChannels(IEnumerable<RepositoryChannelDto>)"),
                InterfaceShape.MethodSignaturesOf(typeof(IChannelRepository)));
        }

        [Fact]
        public void IVideoMetadataRepository_Implementation_IsVideoMetadataRepository()
        {
            Assert.Contains(typeof(IVideoMetadataRepository), typeof(VideoMetadataRepository).GetInterfaces());
        }

        [Fact]
        public void IVideoMetadataRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<RepositoryVideoMetadataDto> GetVideosQueuedToDownload()",
                    "RepositoryVideoMetadataDto GetFirstVideoWhoseSiteIdContains(String)",
                    "List<RepositoryVideoMetadataDto> GetNewVideos(Int32, Int32)",
                    "List<RepositoryVideoMetadataDto> GetVideosQueuedToDownloadOrWaiting(Int32, Int32)",
                    "List<RepositoryVideoMetadataDto> GetVideosWhoseTitleContains(String, Int32, Int32)",
                    "List<RepositoryVideoMetadataDto> GetVideosOfChannelsWhoseTitleContains(IEnumerable<String>, String, Int32, Int32)",
                    "Void InsertVideosThatAreNotStoredYet(IEnumerable<RepositoryVideoMetadataDto>)",
                    "Void UpdateDownloadStatusOfVideos(IEnumerable<RepositoryVideoMetadataDto>)",
                    "Void UpdateDownloadQualityChoiceOverridesOfVideo(RepositoryVideoMetadataDto)",
                    "Void SetBeingDownloadedFlagOfVideos(IEnumerable<RepositoryVideoMetadataDto>, Boolean)"),
                InterfaceShape.MethodSignaturesOf(typeof(IVideoMetadataRepository)));
        }

        [Fact]
        public void ILogRepository_Implementation_IsLogRepository()
        {
            Assert.Contains(typeof(ILogRepository), typeof(LogRepository).GetInterfaces());
        }

        [Fact]
        public void ILogRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<RepositoryLogEntryDto> GetWarningsLoggedBetween(DateTime, DateTime)",
                    "List<RepositoryLogEntryDto> GetErrorsLoggedBetween(DateTime, DateTime)",
                    "Void RecordErrorLogEntry(RepositoryErrorLogEntryToRecordDto)"),
                InterfaceShape.MethodSignaturesOf(typeof(ILogRepository)));
        }

        [Fact]
        public void IVideoSiteRepository_Implementation_IsVideoSiteRepository()
        {
            Assert.Contains(typeof(IVideoSiteRepository), typeof(VideoSiteRepository).GetInterfaces());
        }

        [Fact]
        public void IVideoSiteRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "RepositoryNewVideoMetadataFetchResultDto FetchNewVideoMetadataForChannels(List<RepositoryChannelDto>)",
                    "String GetVideoSiteIdFromVideoUrl(String)",
                    "RepositoryVideoWithChannelDto FetchVideoWithItsChannelFromVideoSite(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(IVideoSiteRepository)));
        }

        [Fact]
        public void IVideoDownloaderRepository_Implementation_IsVideoDownloaderRepository()
        {
            Assert.Contains(typeof(IVideoDownloaderRepository), typeof(VideoDownloaderRepository).GetInterfaces());
        }

        [Fact]
        public void IVideoDownloaderRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "Void PrepareDownloaderBeforeAnyVideoIsMarkedAsBeingDownloaded()",
                    "List<RepositoryVideoMetadataDto> DownloadVideosAndReturnThemWithTheirResultingDownloadStatus(List<RepositoryVideoMetadataDto>)"),
                InterfaceShape.MethodSignaturesOf(typeof(IVideoDownloaderRepository)));
        }

        [Fact]
        public void ITaskRunnerProcessRepository_Implementation_IsTaskRunnerProcessRepository()
        {
            Assert.Contains(typeof(ITaskRunnerProcessRepository), typeof(TaskRunnerProcessRepository).GetInterfaces());
        }

        [Fact]
        public void ITaskRunnerProcessRepository_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(ITaskRunnerProcessRepository)));
        }
    }
}
