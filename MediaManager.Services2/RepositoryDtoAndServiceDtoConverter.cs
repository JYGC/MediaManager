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
