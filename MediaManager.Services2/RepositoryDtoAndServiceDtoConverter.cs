using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;

namespace MediaManager.Services2;

public static class RepositoryDtoAndServiceDtoConverter
{
    public static ServiceThumbnailDto ConvertThumbnailToServiceDto(RepositoryThumbnailDto thumbnailDto)
    {
        return new ServiceThumbnailDto
        {
            Url = thumbnailDto.Url,
            WidthInPixels = thumbnailDto.WidthInPixels,
            HeightInPixels = thumbnailDto.HeightInPixels,
        };
    }

    public static RepositoryThumbnailDto ConvertThumbnailToRepositoryDto(ServiceThumbnailDto thumbnailDto)
    {
        return new RepositoryThumbnailDto
        {
            Url = thumbnailDto.Url,
            WidthInPixels = thumbnailDto.WidthInPixels,
            HeightInPixels = thumbnailDto.HeightInPixels,
        };
    }

    public static ServiceChannelDto ConvertChannelToServiceDto(RepositoryChannelDto channelDto)
    {
        return new ServiceChannelDto
        {
            SiteId = channelDto.SiteId,
            Url = channelDto.Url,
            Name = channelDto.Name,
            Thumbnail = ConvertThumbnailToServiceDto(channelDto.Thumbnail),
            Description = channelDto.Description,
            IsBlacklisted = channelDto.IsBlacklisted,
            IsAddedBySingleVideo = channelDto.IsAddedBySingleVideo,
            IsNotFoundOnVideoSite = channelDto.IsNotFoundOnVideoSite,
            LastCheckedForNewVideosAt = channelDto.LastCheckedForNewVideosAt,
            LatestVideoPublishedAt = channelDto.LatestVideoPublishedAt,
        };
    }

    public static RepositoryChannelDto ConvertChannelToRepositoryDto(ServiceChannelDto channelDto)
    {
        return new RepositoryChannelDto
        {
            SiteId = channelDto.SiteId,
            Url = channelDto.Url,
            Name = channelDto.Name,
            Thumbnail = ConvertThumbnailToRepositoryDto(channelDto.Thumbnail),
            Description = channelDto.Description,
            IsBlacklisted = channelDto.IsBlacklisted,
            IsAddedBySingleVideo = channelDto.IsAddedBySingleVideo,
            IsNotFoundOnVideoSite = channelDto.IsNotFoundOnVideoSite,
            LastCheckedForNewVideosAt = channelDto.LastCheckedForNewVideosAt,
            LatestVideoPublishedAt = channelDto.LatestVideoPublishedAt,
        };
    }

    public static ServiceVideoMetadataDto ConvertVideoToServiceDto(RepositoryVideoMetadataDto videoDto)
    {
        return new ServiceVideoMetadataDto
        {
            SiteId = videoDto.SiteId,
            Url = videoDto.Url,
            Title = videoDto.Title,
            Thumbnail = ConvertThumbnailToServiceDto(videoDto.Thumbnail),
            Description = videoDto.Description,
            DownloadStatus = ConvertToServiceVideoDownloadStatus(videoDto.DownloadStatus),
            IsBeingDownloaded = videoDto.IsBeingDownloaded,
            PublishedAt = videoDto.PublishedAt,
            Duration = videoDto.Duration,
            ChannelSiteId = videoDto.ChannelSiteId,
            VideoCodecChoiceOverride = ConvertToServiceVideoCodecChoiceOrNull(videoDto.VideoCodecChoiceOverride),
            MaximumVideoResolutionChoiceOverride = ConvertToServiceMaximumVideoResolutionChoiceOrNull(videoDto.MaximumVideoResolutionChoiceOverride),
            AudioQualityChoiceOverride = ConvertToServiceAudioQualityChoiceOrNull(videoDto.AudioQualityChoiceOverride),
        };
    }

