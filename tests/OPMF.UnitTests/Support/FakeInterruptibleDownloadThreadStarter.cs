using System;
using System.Collections.Generic;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public class FakeInterruptibleDownloadThreadStarter : IInterruptibleDownloadThreadStarter
    {
        private readonly List<Action> _threadBodiesNotRunYet = new List<Action>();

        public int ThreadsStarted { get; private set; }
        public int InterruptActionsInvoked { get; private set; }

        public Action StartThreadThatRunsAndReturnActionThatInterruptsIt(Action threadBody)
        {
            ThreadsStarted++;
            _threadBodiesNotRunYet.Add(threadBody);
            return () => InterruptActionsInvoked++;
        }

        public void RunEveryThreadBodyThatHasNotRunYet()
        {
            var threadBodiesToRun = _threadBodiesNotRunYet.ToArray();
            _threadBodiesNotRunYet.Clear();
            foreach (var threadBody in threadBodiesToRun)
            {
                threadBody();
            }
        }
    }
}
