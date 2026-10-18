namespace MediaManager.Dtos.Services;

public class ServiceChannelDto
{
    public string SiteId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ServiceThumbnailDto Thumbnail { get; set; } = new();
    public string? Description { get; set; }
    public bool IsBlacklisted { get; set; }
    public bool IsAddedBySingleVideo { get; set; }
    public bool IsNotFoundOnVideoSite { get; set; }
    public DateTime? LastCheckedForNewVideosAt { get; set; }
    public DateTime? LatestVideoPublishedAt { get; set; }
}