    public static RepositoryVideoMetadataDto ConvertVideoToRepositoryDto(ServiceVideoMetadataDto videoDto)
    {
        return new RepositoryVideoMetadataDto
        {
            SiteId = videoDto.SiteId,
            Url = videoDto.Url,
            Title = videoDto.Title,
            Thumbnail = ConvertThumbnailToRepositoryDto(videoDto.Thumbnail),
            Description = videoDto.Description,
            DownloadStatus = ConvertToRepositoryVideoDownloadStatus(videoDto.DownloadStatus),
            IsBeingDownloaded = videoDto.IsBeingDownloaded,
            PublishedAt = videoDto.PublishedAt,
            Duration = videoDto.Duration,
            ChannelSiteId = videoDto.ChannelSiteId,
            VideoCodecChoiceOverride = ConvertToRepositoryVideoCodecChoiceOrNull(videoDto.VideoCodecChoiceOverride),
            MaximumVideoResolutionChoiceOverride = ConvertToRepositoryMaximumVideoResolutionChoiceOrNull(videoDto.MaximumVideoResolutionChoiceOverride),
            AudioQualityChoiceOverride = ConvertToRepositoryAudioQualityChoiceOrNull(videoDto.AudioQualityChoiceOverride),
        };
    }

    public static ServiceVideoWithChannelDto ConvertVideoWithChannelToServiceDto(RepositoryVideoMetadataDto videoDto, RepositoryChannelDto? channelDto)
    {
        return new ServiceVideoWithChannelDto
        {
            Video = ConvertVideoToServiceDto(videoDto),
            Channel = channelDto == null ? null : ConvertChannelToServiceDto(channelDto),
        };
    }

    public static ServiceVideoDownloadStatus ConvertToServiceVideoDownloadStatus(RepositoryVideoDownloadStatus status)
    {
        return status switch
        {
            RepositoryVideoDownloadStatus.New => ServiceVideoDownloadStatus.New,
            RepositoryVideoDownloadStatus.ToDownload => ServiceVideoDownloadStatus.ToDownload,
            RepositoryVideoDownloadStatus.Wait => ServiceVideoDownloadStatus.Wait,
            RepositoryVideoDownloadStatus.Ignore => ServiceVideoDownloadStatus.Ignore,
            RepositoryVideoDownloadStatus.Downloaded => ServiceVideoDownloadStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No service video download status matches this repository video download status."),
        };
    }

    public static RepositoryVideoDownloadStatus ConvertToRepositoryVideoDownloadStatus(ServiceVideoDownloadStatus status)
    {
        return status switch
        {
            ServiceVideoDownloadStatus.New => RepositoryVideoDownloadStatus.New,
            ServiceVideoDownloadStatus.ToDownload => RepositoryVideoDownloadStatus.ToDownload,
            ServiceVideoDownloadStatus.Wait => RepositoryVideoDownloadStatus.Wait,
            ServiceVideoDownloadStatus.Ignore => RepositoryVideoDownloadStatus.Ignore,
            ServiceVideoDownloadStatus.Downloaded => RepositoryVideoDownloadStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No repository video download status matches this service video download status."),
        };
    }

    private static ServiceVideoCodecChoice? ConvertToServiceVideoCodecChoiceOrNull(RepositoryVideoCodecChoice? choice)
    {
        return choice switch
        {
            null => null,
            RepositoryVideoCodecChoice.H264 => ServiceVideoCodecChoice.H264,
            RepositoryVideoCodecChoice.AnyCodec => ServiceVideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service video codec choice matches this repository video codec choice."),
        };
    }

