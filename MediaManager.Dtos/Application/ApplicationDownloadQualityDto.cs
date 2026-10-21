namespace MediaManager.Dtos.Application;

public class ApplicationDownloadQualityDto
{
    public ApplicationVideoCodecChoice VideoCodecToPreferWhenDownloading { get; set; }
    public ApplicationMaximumVideoResolutionChoice MaximumVideoResolutionOnTheShorterSide { get; set; }
    public ApplicationAudioQualityChoice AudioQualityToDownload { get; set; }
}
