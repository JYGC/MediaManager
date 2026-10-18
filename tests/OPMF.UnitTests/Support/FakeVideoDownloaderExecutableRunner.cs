using System;
using System.Collections.Generic;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public record VideoDownloaderExecutableRun(string ExecutableFilePath, bool TellExecutableNotToResumePartialFiles, string VideoUrl, string OutputFilePath);

    public class FakeVideoDownloaderExecutableRunner : IVideoDownloaderExecutableRunner
    {
        private record ScriptedRun(string[] StandardOutputLines, string LastErrorOutputLine, Exception ExceptionToThrow);

        private readonly Queue<ScriptedRun> _scriptedRunsInOrder = new Queue<ScriptedRun>();

        public List<VideoDownloaderExecutableRun> Runs { get; } = new List<VideoDownloaderExecutableRun>();

        public void ScriptNextRun(string[] standardOutputLines = null, string lastErrorOutputLine = null, Exception exceptionToThrow = null)
        {
            _scriptedRunsInOrder.Enqueue(new ScriptedRun(standardOutputLines ?? new string[0], lastErrorOutputLine, exceptionToThrow));
        }

        public string RunExecutableToDownloadVideoAndReturnLastErrorOutputLine(
            string executableFilePath,
            bool tellExecutableNotToResumePartialFiles,
            string videoUrl,
            string outputFilePath,
            Action<string> reportStandardOutputLine)
        {
            Runs.Add(new VideoDownloaderExecutableRun(executableFilePath, tellExecutableNotToResumePartialFiles, videoUrl, outputFilePath));
            var scriptedRun = _scriptedRunsInOrder.Count > 0 ? _scriptedRunsInOrder.Dequeue() : new ScriptedRun(new string[0], null, null);
            foreach (var standardOutputLine in scriptedRun.StandardOutputLines)
            {
                reportStandardOutputLine(standardOutputLine);
            }
            if (scriptedRun.ExceptionToThrow != null)
            {
                throw scriptedRun.ExceptionToThrow;
            }
            return scriptedRun.LastErrorOutputLine;
        }
    }
}