    private static RepositoryVideoCodecChoice? ConvertToRepositoryVideoCodecChoiceOrNull(ServiceVideoCodecChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceVideoCodecChoice.H264 => RepositoryVideoCodecChoice.H264,
            ServiceVideoCodecChoice.AnyCodec => RepositoryVideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No repository video codec choice matches this service video codec choice."),
        };
    }

    private static ServiceMaximumVideoResolutionChoice? ConvertToServiceMaximumVideoResolutionChoiceOrNull(RepositoryMaximumVideoResolutionChoice? choice)
    {
        return choice switch
        {
            null => null,
            RepositoryMaximumVideoResolutionChoice.NoLimit => ServiceMaximumVideoResolutionChoice.NoLimit,
            RepositoryMaximumVideoResolutionChoice.UpTo2160p => ServiceMaximumVideoResolutionChoice.UpTo2160p,
            RepositoryMaximumVideoResolutionChoice.UpTo1440p => ServiceMaximumVideoResolutionChoice.UpTo1440p,
            RepositoryMaximumVideoResolutionChoice.UpTo1080p => ServiceMaximumVideoResolutionChoice.UpTo1080p,
            RepositoryMaximumVideoResolutionChoice.UpTo720p => ServiceMaximumVideoResolutionChoice.UpTo720p,
            RepositoryMaximumVideoResolutionChoice.UpTo480p => ServiceMaximumVideoResolutionChoice.UpTo480p,
            RepositoryMaximumVideoResolutionChoice.UpTo360p => ServiceMaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service maximum video resolution choice matches this repository maximum video resolution choice."),
        };
    }

    private static RepositoryMaximumVideoResolutionChoice? ConvertToRepositoryMaximumVideoResolutionChoiceOrNull(ServiceMaximumVideoResolutionChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceMaximumVideoResolutionChoice.NoLimit => RepositoryMaximumVideoResolutionChoice.NoLimit,
            ServiceMaximumVideoResolutionChoice.UpTo2160p => RepositoryMaximumVideoResolutionChoice.UpTo2160p,
            ServiceMaximumVideoResolutionChoice.UpTo1440p => RepositoryMaximumVideoResolutionChoice.UpTo1440p,
            ServiceMaximumVideoResolutionChoice.UpTo1080p => RepositoryMaximumVideoResolutionChoice.UpTo1080p,
            ServiceMaximumVideoResolutionChoice.UpTo720p => RepositoryMaximumVideoResolutionChoice.UpTo720p,
            ServiceMaximumVideoResolutionChoice.UpTo480p => RepositoryMaximumVideoResolutionChoice.UpTo480p,
            ServiceMaximumVideoResolutionChoice.UpTo360p => RepositoryMaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No repository maximum video resolution choice matches this service maximum video resolution choice."),
        };
    }

    private static ServiceAudioQualityChoice? ConvertToServiceAudioQualityChoiceOrNull(RepositoryAudioQualityChoice? choice)
    {
        return choice switch
        {
            null => null,
            RepositoryAudioQualityChoice.BestAac => ServiceAudioQualityChoice.BestAac,
            RepositoryAudioQualityChoice.BestOfAnyCodec => ServiceAudioQualityChoice.BestOfAnyCodec,
            RepositoryAudioQualityChoice.SmallestAac => ServiceAudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service audio quality choice matches this repository audio quality choice."),
        };
    }

    private static RepositoryAudioQualityChoice? ConvertToRepositoryAudioQualityChoiceOrNull(ServiceAudioQualityChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceAudioQualityChoice.BestAac => RepositoryAudioQualityChoice.BestAac,
            ServiceAudioQualityChoice.BestOfAnyCodec => RepositoryAudioQualityChoice.BestOfAnyCodec,
            ServiceAudioQualityChoice.SmallestAac => RepositoryAudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No repository audio quality choice matches this service audio quality choice."),
        };
    }

    public static ServiceLogEntryType ConvertToServiceLogEntryType(RepositoryLogEntryType logEntryType)
    {
        return logEntryType switch
        {
            RepositoryLogEntryType.Info => ServiceLogEntryType.Info,
            RepositoryLogEntryType.Warning => ServiceLogEntryType.Warning,
            RepositoryLogEntryType.Error => ServiceLogEntryType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logEntryType), logEntryType, "No service log entry type matches this repository log entry type."),
        };
    }

    public static ServiceLogEntryDto ConvertLogEntryToServiceDto(RepositoryLogEntryDto logEntryDto)
    {
        return new ServiceLogEntryDto
        {
            Id = logEntryDto.Id,
            Message = logEntryDto.Message,
            LogType = ConvertToServiceLogEntryType(logEntryDto.LogType),
            CreatedAt = logEntryDto.CreatedAt,
            ExceptionText = logEntryDto.ExceptionText,
            VariablesAsIndentedJson = logEntryDto.VariablesAsIndentedJson,
        };
    }
}
