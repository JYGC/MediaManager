using System.Collections.Generic;
using System.Linq;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public record ConsoleRowMessage(int ConsoleRow, string Message);

    public class RecordingConsoleRowOverwriter : IConsoleRowOverwriter
    {
        public List<ConsoleRowMessage> Overwrites { get; } = new List<ConsoleRowMessage>();

        public void OverwriteConsoleRowWithMessage(int consoleRow, string message)
        {
            Overwrites.Add(new ConsoleRowMessage(consoleRow, message));
        }

        public List<string> MessagesWrittenOnConsoleRow(int consoleRow)
        {
            return Overwrites.Where(overwrite => overwrite.ConsoleRow == consoleRow).Select(overwrite => overwrite.Message).ToList();
        }
    }
}
