using System;

namespace OPMF.Downloader.YTDownloader
{
    public interface IConsoleRowOverwriter
    {
        void OverwriteConsoleRowWithMessage(int consoleRow, string message);
    }

    public class ConsoleRowOverwriter : IConsoleRowOverwriter
    {
        public void OverwriteConsoleRowWithMessage(int consoleRow, string message)
        {
            Console.SetCursorPosition(0, consoleRow);
            Console.WriteLine(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, consoleRow);
            Console.WriteLine(message);
        }
    }
}
