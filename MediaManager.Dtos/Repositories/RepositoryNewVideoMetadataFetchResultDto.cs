namespace MediaManager.Dtos.Repositories;

public class RepositoryNewVideoMetadataFetchResultDto
{
    public List<RepositoryChannelDto> ChannelsWithUpdatedCheckDates { get; set; } = new();
    public List<RepositoryVideoMetadataDto> NewlyFetchedVideos { get; set; } = new();
}
