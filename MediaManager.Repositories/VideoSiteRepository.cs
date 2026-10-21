using MediaManager.Dtos.Repositories;
using OPMF.Entities;
using OPMF.SiteAdapter;

namespace MediaManager.Repositories;

public interface IVideoSiteRepository
{
    RepositoryNewVideoMetadataFetchResultDto FetchNewVideoMetadataForChannels(List<RepositoryChannelDto> channelsToCheckForNewVideos);
    string GetVideoSiteIdFromVideoUrl(string videoUrl);
    RepositoryVideoWithChannelDto FetchVideoWithItsChannelFromVideoSite(string videoSiteId);
}

public class VideoSiteRepository(
    Func<IMetadataFetcher<Channel, Metadata>> createMetadataFetcherAfterSigningInToGoogle,
    Func<ISiteVideoMetadataGetter> createVideoMetadataGetterAfterSigningInToGoogle) : IVideoSiteRepository
{
    private IMetadataFetcher<Channel, Metadata>? _metadataFetcher;
    private ISiteVideoMetadataGetter? _videoMetadataGetter;

    public RepositoryNewVideoMetadataFetchResultDto FetchNewVideoMetadataForChannels(List<RepositoryChannelDto> channelsToCheckForNewVideos)
    {
        _metadataFetcher ??= createMetadataFetcherAfterSigningInToGoogle();
        List<Channel> channels = channelsToCheckForNewVideos
            .Select(channelDto => (Channel)DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeChannel(channelDto))
            .ToList();

        List<Metadata> newlyFetchedVideos = _metadataFetcher.FetchMetadata(ref channels);

        return new RepositoryNewVideoMetadataFetchResultDto
        {
            ChannelsWithUpdatedCheckDates = channels.Select(DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto).ToList(),
            NewlyFetchedVideos = newlyFetchedVideos.Select(DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto).ToList(),
        };
    }

    public string GetVideoSiteIdFromVideoUrl(string videoUrl)
    {
        _videoMetadataGetter ??= createVideoMetadataGetterAfterSigningInToGoogle();
        return _videoMetadataGetter.GetSiteIdFromURL(videoUrl);
    }

    public RepositoryVideoWithChannelDto FetchVideoWithItsChannelFromVideoSite(string videoSiteId)
    {
        _videoMetadataGetter ??= createVideoMetadataGetterAfterSigningInToGoogle();
        (Metadata? video, Channel? channel) = _videoMetadataGetter.GetVideoByURL(videoSiteId);
        return new RepositoryVideoWithChannelDto
        {
            Video = video == null ? null : DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(video),
            Channel = channel == null ? null : DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto(channel),
        };
    }
}
