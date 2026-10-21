using System;
using System.Threading;

namespace OPMF.Downloader.YTDownloader
{
    public interface ICurrentThreadWaiter
    {
        void WaitOnCurrentThreadFor(TimeSpan timeToWait);
    }

    public class ThreadSleepCurrentThreadWaiter : ICurrentThreadWaiter
    {
        public void WaitOnCurrentThreadFor(TimeSpan timeToWait)
        {
            Thread.Sleep(timeToWait);
        }
    }
}
