using System;
using System.Collections.Generic;
using Microsoft.Extensions.Time.Testing;
using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public class FakeCurrentThreadWaiter : ICurrentThreadWaiter
    {
        private readonly FakeTimeProvider _fakeTimeProvider;

        public List<TimeSpan> Waits { get; } = new List<TimeSpan>();
        public Action RunWhileWaiting { get; set; } = () => { };

        public FakeCurrentThreadWaiter(FakeTimeProvider fakeTimeProvider)
        {
            _fakeTimeProvider = fakeTimeProvider;
        }

        public void WaitOnCurrentThreadFor(TimeSpan timeToWait)
        {
            Waits.Add(timeToWait);
            _fakeTimeProvider.Advance(timeToWait);
            RunWhileWaiting();
        }
    }
}
