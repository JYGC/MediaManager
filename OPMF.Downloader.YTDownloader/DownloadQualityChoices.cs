using OPMF.Settings;

namespace OPMF.Downloader.YTDownloader
{
    public record DownloadQualityChoices(VideoCodecChoice VideoCodec, MaximumVideoResolutionChoice MaximumVideoResolution, AudioQualityChoice AudioQuality);
}
