using System.Net;

namespace OPMF.Downloader.YTDownloader
{
    public interface IUrlToFileDownloader
    {
        void DownloadFileFromUrlToFilePath(string url, string filePath);
    }

    public class WebClientUrlToFileDownloader : IUrlToFileDownloader
    {
        public void DownloadFileFromUrlToFilePath(string url, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, filePath);
            }
        }
    }
}
