using OPMF.Downloader.YTDownloader;

namespace OPMF.UnitTests.Support
{
    public class FakeFfmpegOnPathFinder : IFfmpegOnPathFinder
    {
        public bool IsFfmpegFound { get; set; } = true;

        public int TimesAskedWhetherFfmpegIsFound { get; private set; }

        public bool IsFfmpegFoundInAFolderOnPath()
        {
            TimesAskedWhetherFfmpegIsFound++;
            return IsFfmpegFound;
        }
    }
}
