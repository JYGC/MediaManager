namespace MediaManager.Dtos.Services;

public class ServiceVideoWithChannelDto
{
    public ServiceVideoMetadataDto Video { get; set; } = new();
    public ServiceChannelDto? Channel { get; set; }
}
