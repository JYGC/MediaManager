namespace OPMF.Settings
{
    public class YoutubeDLConfig
    {
        public bool CheckForBinaryUpdates { get; set; } = true;
        public bool GetSubtitles { get; set; } = false;
        public string VideoExtension { get; set; } = "mp4";
        public string SubtitleExtension { get; set; } = "vtt";
        public int MaxNoOfParallelDownloads { get; set; } = 5;
        public string[] FalseErrorMessages { get; set; } =
        [
            "If you know what you are doing and want only the best pre-merged format, use \"-f b\" instead to suppress this warning",
        ];
    }
}
