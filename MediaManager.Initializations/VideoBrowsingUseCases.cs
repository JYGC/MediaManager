using MediaManager.Dtos.Application;
using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface IVideoBrowsingUseCases
{
    List<ApplicationVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize);
    List<ApplicationVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize);
    List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize);
    List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(string wordInChannelName, string wordInVideoTitle, int skip, int pageSize);
    ApplicationVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl);
    void UpdateDownloadStatusOfVideos(IEnumerable<ApplicationVideoMetadataDto> videos);
    ApplicationVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ApplicationVideoMetadataDto video);
    ApplicationVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ApplicationVideoMetadataDto video);
    void SaveDownloadQualityChoiceOverridesOfVideo(ApplicationVideoMetadataDto video);
}

public class VideoBrowsingUseCases(IChannelMetadataServices channelMetadataServices, IMetadataServices metadataServices) : IVideoBrowsingUseCases
{
    public List<ApplicationVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize)
    {
        return ConvertToApplicationDtos(channelMetadataServices.GetNewVideosWithChannels(skip, pageSize));
    }

    public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize)
    {
        return ConvertToApplicationDtos(channelMetadataServices.GetVideosWithChannelsQueuedToDownloadOrWaiting(skip, pageSize));
    }

    public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize)
    {
        return ConvertToApplicationDtos(channelMetadataServices.GetVideosWithChannelsWhereVideoTitleContainsWord(wordInVideoTitle, skip, pageSize));
    }

    public List<ApplicationVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(string wordInChannelName, string wordInVideoTitle, int skip, int pageSize)
    {
        return ConvertToApplicationDtos(
            channelMetadataServices.GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(wordInChannelName, wordInVideoTitle, skip, pageSize));
    }

    public ApplicationVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl)
    {
        return ServiceDtoAndApplicationDtoConverter.ConvertVideoWithChannelToApplicationDto(
            channelMetadataServices.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(videoUrl));
    }

    public void UpdateDownloadStatusOfVideos(IEnumerable<ApplicationVideoMetadataDto> videos)
    {
        metadataServices.UpdateDownloadStatusOfVideos(videos.Select(ServiceDtoAndApplicationDtoConverter.ConvertVideoToServiceDto).ToList());
    }

    public ApplicationVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ApplicationVideoMetadataDto video)
    {
        return ServiceDtoAndApplicationDtoConverter.ConvertVideoToApplicationDto(
            metadataServices.ClearBeingDownloadedFlagOfVideo(ServiceDtoAndApplicationDtoConverter.ConvertVideoToServiceDto(video)));
    }

    public ApplicationVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ApplicationVideoMetadataDto video)
    {
        return ServiceDtoAndApplicationDtoConverter.ConvertVideoToApplicationDto(
            metadataServices.MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceDtoAndApplicationDtoConverter.ConvertVideoToServiceDto(video)));
    }

    public void SaveDownloadQualityChoiceOverridesOfVideo(ApplicationVideoMetadataDto video)
    {
        metadataServices.SaveDownloadQualityChoiceOverridesOfVideo(ServiceDtoAndApplicationDtoConverter.ConvertVideoToServiceDto(video));
    }

    private static List<ApplicationVideoWithChannelDto> ConvertToApplicationDtos(List<MediaManager.Dtos.Services.ServiceVideoWithChannelDto> videosWithChannels)
    {
        return videosWithChannels.Select(ServiceDtoAndApplicationDtoConverter.ConvertVideoWithChannelToApplicationDto).ToList();
    }
}
