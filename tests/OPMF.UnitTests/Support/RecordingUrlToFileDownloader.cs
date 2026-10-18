using System;
using System.Collections.Generic;
using System.IO;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public record UrlToFileDownload(string Url, string FilePath, string StandardOutputWhenTheDownloadWasRequested);

    public class RecordingUrlToFileDownloader : IUrlToFileDownloader
    {
        private readonly StringWriter _standardOutputToSnapshotOnEveryDownload;

        public List<UrlToFileDownload> Downloads { get; } = new List<UrlToFileDownload>();
        public Exception ExceptionToThrowOnEveryDownload { get; set; }

        public RecordingUrlToFileDownloader(StringWriter standardOutputToSnapshotOnEveryDownload)
        {
            _standardOutputToSnapshotOnEveryDownload = standardOutputToSnapshotOnEveryDownload;
        }

        public void DownloadFileFromUrlToFilePath(string url, string filePath)
        {
            Downloads.Add(new UrlToFileDownload(url, filePath, _standardOutputToSnapshotOnEveryDownload.ToString()));
            if (ExceptionToThrowOnEveryDownload != null)
            {
                throw ExceptionToThrowOnEveryDownload;
            }
        }
    }
}
