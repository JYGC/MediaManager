using MediaManager.Dtos.Application;
using MediaManager.Dtos.Services;

namespace MediaManager.Initializations;

public static class ServiceDtoAndApplicationDtoConverter
{
    public static ApplicationThumbnailDto ConvertThumbnailToApplicationDto(ServiceThumbnailDto thumbnailDto)
    {
        return new ApplicationThumbnailDto
        {
            Url = thumbnailDto.Url,
            WidthInPixels = thumbnailDto.WidthInPixels,
            HeightInPixels = thumbnailDto.HeightInPixels,
        };
    }

    public static ServiceThumbnailDto ConvertThumbnailToServiceDto(ApplicationThumbnailDto thumbnailDto)
    {
        return new ServiceThumbnailDto
        {
            Url = thumbnailDto.Url,
            WidthInPixels = thumbnailDto.WidthInPixels,
            HeightInPixels = thumbnailDto.HeightInPixels,
        };
    }

    public static ApplicationChannelDto ConvertChannelToApplicationDto(ServiceChannelDto channelDto)
    {
        return new ApplicationChannelDto
        {
            SiteId = channelDto.SiteId,
            Url = channelDto.Url,
            Name = channelDto.Name,
            Thumbnail = ConvertThumbnailToApplicationDto(channelDto.Thumbnail),
            Description = channelDto.Description,
            IsBlacklisted = channelDto.IsBlacklisted,
            IsAddedBySingleVideo = channelDto.IsAddedBySingleVideo,
            IsNotFoundOnVideoSite = channelDto.IsNotFoundOnVideoSite,
            LastCheckedForNewVideosAt = channelDto.LastCheckedForNewVideosAt,
            LatestVideoPublishedAt = channelDto.LatestVideoPublishedAt,
        };
    }

    public static ServiceChannelDto ConvertChannelToServiceDto(ApplicationChannelDto channelDto)
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

    public static ApplicationVideoMetadataDto ConvertVideoToApplicationDto(ServiceVideoMetadataDto videoDto)
    {
        return new ApplicationVideoMetadataDto
        {
            SiteId = videoDto.SiteId,
            Url = videoDto.Url,
            Title = videoDto.Title,
            Thumbnail = ConvertThumbnailToApplicationDto(videoDto.Thumbnail),
            Description = videoDto.Description,
            DownloadStatus = ConvertToApplicationVideoDownloadStatus(videoDto.DownloadStatus),
            IsBeingDownloaded = videoDto.IsBeingDownloaded,
            PublishedAt = videoDto.PublishedAt,
            Duration = videoDto.Duration,
            ChannelSiteId = videoDto.ChannelSiteId,
            VideoCodecChoiceOverride = ConvertToApplicationVideoCodecChoiceOrNull(videoDto.VideoCodecChoiceOverride),
            MaximumVideoResolutionChoiceOverride = ConvertToApplicationMaximumVideoResolutionChoiceOrNull(videoDto.MaximumVideoResolutionChoiceOverride),
            AudioQualityChoiceOverride = ConvertToApplicationAudioQualityChoiceOrNull(videoDto.AudioQualityChoiceOverride),
        };
    }

    public static ServiceVideoMetadataDto ConvertVideoToServiceDto(ApplicationVideoMetadataDto videoDto)
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

    public static ApplicationDownloadQualityDto ConvertDownloadQualityToApplicationDto(ServiceDownloadQualityDto downloadQualityDto)
    {
        return new ApplicationDownloadQualityDto
        {
            VideoCodecToPreferWhenDownloading = ConvertToApplicationVideoCodecChoice(downloadQualityDto.VideoCodecToPreferWhenDownloading),
            MaximumVideoResolutionOnTheShorterSide = ConvertToApplicationMaximumVideoResolutionChoice(downloadQualityDto.MaximumVideoResolutionOnTheShorterSide),
            AudioQualityToDownload = ConvertToApplicationAudioQualityChoice(downloadQualityDto.AudioQualityToDownload),
        };
    }

    public static ServiceVideoCodecChoice ConvertToServiceVideoCodecChoice(ApplicationVideoCodecChoice choice) => ConvertToServiceVideoCodecChoiceOrNull(choice)!.Value;

