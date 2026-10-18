namespace MediaManager.Dtos.Repositories;

public class RepositoryVideoMetadataDto
{
    public string SiteId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public RepositoryThumbnailDto Thumbnail { get; set; } = new();
    public string? Description { get; set; }
    public RepositoryVideoDownloadStatus DownloadStatus { get; set; }
    public bool IsBeingDownloaded { get; set; }
    public DateTime PublishedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string ChannelSiteId { get; set; } = string.Empty;
    public object? OriginalResponseFromVideoSite { get; set; }
}
