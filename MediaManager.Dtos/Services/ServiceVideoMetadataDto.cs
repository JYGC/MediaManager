namespace MediaManager.Dtos.Services;

public class ServiceVideoMetadataDto
{
    public string SiteId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ServiceThumbnailDto Thumbnail { get; set; } = new();
    public string? Description { get; set; }
    public ServiceVideoDownloadStatus DownloadStatus { get; set; }
    public bool IsBeingDownloaded { get; set; }
    public DateTime PublishedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string ChannelSiteId { get; set; } = string.Empty;
}
