using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;
using MediaManager.Repositories;

namespace MediaManager.Services2
{
    public interface IChannelMetadataServices
    {
        List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(
            string wordInChannelName, string wordInVideoTitle, int skip, int pageSize);
        List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize);
        List<ServiceVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize);
        List<ServiceVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize);
        ServiceVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl);
    }

    public class ChannelMetadataServices(
        IChannelRepository channelRepository,
        IVideoMetadataRepository videoMetadataRepository,
        IVideoSiteRepository videoSiteRepository) : IChannelMetadataServices
    {
        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(
            string wordInChannelName, string wordInVideoTitle, int skip, int pageSize)
        {
            Dictionary<string, RepositoryChannelDto> channelsWithSiteId = channelRepository
                .GetChannelsWhoseNameContains(wordInChannelName)
                .ToDictionary(channel => channel.SiteId, channel => channel);

            List<RepositoryVideoMetadataDto> videos = videoMetadataRepository
                .GetVideosOfChannelsWhoseTitleContains(channelsWithSiteId.Keys, wordInVideoTitle, skip, pageSize);

            return videos
                .Select(video => RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(video, channelsWithSiteId[video.ChannelSiteId]))
                .ToList();
        }

        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsWhereVideoTitleContainsWord(string wordInVideoTitle, int skip, int pageSize)
        {
            return JoinVideosWithTheirChannels(videoMetadataRepository.GetVideosWhoseTitleContains(wordInVideoTitle, skip, pageSize));
        }

        public List<ServiceVideoWithChannelDto> GetNewVideosWithChannels(int skip, int pageSize)
        {
            return JoinVideosWithTheirChannels(videoMetadataRepository.GetNewVideos(skip, pageSize));
        }

        public List<ServiceVideoWithChannelDto> GetVideosWithChannelsQueuedToDownloadOrWaiting(int skip, int pageSize)
        {
            return JoinVideosWithTheirChannels(videoMetadataRepository.GetVideosQueuedToDownloadOrWaiting(skip, pageSize));
        }

        public ServiceVideoWithChannelDto GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(string videoUrl)
        {
            string videoSiteId = videoSiteRepository.GetVideoSiteIdFromVideoUrl(videoUrl);

            RepositoryVideoMetadataDto? storedVideo = videoMetadataRepository.GetFirstVideoWhoseSiteIdContains(videoSiteId);
            if (storedVideo != null)
            {
                RepositoryChannelDto? channelOfStoredVideo = channelRepository.GetChannelWithSiteId(storedVideo.ChannelSiteId);
                return RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(storedVideo, channelOfStoredVideo);
            }

            RepositoryVideoWithChannelDto fetchedVideoWithChannel = videoSiteRepository.FetchVideoWithItsChannelFromVideoSite(videoSiteId);
            if (fetchedVideoWithChannel.Channel == null || fetchedVideoWithChannel.Video == null)
            {
                throw new Exception("Failed to get video");
            }
            videoMetadataRepository.InsertVideosThatAreNotStoredYet([fetchedVideoWithChannel.Video]);
            channelRepository.InsertNewChannelsAndUpdateNameUrlThumbnailAndDescriptionOfExistingChannels([fetchedVideoWithChannel.Channel]);
            return RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(fetchedVideoWithChannel.Video, fetchedVideoWithChannel.Channel);
        }

        private List<ServiceVideoWithChannelDto> JoinVideosWithTheirChannels(List<RepositoryVideoMetadataDto> videos)
        {
            List<string> channelSiteIds = videos.Select(video => video.ChannelSiteId).Distinct().ToList();
            Dictionary<string, RepositoryChannelDto> channelsWithSiteIds = channelRepository
                .GetChannelsWithSiteIds(channelSiteIds)
                .ToDictionary(channel => channel.SiteId, channel => channel);

            return videos
                .Select(video => RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(video, channelsWithSiteIds[video.ChannelSiteId]))
                .ToList();
        }
    }
}
