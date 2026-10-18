using System.Linq;
using MediaManager.Repositories;
using OPMF.Database;
using OPMF.Downloader.YTDownloader;
using OPMF.Filesystem;
using OPMF.SiteAdapter.Youtube;
using Xunit;

namespace OPMF.ContractTests
{
    public class IMissingFolderCreatorContractTests
    {
        [Fact]
        public void Implementation_IsMissingFolderCreator()
        {
            Assert.Contains(typeof(IMissingFolderCreator), typeof(MissingFolderCreator).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[]
                {
                    "Void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(IReadonlySettings)",
                    "Void CreateFolderWhenMissing(String)",
                },
                InterfaceShape.MethodSignaturesOf(typeof(IMissingFolderCreator)));
        }
    }

    public class IDatabaseConnectionOpenerContractTests
    {
        [Fact]
        public void Implementation_IsSharedModeDatabaseFileConnectionOpener()
        {
            Assert.Contains(typeof(IDatabaseConnectionOpener), typeof(SharedModeDatabaseFileConnectionOpener).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void RunWithDatabaseConnectionOpenForThisCallOnly(Action<DatabaseAdapter>)" },
                InterfaceShape.MethodSignaturesOf(typeof(IDatabaseConnectionOpener)));
        }
    }

    public class IYoutubeDataApiContractTests
    {
        [Fact]
        public void Implementation_IsYoutubeDataApi()
        {
            Assert.Contains(typeof(IYoutubeDataApi), typeof(YoutubeDataApi).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[]
                {
                    "ActivityListResponse ListOnePageOfActivitiesOfChannelPublishedAfter(String, DateTime, String)",
                    "ChannelListResponse ListChannelWithSiteId(String)",
                    "VideoListResponse ListVideoWithSiteId(String)",
                },
                InterfaceShape.MethodSignaturesOf(typeof(IYoutubeDataApi)));
        }
    }

    public class IVideoDownloaderExecutableRunnerContractTests
    {
        [Fact]
        public void Implementation_IsNYoutubeDLVideoDownloaderExecutableRunner()
        {
            Assert.Contains(typeof(IVideoDownloaderExecutableRunner), typeof(NYoutubeDLVideoDownloaderExecutableRunner).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "String RunExecutableToDownloadVideoAndReturnLastErrorOutputLine(String, Boolean, String, String, Action<String>)" },
                InterfaceShape.MethodSignaturesOf(typeof(IVideoDownloaderExecutableRunner)));
        }
    }

    public class IUrlToFileDownloaderContractTests
    {
        [Fact]
        public void Implementation_IsWebClientUrlToFileDownloader()
        {
            Assert.Contains(typeof(IUrlToFileDownloader), typeof(WebClientUrlToFileDownloader).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void DownloadFileFromUrlToFilePath(String, String)" },
                InterfaceShape.MethodSignaturesOf(typeof(IUrlToFileDownloader)));
        }
    }

    public class IMissingVideoDownloaderExecutableDownloaderContractTests
    {
        [Fact]
        public void Implementation_IsMissingVideoDownloaderExecutableDownloader()
        {
            Assert.Contains(typeof(IMissingVideoDownloaderExecutableDownloader), typeof(MissingVideoDownloaderExecutableDownloader).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[]
                {
                    "Boolean DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(VideoDownloaderExecutableLocation)",
                    "Void DownloadExecutableWhenItIsMissing(VideoDownloaderExecutableLocation)",
                },
                InterfaceShape.MethodSignaturesOf(typeof(IMissingVideoDownloaderExecutableDownloader)));
        }

        [Fact]
        public void VideoDownloaderExecutableLocation_HasTheNamePathAndUrlProperties()
        {
            Assert.Equal(
                new[] { "String DownloadUrlUsedWhenExecutableIsMissing", "String ExecutableFilePath", "String ExecutableName" },
                typeof(VideoDownloaderExecutableLocation).GetProperties()
                    .Where(property => property.Name != "EqualityContract")
                    .Select(property => property.PropertyType.Name + " " + property.Name)
                    .OrderBy(declaration => declaration, System.StringComparer.Ordinal));
        }
    }

    public class IInterruptibleDownloadThreadStarterContractTests
    {
        [Fact]
        public void Implementation_IsInterruptibleDownloadThreadStarter()
        {
            Assert.Contains(typeof(IInterruptibleDownloadThreadStarter), typeof(InterruptibleDownloadThreadStarter).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Action StartThreadThatRunsAndReturnActionThatInterruptsIt(Action)" },
                InterfaceShape.MethodSignaturesOf(typeof(IInterruptibleDownloadThreadStarter)));
        }
    }

    public class ICurrentThreadWaiterContractTests
    {
        [Fact]
        public void Implementation_IsThreadSleepCurrentThreadWaiter()
        {
            Assert.Contains(typeof(ICurrentThreadWaiter), typeof(ThreadSleepCurrentThreadWaiter).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void WaitOnCurrentThreadFor(TimeSpan)" },
                InterfaceShape.MethodSignaturesOf(typeof(ICurrentThreadWaiter)));
        }
    }

    public class IConsoleRowOverwriterContractTests
    {
        [Fact]
        public void Implementation_IsConsoleRowOverwriter()
        {
            Assert.Contains(typeof(IConsoleRowOverwriter), typeof(ConsoleRowOverwriter).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void OverwriteConsoleRowWithMessage(Int32, String)" },
                InterfaceShape.MethodSignaturesOf(typeof(IConsoleRowOverwriter)));
        }
    }

    public class IExternalProcessStarterContractTests
    {
        [Fact]
        public void Implementation_IsExternalProcessStarter()
        {
            Assert.Contains(typeof(IExternalProcessStarter), typeof(ExternalProcessStarter).GetInterfaces());
        }

        [Fact]
        public void HasExactlyTheExpectedMethods()
        {
            Assert.Equal(
                new[] { "Void StartProcessWithExecutableFileNameAndArguments(String, String)" },
                InterfaceShape.MethodSignaturesOf(typeof(IExternalProcessStarter)));
        }
    }
}
