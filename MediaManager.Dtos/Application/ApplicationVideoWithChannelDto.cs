namespace MediaManager.Dtos.Application;

public class ApplicationVideoWithChannelDto
{
    public ApplicationVideoMetadataDto Video { get; set; } = new();
    public ApplicationChannelDto? Channel { get; set; }
}
