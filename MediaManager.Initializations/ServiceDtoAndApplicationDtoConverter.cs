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
        };
    }

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
