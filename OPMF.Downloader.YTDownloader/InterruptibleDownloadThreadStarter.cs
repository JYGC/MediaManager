using System;
using System.Threading;

namespace OPMF.Downloader.YTDownloader
{
    public interface IInterruptibleDownloadThreadStarter
    {
        Action StartThreadThatRunsAndReturnActionThatInterruptsIt(Action threadBody);
    }

    public class InterruptibleDownloadThreadStarter : IInterruptibleDownloadThreadStarter
    {
        public Action StartThreadThatRunsAndReturnActionThatInterruptsIt(Action threadBody)
        {
            Thread thread = new Thread(() => threadBody());
            thread.Start();
            return thread.Interrupt;
        }
    }
}
