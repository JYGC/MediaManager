namespace MediaManager.Dtos.Services;

public class ServiceDownloadQualityDto
{
    public ServiceVideoCodecChoice VideoCodecToPreferWhenDownloading { get; set; }
    public ServiceMaximumVideoResolutionChoice MaximumVideoResolutionOnTheShorterSide { get; set; }
    public ServiceAudioQualityChoice AudioQualityToDownload { get; set; }
}
