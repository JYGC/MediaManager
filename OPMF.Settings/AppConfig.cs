using System;
using System.IO;

namespace OPMF.Settings
{
    public class AppConfig
    {
        public int NewChannelPastVideoDayLimit { get; set; } = 1;
        public int StopDownloadInstanceAfterSeconds { get; set; } = 60 * 60 * 12;
        public string GoogleClientSecretPath { get; set; } = "gmail-python-quickstart.json";
        public string VideoOutputFolderPath { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OffPeakVideos");
        public YoutubeDLConfig YoutubeDL { get; set; } = new YoutubeDLConfig();
    }
}
