using System;
using System.Collections.Generic;
using OPMF.Downloader;
using OPMF.Entities;

namespace OPMF.UnitTests.Support
{
    public class FakeDownloader : IDownloader
    {
        public List<Metadata> DownloadQueue { get; set; }
        public int TimesDownloadingWasStarted { get; private set; }
        public Action<List<Metadata>> RunWhileDownloading { get; set; } = queue => queue.ForEach(video => video.Status = MetadataStatus.Downloaded);

        public void StartDownloadingQueue()
        {
            TimesDownloadingWasStarted++;
            RunWhileDownloading(DownloadQueue);
        }
    }
}
