using System;
using System.IO;
using NYoutubeDL;

namespace OPMF.Downloader.YTDownloader
{
    public record VideoDownloaderExecutableRunOutcome(string LastErrorOutputLineOrNull, bool OutputFileExistsAfterTheRun);

    public interface IVideoDownloaderExecutableRunner
    {
        VideoDownloaderExecutableRunOutcome RunExecutableToDownloadVideoAndReportWhetherTheOutputFileExists(
            string executableFilePath,
            bool tellExecutableNotToResumePartialFiles,
            string videoUrl,
            string outputFilePath,
            string formatSelector,
            string mergeContainer,
            Action<string> reportStandardOutputLine);
    }

    public class NYoutubeDLVideoDownloaderExecutableRunner : IVideoDownloaderExecutableRunner
    {
        private readonly bool _downloadAllSubtitles;

        public NYoutubeDLVideoDownloaderExecutableRunner(bool downloadAllSubtitles)
        {
            _downloadAllSubtitles = downloadAllSubtitles;
        }

        public VideoDownloaderExecutableRunOutcome RunExecutableToDownloadVideoAndReportWhetherTheOutputFileExists(
            string executableFilePath,
            bool tellExecutableNotToResumePartialFiles,
            string videoUrl,
            string outputFilePath,
            string formatSelector,
            string mergeContainer,
            Action<string> reportStandardOutputLine)
        {
            YoutubeDL youtubeDL = new YoutubeDL();
            youtubeDL.Options.SubtitleOptions.AllSubs = _downloadAllSubtitles;
            youtubeDL.Options.VideoFormatOptions.FormatAdvanced = formatSelector;
            youtubeDL.Options.VideoFormatOptions.MergeOutputFormat = Enum.Parse<NYoutubeDL.Helpers.Enums.VideoFormat>(mergeContainer);
            youtubeDL.YoutubeDlPath = executableFilePath;
            youtubeDL.Options.FilesystemOptions.Output = outputFilePath;
            youtubeDL.Options.FilesystemOptions.NoContinue = tellExecutableNotToResumePartialFiles;

            string lastErrorOutputLine = null;
            youtubeDL.StandardOutputEvent += (sender, output) => reportStandardOutputLine(output);
            youtubeDL.StandardErrorEvent += (sender, errorOutput) => lastErrorOutputLine = errorOutput;

            youtubeDL.Download(videoUrl);
            return new VideoDownloaderExecutableRunOutcome(lastErrorOutputLine, File.Exists(outputFilePath));
        }
    }
}
