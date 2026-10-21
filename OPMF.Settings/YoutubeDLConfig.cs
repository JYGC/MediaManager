namespace OPMF.Settings
{
    public class YoutubeDLConfig
    {
        public bool CheckForBinaryUpdates { get; set; } = true;
        public bool GetSubtitles { get; set; } = false;
        public string VideoExtension { get; set; } = "mp4";
        public string SubtitleExtension { get; set; } = "vtt";
        public int MaxNoOfParallelDownloads { get; set; } = 5;
        public VideoDownloaderExecutable ExecutableTriedFirstToDownloadVideos { get; set; } = VideoDownloaderExecutable.YoutubeDl;
        public VideoCodecChoice VideoCodecToPreferWhenDownloading { get; set; } = VideoCodecChoice.H264;
        public MaximumVideoResolutionChoice MaximumVideoResolutionOnTheShorterSide { get; set; } = MaximumVideoResolutionChoice.NoLimit;
        public AudioQualityChoice AudioQualityToDownload { get; set; } = AudioQualityChoice.BestAac;
        public string[] FalseErrorMessages { get; set; } =
        [
            "If you know what you are doing and want only the best pre-merged format, use \"-f b\" instead to suppress this warning",
        ];
    }
}
