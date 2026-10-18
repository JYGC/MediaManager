using System;
using System.IO;

namespace OPMF.Downloader.YTDownloader
{
    public record VideoDownloaderExecutableLocation(string ExecutableName, string ExecutableFilePath, string DownloadUrlUsedWhenExecutableIsMissing);

    public interface IMissingVideoDownloaderExecutableDownloader
    {
        void DownloadExecutableWhenItIsMissing(VideoDownloaderExecutableLocation executableLocation);
        bool DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(VideoDownloaderExecutableLocation fallbackExecutableLocation);
    }

    public class MissingVideoDownloaderExecutableDownloader : IMissingVideoDownloaderExecutableDownloader
    {
        private readonly IUrlToFileDownloader _urlToFileDownloader;
        private readonly Logging.Logger _logger;
        private readonly TextWriter _standardOutput;

        public MissingVideoDownloaderExecutableDownloader(IUrlToFileDownloader urlToFileDownloader, Logging.Logger logger, TextWriter standardOutput)
        {
            _urlToFileDownloader = urlToFileDownloader;
            _logger = logger;
            _standardOutput = standardOutput;
        }

        public void DownloadExecutableWhenItIsMissing(VideoDownloaderExecutableLocation executableLocation)
        {
            if (!File.Exists(executableLocation.ExecutableFilePath))
            {
                _standardOutput.WriteLine("downloading " + executableLocation.ExecutableName + " binary");
                _urlToFileDownloader.DownloadFileFromUrlToFilePath(executableLocation.DownloadUrlUsedWhenExecutableIsMissing, executableLocation.ExecutableFilePath);
            }
        }

        public bool DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(VideoDownloaderExecutableLocation fallbackExecutableLocation)
        {
            try
            {
                DownloadExecutableWhenItIsMissing(fallbackExecutableLocation);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogEntry(new Entities.OPMFLog
                {
                    Message = "Failed to download " + fallbackExecutableLocation.ExecutableName + ", so videos that fail will not be retried with it in this run: " + e.Message,
                    Type = Entities.OPMFLogType.Warning,
                    ExceptionObject = e.ToString(),
                    Variables = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "executableName", fallbackExecutableLocation.ExecutableName },
                        { "downloadUrl", fallbackExecutableLocation.DownloadUrlUsedWhenExecutableIsMissing },
                    }
                });
                return false;
            }
        }
    }
}
