using System;
using System.Linq;
using MediaManager.Initializations;
using Xunit;

namespace OPMF.ContractTests
{
    public class ApplicationInterfaceContractTests
    {
        private static string[] InOrdinalOrder(params string[] signatures)
        {
            return signatures.OrderBy(signature => signature, StringComparer.Ordinal).ToArray();
        }

        [Fact]
        public void IApplicationStartupUseCase_Implementation_IsApplicationStartupUseCase()
        {
            Assert.Contains(typeof(IApplicationStartupUseCase), typeof(ApplicationStartupUseCase).GetInterfaces());
        }

        [Fact]
        public void IApplicationStartupUseCase_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig()"),
                InterfaceShape.MethodSignaturesOf(typeof(IApplicationStartupUseCase)));
        }

        [Fact]
        public void IUnhandledErrorRecordingUseCase_Implementation_IsUnhandledErrorRecordingUseCase()
        {
            Assert.Contains(typeof(IUnhandledErrorRecordingUseCase), typeof(UnhandledErrorRecordingUseCase).GetInterfaces());
        }

        [Fact]
        public void IUnhandledErrorRecordingUseCase_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void RecordUnhandledError(Exception)"),
                InterfaceShape.MethodSignaturesOf(typeof(IUnhandledErrorRecordingUseCase)));
        }

        [Fact]
        public void IApplicationEntryPointUseCase_Implementation_IsApplicationEntryPointUseCase()
        {
            Assert.Contains(typeof(IApplicationEntryPointUseCase), typeof(ApplicationEntryPointUseCase).GetInterfaces());
        }

        [Fact]
        public void IApplicationEntryPointUseCase_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder("Void RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(Action)"),
                InterfaceShape.MethodSignaturesOf(typeof(IApplicationEntryPointUseCase)));
        }

        [Fact]
        public void ICommandLineTaskUseCases_Implementation_IsCommandLineTaskUseCases()
        {
            Assert.Contains(typeof(ICommandLineTaskUseCases), typeof(CommandLineTaskUseCases).GetInterfaces());
        }

        [Fact]
        public void ICommandLineTaskUseCases_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "Void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted()",
                    "Void DownloadEveryVideoQueuedToDownload()",
                    "Void DownloadFirstVideoWhoseSiteIdContains(String)"),
                InterfaceShape.MethodSignaturesOf(typeof(ICommandLineTaskUseCases)));
        }

        [Fact]
        public void IChannelBrowsingUseCases_Implementation_IsChannelBrowsingUseCases()
        {
            Assert.Contains(typeof(IChannelBrowsingUseCases), typeof(ChannelBrowsingUseCases).GetInterfaces());
        }

        [Fact]
        public void IChannelBrowsingUseCases_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ApplicationChannelDto> GetAllChannels()",
                    "Void UpdateBlacklistFlagOfChannels(IEnumerable<ApplicationChannelDto>)"),
                InterfaceShape.MethodSignaturesOf(typeof(IChannelBrowsingUseCases)));
        }

        [Fact]
        public void IVideoBrowsingUseCases_Implementation_IsVideoBrowsingUseCases()
        {
            Assert.Contains(typeof(IVideoBrowsingUseCases), typeof(VideoBrowsingUseCases).GetInterfaces());
        }

        [Fact]
        public void IVideoBrowsingUseCases_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ApplicationVideoWithChannelDto> GetNewVideosWithChannels(Int32, Int32)",
                    "List<ApplicationVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(Int32, Int32)",
                    "List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(String, Int32, Int32)",
                    "List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(String, String, Int32, Int32)",
                    "ApplicationVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(String)",
                    "Void UpdateDownloadStatusOfVideos(IEnumerable<ApplicationVideoMetadataDto>)",
                    "ApplicationVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ApplicationVideoMetadataDto)",
                    "ApplicationVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ApplicationVideoMetadataDto)",
                    "Void SaveDownloadQualityChoiceOverridesOfVideo(ApplicationVideoMetadataDto)"),
                InterfaceShape.MethodSignaturesOf(typeof(IVideoBrowsingUseCases)));
        }

        [Fact]
        public void IDownloadQualitySettingsUseCases_Implementation_IsDownloadQualitySettingsUseCases()
        {
            Assert.Contains(typeof(IDownloadQualitySettingsUseCases), typeof(DownloadQualitySettingsUseCases).GetInterfaces());
        }

        [Fact]
        public void IDownloadQualitySettingsUseCases_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "ApplicationDownloadQualityDto GetAppWideDownloadQuality()",
                    "Void SaveAppWideVideoCodecChoice(ApplicationVideoCodecChoice)",
                    "Void SaveAppWideMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice)",
                    "Void SaveAppWideAudioQualityChoice(ApplicationAudioQualityChoice)"),
                InterfaceShape.MethodSignaturesOf(typeof(IDownloadQualitySettingsUseCases)));
        }

        [Fact]
        public void ILogBrowsingUseCases_Implementation_IsLogBrowsingUseCases()
        {
            Assert.Contains(typeof(ILogBrowsingUseCases), typeof(LogBrowsingUseCases).GetInterfaces());
        }

        [Fact]
        public void ILogBrowsingUseCases_HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                InOrdinalOrder(
                    "List<ApplicationLogEntryDto> GetWarningsLoggedBetween(DateTime, DateTime)",
                    "List<ApplicationLogEntryDto> GetErrorsLoggedBetween(DateTime, DateTime)"),
                InterfaceShape.MethodSignaturesOf(typeof(ILogBrowsingUseCases)));
        }
    }
}
