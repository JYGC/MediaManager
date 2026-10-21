using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OPMF.Downloader.YTDownloader
{
    public class YTDownloader : IDownloader
    {
        private readonly Settings.IReadonlySettings __readonlySettings;
        private readonly Settings.AppConfig __appConfig;
        private readonly IVideoDownloaderExecutableRunner __videoDownloaderExecutableRunner;
        private readonly IInterruptibleDownloadThreadStarter __interruptibleDownloadThreadStarter;
        private readonly ICurrentThreadWaiter __currentThreadWaiter;
        private readonly IConsoleRowOverwriter __consoleRowOverwriter;
        private readonly TimeProvider __timeProvider;
        private readonly Logging.Logger __logger;
        private readonly VideoDownloaderExecutableLocation __executableTriedFirstLocation;
        private readonly VideoDownloaderExecutableLocation __fallbackExecutableLocationOrNull;
        private readonly string __mergeContainer;

        private List<DownloadInstance> __downloadInstances;
        public List<Entities.Metadata> DownloadQueue { get; set; }

        public YTDownloader(
            Settings.IReadonlySettings readonlySettings,
            Settings.AppConfig appConfig,
            IVideoDownloaderExecutableRunner videoDownloaderExecutableRunner,
            IMissingVideoDownloaderExecutableDownloader missingVideoDownloaderExecutableDownloader,
            IFfmpegOnPathFinder ffmpegOnPathFinder,
            IInterruptibleDownloadThreadStarter interruptibleDownloadThreadStarter,
            ICurrentThreadWaiter currentThreadWaiter,
            IConsoleRowOverwriter consoleRowOverwriter,
            TimeProvider timeProvider,
            Logging.Logger logger)
        {
            __readonlySettings = readonlySettings;
            __appConfig = appConfig;
            __videoDownloaderExecutableRunner = videoDownloaderExecutableRunner;
            __interruptibleDownloadThreadStarter = interruptibleDownloadThreadStarter;
            __currentThreadWaiter = currentThreadWaiter;
            __consoleRowOverwriter = consoleRowOverwriter;
            __timeProvider = timeProvider;
            __logger = logger;

            __mergeContainer = MergeContainerRules.GetMergeContainerOfVideoExtensionOrThrow(appConfig.YoutubeDL.VideoExtension);
            if (!ffmpegOnPathFinder.IsFfmpegFoundInAFolderOnPath())
            {
                throw new InvalidOperationException(
                    "ffmpeg.exe was not found in any folder on PATH. It is needed to merge the video and audio of each download into one file. Install ffmpeg and add its folder to PATH, then run again.");
            }

            Settings.VideoDownloaderExecutable executableTriedFirst = appConfig.YoutubeDL.ExecutableTriedFirstToDownloadVideos;
            Settings.VideoDownloaderExecutable fallbackExecutable = VideoDownloaderExecutableOrderAndLocationRules.GetFallbackExecutable(executableTriedFirst);
            __executableTriedFirstLocation = VideoDownloaderExecutableOrderAndLocationRules.GetLocationOfExecutable(executableTriedFirst, readonlySettings);
            VideoDownloaderExecutableLocation fallbackExecutableLocation = VideoDownloaderExecutableOrderAndLocationRules.GetLocationOfExecutable(fallbackExecutable, readonlySettings);

            missingVideoDownloaderExecutableDownloader.DownloadExecutableWhenItIsMissing(__executableTriedFirstLocation);
            bool fallbackExecutableIsAvailable = missingVideoDownloaderExecutableDownloader.DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(fallbackExecutableLocation);
            __fallbackExecutableLocationOrNull = fallbackExecutableIsAvailable ? fallbackExecutableLocation : null;
        }

        public void StartDownloadingQueue()
        {
            __downloadInstances = new List<DownloadInstance>();
            int noOfInstances = (
                DownloadQueue.Count < __appConfig.YoutubeDL.MaxNoOfParallelDownloads
            ) ? DownloadQueue.Count : __appConfig.YoutubeDL.MaxNoOfParallelDownloads;
            for (int i = 0; i < noOfInstances; i++)
            {
                __downloadInstances.Add(new DownloadInstance(
                    i,
                    __executableTriedFirstLocation,
                    __fallbackExecutableLocationOrNull,
                    __videoDownloaderExecutableRunner,
                    __appConfig.YoutubeDL,
                    __mergeContainer,
                    __readonlySettings.GetDownloadFolderPath(),
                    __interruptibleDownloadThreadStarter,
                    __consoleRowOverwriter,
                    __timeProvider,
                    __logger));
            }

            int currentMetadataIndex = 0;
            while (currentMetadataIndex < DownloadQueue.Count || __downloadInstances.Count > 0)
            {
                for (int downloadInstanceIndex = __downloadInstances.Count - 1; downloadInstanceIndex >= 0; downloadInstanceIndex--)
                {
                    DownloadInstance downloadInstance = __downloadInstances[downloadInstanceIndex];
                    bool downloadInstanceIsFreeOrHasRunTooLong = downloadInstance.NotDownloading ||
                        __timeProvider.GetLocalNow().LocalDateTime.Subtract(downloadInstance.StartDateTime).TotalSeconds > __appConfig.StopDownloadInstanceAfterSeconds;
                    if (downloadInstanceIsFreeOrHasRunTooLong)
                    {
                        downloadInstance.StopDownloadThread();
                        if (currentMetadataIndex < DownloadQueue.Count)
                        {
                            downloadInstance.Download(DownloadQueue[currentMetadataIndex]);
                            currentMetadataIndex++;
                        }
                        else
                        {
                            downloadInstance.CleanUpInstance();
                            __downloadInstances.RemoveAt(downloadInstanceIndex);
                        }
                    }
                }
                __currentThreadWaiter.WaitOnCurrentThreadFor(TimeSpan.FromSeconds(5));
            }
        }
    }

    public class DownloadInstance
    {
        private const int TITLE_DISPLAY_LENGTH = 40;

        private readonly VideoDownloaderExecutableLocation __executableTriedFirst;
        private readonly VideoDownloaderExecutableLocation __fallbackExecutableOrNull;
        private readonly IVideoDownloaderExecutableRunner __videoDownloaderExecutableRunner;
        private readonly Settings.YoutubeDLConfig __youtubeDLConfig;
        private readonly string __mergeContainer;
        private readonly string __downloadFolderPath;
        private readonly IInterruptibleDownloadThreadStarter __interruptibleDownloadThreadStarter;
        private readonly IConsoleRowOverwriter __consoleRowOverwriter;
        private readonly TimeProvider __timeProvider;
        private readonly Logging.Logger __logger;

        private Action __interruptDownloadThread;
        private CancellationTokenSource __stopCurrentDownload;
        private string __title;

        public DateTime StartDateTime { get; }
        public bool NotDownloading { get; set; } = true;
        public int ScreenPosition { get; }

        public DownloadInstance(
            int screenPosition,
            VideoDownloaderExecutableLocation executableTriedFirst,
            VideoDownloaderExecutableLocation fallbackExecutableOrNull,
            IVideoDownloaderExecutableRunner videoDownloaderExecutableRunner,
            Settings.YoutubeDLConfig youtubeDLConfig,
            string mergeContainer,
            string downloadFolderPath,
            IInterruptibleDownloadThreadStarter interruptibleDownloadThreadStarter,
            IConsoleRowOverwriter consoleRowOverwriter,
            TimeProvider timeProvider,
            Logging.Logger logger)
        {
            ScreenPosition = screenPosition;
            __executableTriedFirst = executableTriedFirst;
            __fallbackExecutableOrNull = fallbackExecutableOrNull;
            __videoDownloaderExecutableRunner = videoDownloaderExecutableRunner;
            __youtubeDLConfig = youtubeDLConfig;
            __mergeContainer = mergeContainer;
            __downloadFolderPath = downloadFolderPath;
            __interruptibleDownloadThreadStarter = interruptibleDownloadThreadStarter;
            __consoleRowOverwriter = consoleRowOverwriter;
            __timeProvider = timeProvider;
            __logger = logger;
            StartDateTime = Now();
        }

        public void CleanUpInstance()
        {
            __consoleRowOverwriter.OverwriteConsoleRowWithMessage(ScreenPosition, "download instance not needed. removed...");
        }

        public void Download(Entities.Metadata metadata)
        {
            NotDownloading = false;
            string outputFilePath = Path.Join(__downloadFolderPath, VideoFileNameRules.CreateFileNameWithoutExtensionFromTitle(metadata.Title) + "." + __youtubeDLConfig.VideoExtension);
            string formatSelector = VideoFormatSelectorRules.BuildFormatSelectorThatBothExecutablesUnderstand(
                DownloadQualityChoiceRules.ChooseDownloadQualityOfVideo(metadata, __youtubeDLConfig));
            CancellationTokenSource stopThisDownload = new CancellationTokenSource();
            __stopCurrentDownload = stopThisDownload;
            __interruptDownloadThread = __interruptibleDownloadThreadStarter.StartThreadThatRunsAndReturnActionThatInterruptsIt(() =>
            {
                __title = metadata.Title;
                VideoDownloadAttemptOutcome outcome = VideoDownloadAttemptRules.RunExecutableTriedFirstThenFallbackExecutableWhenFirstAttemptFailed(
                    () => __AttemptToDownload(metadata, __executableTriedFirst, false, outputFilePath, formatSelector, stopThisDownload.Token),
                    __fallbackExecutableOrNull == null ? null : () =>
                    {
                        __consoleRowOverwriter.OverwriteConsoleRowWithMessage(
                            ScreenPosition, __executableTriedFirst.ExecutableName + " failed, trying " + __fallbackExecutableOrNull.ExecutableName);
                        return __AttemptToDownload(metadata, __fallbackExecutableOrNull, true, outputFilePath, formatSelector, stopThisDownload.Token);
                    });
                if (outcome == VideoDownloadAttemptOutcome.Succeeded)
                {
                    metadata.Status = Entities.MetadataStatus.Downloaded;
                }
                NotDownloading = true;
            });
        }

        public void StopDownloadThread()
        {
            if (__stopCurrentDownload != null) __stopCurrentDownload.Cancel();
            if (__interruptDownloadThread != null) __interruptDownloadThread();
        }

        private VideoDownloadAttemptOutcome __AttemptToDownload(
            Entities.Metadata metadata,
            VideoDownloaderExecutableLocation executable,
            bool tellExecutableNotToResumePartialFiles,
            string outputFilePath,
            string formatSelector,
            CancellationToken downloadWasStopped)
        {
            try
            {
                VideoDownloaderExecutableRunOutcome runOutcome = __videoDownloaderExecutableRunner.RunExecutableToDownloadVideoAndReportWhetherTheOutputFileExists(
                    executable.ExecutableFilePath,
                    tellExecutableNotToResumePartialFiles,
                    metadata.Url,
                    outputFilePath,
                    formatSelector,
                    __mergeContainer,
                    output => __consoleRowOverwriter.OverwriteConsoleRowWithMessage(
                        ScreenPosition,
                        output.StartsWith("[download]") ? string.Format("{0} Video: {1}...", output, __TruncateStrIfTooLong(__title)) : output));
                if (downloadWasStopped.IsCancellationRequested)
                {
                    return VideoDownloadAttemptOutcome.StoppedBecauseDownloadInstanceTimedOut;
                }
                if (runOutcome.OutputFileExistsAfterTheRun)
                {
                    return VideoDownloadAttemptOutcome.Succeeded;
                }
                __logger.LogEntry(new Entities.OPMFLog
                {
                    Message = VideoDownloadAttemptRules.CreateMessageOfAttemptThatCreatedNoVideoFile(runOutcome.LastErrorOutputLineOrNull, executable.ExecutableName, outputFilePath),
                    Type = Entities.OPMFLogType.Warning,
                    DateCreated = Now(),
                    Variables = new Dictionary<string, object>
                    {
                        { "metadata.Title", __title },
                        { "metadata.Url", metadata.Url },
                        { "executableName", executable.ExecutableName }
                    }
                });
                __consoleRowOverwriter.OverwriteConsoleRowWithMessage(ScreenPosition, "An error has occured. See error log...");
                return VideoDownloadAttemptOutcome.Failed;
            }
            catch (Exception ex)
            {
                __logger.LogEntry(new Entities.OPMFError(ex)
                {
                    DateCreated = Now(),
                    Variables = new Dictionary<string, object>
                    {
                        { "metadata.Title", __title },
                        { "metadata.Url", metadata.Url },
                        { "executableName", executable.ExecutableName }
                    }
                });
                return downloadWasStopped.IsCancellationRequested
                    ? VideoDownloadAttemptOutcome.StoppedBecauseDownloadInstanceTimedOut
                    : VideoDownloadAttemptOutcome.Failed;
            }
        }

        private DateTime Now()
        {
            return __timeProvider.GetLocalNow().LocalDateTime;
        }

        private string __TruncateStrIfTooLong(string str)
        {
            return str.Length <= TITLE_DISPLAY_LENGTH ? str : str.Substring(0, TITLE_DISPLAY_LENGTH);
        }
    }
}
