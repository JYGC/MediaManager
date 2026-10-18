using System;
using NYoutubeDL;

namespace OPMF.Downloader.YTDownloader
{
    public interface IVideoDownloaderExecutableRunner
    {
        string RunExecutableToDownloadVideoAndReturnLastErrorOutputLine(
            string executableFilePath,
            bool tellExecutableNotToResumePartialFiles,
            string videoUrl,
            string outputFilePath,
            Action<string> reportStandardOutputLine);
    }

    public class NYoutubeDLVideoDownloaderExecutableRunner : IVideoDownloaderExecutableRunner
    {
        private readonly bool _downloadAllSubtitles;

        public NYoutubeDLVideoDownloaderExecutableRunner(bool downloadAllSubtitles)
        {
            _downloadAllSubtitles = downloadAllSubtitles;
        }

        public string RunExecutableToDownloadVideoAndReturnLastErrorOutputLine(
            string executableFilePath,
            bool tellExecutableNotToResumePartialFiles,
            string videoUrl,
            string outputFilePath,
            Action<string> reportStandardOutputLine)
        {
            YoutubeDL youtubeDL = new YoutubeDL();
            youtubeDL.Options.SubtitleOptions.AllSubs = _downloadAllSubtitles;
            youtubeDL.Options.VideoFormatOptions.Format = NYoutubeDL.Helpers.Enums.VideoFormat.best;
            youtubeDL.YoutubeDlPath = executableFilePath;
            youtubeDL.Options.FilesystemOptions.Output = outputFilePath;
            youtubeDL.Options.FilesystemOptions.NoContinue = tellExecutableNotToResumePartialFiles;

            string lastErrorOutputLine = null;
            youtubeDL.StandardOutputEvent += (sender, output) => reportStandardOutputLine(output);
            youtubeDL.StandardErrorEvent += (sender, errorOutput) => lastErrorOutputLine = errorOutput;

            youtubeDL.Download(videoUrl);
            return lastErrorOutputLine;
        }
    }
}
