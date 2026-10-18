using System;
using System.Collections.Generic;
using MediaManager.Initializations;
using OffPeakMediaFetcher;
using Xunit;

namespace OPMF.UnitTests.OffPeakMediaFetcher
{
    public class CommandLineArgumentsDispatcherTests
    {
        private class RecordingCommandLineTaskUseCases : ICommandLineTaskUseCases
        {
            public List<string> Calls { get; } = new List<string>();

            public void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted() => Calls.Add("metadata");

            public void DownloadEveryVideoQueuedToDownload() => Calls.Add("videos");

            public void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt) => Calls.Add("videos " + siteIdOrPartOfIt);
        }

        private readonly RecordingCommandLineTaskUseCases _recordingCommandLineTaskUseCases = new RecordingCommandLineTaskUseCases();
        private readonly CommandLineArgumentsDispatcher _commandLineArgumentsDispatcher;

        public CommandLineArgumentsDispatcherTests()
        {
            _commandLineArgumentsDispatcher = new CommandLineArgumentsDispatcher(_recordingCommandLineTaskUseCases);
        }

        [Fact]
        public void MetadataFetchesAndStoresTheNewVideoMetadata()
        {
            _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(new[] { "metadata" });

            Assert.Equal(new[] { "metadata" }, _recordingCommandLineTaskUseCases.Calls);
        }

        [Fact]
        public void VideosDownloadsEveryQueuedVideo()
        {
            _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(new[] { "videos" });

            Assert.Equal(new[] { "videos" }, _recordingCommandLineTaskUseCases.Calls);
        }

        [Fact]
        public void VideosWithASiteIdDownloadsTheFirstVideoWhoseSiteIdContainsIt()
        {
            _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(new[] { "videos", "abc123" });

            Assert.Equal(new[] { "videos abc123" }, _recordingCommandLineTaskUseCases.Calls);
        }

        [Fact]
        public void VideosWithMoreThanOneSiteIdDownloadsTheWholeQueueAsToday()
        {
            _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(new[] { "videos", "a", "b" });

            Assert.Equal(new[] { "videos" }, _recordingCommandLineTaskUseCases.Calls);
        }

        [Fact]
        public void AnUnknownArgumentThrowsInvalidArgument()
        {
            var exception = Assert.Throws<Exception>(() => _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(new[] { "not-a-real-command" }));

            Assert.Equal("Invalid argument.", exception.Message);
            Assert.Empty(_recordingCommandLineTaskUseCases.Calls);
        }

        [Fact]
        public void NoArgumentThrowsAppropriateArgumentsRequired()
        {
            var exception = Assert.Throws<Exception>(() => _commandLineArgumentsDispatcher.DispatchCommandLineArgumentsToUseCase(Array.Empty<string>()));

            Assert.Equal("Appropriate arguments required.", exception.Message);
            Assert.Empty(_recordingCommandLineTaskUseCases.Calls);
        }
    }
}
