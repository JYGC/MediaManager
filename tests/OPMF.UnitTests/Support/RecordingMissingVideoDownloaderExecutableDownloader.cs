using System;
using System.Collections.Generic;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public class RecordingMissingVideoDownloaderExecutableDownloader : IMissingVideoDownloaderExecutableDownloader
    {
        public List<VideoDownloaderExecutableLocation> LocationsAskedToDownloadWhenMissing { get; } = new List<VideoDownloaderExecutableLocation>();
        public List<VideoDownloaderExecutableLocation> FallbackLocationsAskedToDownloadWhenMissing { get; } = new List<VideoDownloaderExecutableLocation>();
        public Exception ExceptionToThrowWhenAskedToDownload { get; set; }
        public bool FallbackIsAvailable { get; set; } = true;

        public void DownloadExecutableWhenItIsMissing(VideoDownloaderExecutableLocation executableLocation)
        {
            LocationsAskedToDownloadWhenMissing.Add(executableLocation);
            if (ExceptionToThrowWhenAskedToDownload != null)
            {
                throw ExceptionToThrowWhenAskedToDownload;
            }
        }

        public bool DownloadFallbackExecutableWhenItIsMissingAndReturnWhetherItIsAvailable(VideoDownloaderExecutableLocation fallbackExecutableLocation)
        {
            FallbackLocationsAskedToDownloadWhenMissing.Add(fallbackExecutableLocation);
            return FallbackIsAvailable;
        }
    }
}
