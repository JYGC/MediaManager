using System;
using System.Linq;
using MediaManager.Services2;
using Xunit;

namespace OPMF.ContractTests
{
    public class ServiceContractTests
    {
        private static string[] InOrdinalOrder(params string[] signatures)
        {
            return signatures.OrderBy(signature => signature, StringComparer.Ordinal).ToArray();
        }

        [Fact]
        public void IChannelServices_Implementation_IsChannelServices()
        {
            Assert.Contains(typeof(IChannelServices), typeof(ChannelServices).GetInterfaces());
        }

        [Fact]
        public void IChannelServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ServiceChannelDto> GetAllChannels()",
                    "Void UpdateBlacklistFlagOfChannels(IEnumerable<ServiceChannelDto>)"),
                InterfaceShape.MethodSignaturesOf(typeof(IChannelServices)));
        }

        [Fact]
        public void IChannelMetadataServices_Implementation_IsChannelMetadataServices()
        {
            Assert.Contains(typeof(IChannelMetadataServices), typeof(ChannelMetadataServices).GetInterfaces());
        }

        [Fact]
        public void IChannelMetadataServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(String, String, Int32, Int32)",
                    "List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(String, Int32, Int32)",
                    "List<ServiceVideoWithChannelDto> GetNewVideosWithChannels(Int32, Int32)",
                    "List<ServiceVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(Int32, Int32)",
                    "ServiceVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(IChannelMetadataServices)));
        }

        [Fact]
        public void IMetadataServices_Implementation_IsMetadataServices()
        {
            Assert.Contains(typeof(IMetadataServices), typeof(MetadataServices).GetInterfaces());
        }

        [Fact]
        public void IMetadataServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "Void UpdateDownloadStatusOfVideos(IEnumerable<ServiceVideoMetadataDto>)",
                    "ServiceVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ServiceVideoMetadataDto)",
                    "ServiceVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceVideoMetadataDto)"),
                InterfaceShape.MethodSignaturesOf(typeof(IMetadataServices)));
        }

        [Fact]
        public void ITaskRunnerServices_Implementation_IsTaskRunnerServices()
        {
            Assert.Contains(typeof(ITaskRunnerServices), typeof(TaskRunnerServices).GetInterfaces());
        }

        [Fact]
        public void ITaskRunnerServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(ITaskRunnerServices)));
        }

        [Fact]
        public void ILogServices_Implementation_IsLogServices()
        {
            Assert.Contains(typeof(ILogServices), typeof(LogServices).GetInterfaces());
        }

        [Fact]
        public void ILogServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ServiceLogEntryDto> GetWarningsLoggedBetween(DateTime, DateTime)",
                    "List<ServiceLogEntryDto> GetErrorsLoggedBetween(DateTime, DateTime)",
                    "Void RecordError(String, String)"),
                InterfaceShape.MethodSignaturesOf(typeof(ILogServices)));
        }

        [Fact]
        public void INewVideoMetadataFetchServices_Implementation_IsNewVideoMetadataFetchServices()
        {
            Assert.Contains(typeof(INewVideoMetadataFetchServices), typeof(NewVideoMetadataFetchServices).GetInterfaces());
        }

        [Fact]
        public void INewVideoMetadataFetchServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted()" },
                InterfaceShape.MethodSignaturesOf(typeof(INewVideoMetadataFetchServices)));
        }

        [Fact]
        public void IVideoDownloadServices_Implementation_IsVideoDownloadServices()
        {
            Assert.Contains(typeof(IVideoDownloadServices), typeof(VideoDownloadServices).GetInterfaces());
        }

        [Fact]
        public void IVideoDownloadServices_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void DownloadEveryVideoQueuedToDownload()", "Void DownloadFirstVideoWhoseSiteIdContains(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(IVideoDownloadServices)));
        }

        [Fact]
        public void IDownloadedVideoFileMover_Implementation_IsDownloadedVideoFileMover()
        {
            Assert.Contains(typeof(IDownloadedVideoFileMover), typeof(DownloadedVideoFileMover).GetInterfaces());
        }

        [Fact]
        public void IDownloadedVideoFileMover_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(String, String, IEnumerable<String>)" },
                InterfaceShape.MethodSignaturesOf(typeof(IDownloadedVideoFileMover)));
        }
    }
}