    public static ServiceMaximumVideoResolutionChoice ConvertToServiceMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice choice) =>
        ConvertToServiceMaximumVideoResolutionChoiceOrNull(choice)!.Value;

    public static ServiceAudioQualityChoice ConvertToServiceAudioQualityChoice(ApplicationAudioQualityChoice choice) => ConvertToServiceAudioQualityChoiceOrNull(choice)!.Value;

    private static ApplicationVideoCodecChoice ConvertToApplicationVideoCodecChoice(ServiceVideoCodecChoice choice) => ConvertToApplicationVideoCodecChoiceOrNull(choice)!.Value;

    private static ApplicationMaximumVideoResolutionChoice ConvertToApplicationMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice choice) =>
        ConvertToApplicationMaximumVideoResolutionChoiceOrNull(choice)!.Value;

    private static ApplicationAudioQualityChoice ConvertToApplicationAudioQualityChoice(ServiceAudioQualityChoice choice) => ConvertToApplicationAudioQualityChoiceOrNull(choice)!.Value;

    public static ApplicationVideoWithChannelDto ConvertVideoWithChannelToApplicationDto(ServiceVideoWithChannelDto videoWithChannelDto)
    {
        return new ApplicationVideoWithChannelDto
        {
            Video = ConvertVideoToApplicationDto(videoWithChannelDto.Video),
            Channel = videoWithChannelDto.Channel == null ? null : ConvertChannelToApplicationDto(videoWithChannelDto.Channel),
        };
    }

    public static ApplicationVideoDownloadStatus ConvertToApplicationVideoDownloadStatus(ServiceVideoDownloadStatus status)
    {
        return status switch
        {
            ServiceVideoDownloadStatus.New => ApplicationVideoDownloadStatus.New,
            ServiceVideoDownloadStatus.ToDownload => ApplicationVideoDownloadStatus.ToDownload,
            ServiceVideoDownloadStatus.Wait => ApplicationVideoDownloadStatus.Wait,
            ServiceVideoDownloadStatus.Ignore => ApplicationVideoDownloadStatus.Ignore,
            ServiceVideoDownloadStatus.Downloaded => ApplicationVideoDownloadStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No application video download status matches this service video download status."),
        };
    }

    public static ServiceVideoDownloadStatus ConvertToServiceVideoDownloadStatus(ApplicationVideoDownloadStatus status)
    {
        return status switch
        {
            ApplicationVideoDownloadStatus.New => ServiceVideoDownloadStatus.New,
            ApplicationVideoDownloadStatus.ToDownload => ServiceVideoDownloadStatus.ToDownload,
            ApplicationVideoDownloadStatus.Wait => ServiceVideoDownloadStatus.Wait,
            ApplicationVideoDownloadStatus.Ignore => ServiceVideoDownloadStatus.Ignore,
            ApplicationVideoDownloadStatus.Downloaded => ServiceVideoDownloadStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No service video download status matches this application video download status."),
        };
    }

    private static ApplicationVideoCodecChoice? ConvertToApplicationVideoCodecChoiceOrNull(ServiceVideoCodecChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceVideoCodecChoice.H264 => ApplicationVideoCodecChoice.H264,
            ServiceVideoCodecChoice.AnyCodec => ApplicationVideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No application VideoCodecChoice matches this service VideoCodecChoice."),
        };
    }

    private static ServiceVideoCodecChoice? ConvertToServiceVideoCodecChoiceOrNull(ApplicationVideoCodecChoice? choice)
    {
        return choice switch
        {
            null => null,
            ApplicationVideoCodecChoice.H264 => ServiceVideoCodecChoice.H264,
            ApplicationVideoCodecChoice.AnyCodec => ServiceVideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service VideoCodecChoice matches this application VideoCodecChoice."),
        };
    }

    private static ApplicationMaximumVideoResolutionChoice? ConvertToApplicationMaximumVideoResolutionChoiceOrNull(ServiceMaximumVideoResolutionChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceMaximumVideoResolutionChoice.NoLimit => ApplicationMaximumVideoResolutionChoice.NoLimit,
            ServiceMaximumVideoResolutionChoice.UpTo2160p => ApplicationMaximumVideoResolutionChoice.UpTo2160p,
            ServiceMaximumVideoResolutionChoice.UpTo1440p => ApplicationMaximumVideoResolutionChoice.UpTo1440p,
            ServiceMaximumVideoResolutionChoice.UpTo1080p => ApplicationMaximumVideoResolutionChoice.UpTo1080p,
            ServiceMaximumVideoResolutionChoice.UpTo720p => ApplicationMaximumVideoResolutionChoice.UpTo720p,
            ServiceMaximumVideoResolutionChoice.UpTo480p => ApplicationMaximumVideoResolutionChoice.UpTo480p,
            ServiceMaximumVideoResolutionChoice.UpTo360p => ApplicationMaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No application MaximumVideoResolutionChoice matches this service MaximumVideoResolutionChoice."),
        };
    }

    private static ServiceMaximumVideoResolutionChoice? ConvertToServiceMaximumVideoResolutionChoiceOrNull(ApplicationMaximumVideoResolutionChoice? choice)
    {
        return choice switch
        {
            null => null,
            ApplicationMaximumVideoResolutionChoice.NoLimit => ServiceMaximumVideoResolutionChoice.NoLimit,
            ApplicationMaximumVideoResolutionChoice.UpTo2160p => ServiceMaximumVideoResolutionChoice.UpTo2160p,
            ApplicationMaximumVideoResolutionChoice.UpTo1440p => ServiceMaximumVideoResolutionChoice.UpTo1440p,
            ApplicationMaximumVideoResolutionChoice.UpTo1080p => ServiceMaximumVideoResolutionChoice.UpTo1080p,
            ApplicationMaximumVideoResolutionChoice.UpTo720p => ServiceMaximumVideoResolutionChoice.UpTo720p,
            ApplicationMaximumVideoResolutionChoice.UpTo480p => ServiceMaximumVideoResolutionChoice.UpTo480p,
            ApplicationMaximumVideoResolutionChoice.UpTo360p => ServiceMaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service MaximumVideoResolutionChoice matches this application MaximumVideoResolutionChoice."),
        };
    }

    private static ApplicationAudioQualityChoice? ConvertToApplicationAudioQualityChoiceOrNull(ServiceAudioQualityChoice? choice)
    {
        return choice switch
        {
            null => null,
            ServiceAudioQualityChoice.BestAac => ApplicationAudioQualityChoice.BestAac,
            ServiceAudioQualityChoice.BestOfAnyCodec => ApplicationAudioQualityChoice.BestOfAnyCodec,
            ServiceAudioQualityChoice.SmallestAac => ApplicationAudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No application AudioQualityChoice matches this service AudioQualityChoice."),
        };
    }

    private static ServiceAudioQualityChoice? ConvertToServiceAudioQualityChoiceOrNull(ApplicationAudioQualityChoice? choice)
    {
        return choice switch
        {
            null => null,
            ApplicationAudioQualityChoice.BestAac => ServiceAudioQualityChoice.BestAac,
            ApplicationAudioQualityChoice.BestOfAnyCodec => ServiceAudioQualityChoice.BestOfAnyCodec,
            ApplicationAudioQualityChoice.SmallestAac => ServiceAudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service AudioQualityChoice matches this application AudioQualityChoice."),
        };
    }

    public static ApplicationLogEntryType ConvertToApplicationLogEntryType(ServiceLogEntryType logEntryType)
    {
        return logEntryType switch
        {
            ServiceLogEntryType.Info => ApplicationLogEntryType.Info,
            ServiceLogEntryType.Warning => ApplicationLogEntryType.Warning,
            ServiceLogEntryType.Error => ApplicationLogEntryType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logEntryType), logEntryType, "No application log entry type matches this service log entry type."),
        };
    }

    public static ApplicationLogEntryDto ConvertLogEntryToApplicationDto(ServiceLogEntryDto logEntryDto)
    {
        return new ApplicationLogEntryDto
        {
            Id = logEntryDto.Id,
            Message = logEntryDto.Message,
            LogType = ConvertToApplicationLogEntryType(logEntryDto.LogType),
            CreatedAt = logEntryDto.CreatedAt,
            ExceptionText = logEntryDto.ExceptionText,
            VariablesAsIndentedJson = logEntryDto.VariablesAsIndentedJson,
        };
    }
}
