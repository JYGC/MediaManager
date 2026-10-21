namespace MediaManager.Dtos.Application;

public class ApplicationVideoMetadataDto
{
    public string SiteId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ApplicationThumbnailDto Thumbnail { get; set; } = new();
    public string? Description { get; set; }
    public ApplicationVideoDownloadStatus DownloadStatus { get; set; }
    public bool IsBeingDownloaded { get; set; }
    public DateTime PublishedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string ChannelSiteId { get; set; } = string.Empty;
    public ApplicationVideoCodecChoice? VideoCodecChoiceOverride { get; set; }
    public ApplicationMaximumVideoResolutionChoice? MaximumVideoResolutionChoiceOverride { get; set; }
    public ApplicationAudioQualityChoice? AudioQualityChoiceOverride { get; set; }
}
