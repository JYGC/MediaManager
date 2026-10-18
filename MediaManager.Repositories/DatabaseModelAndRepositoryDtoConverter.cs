using System.Text.Json;
using System.Text.Json.Nodes;
using MediaManager.Dtos.Repositories;
using OPMF.Entities;

namespace MediaManager.Repositories;

public static class DatabaseModelAndRepositoryDtoConverter
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new() { WriteIndented = true };

    public static RepositoryThumbnailDto ConvertThumbnailToRepositoryDto(EntityThumbnail thumbnail)
    {
        return new RepositoryThumbnailDto
        {
            Url = thumbnail.Url,
            WidthInPixels = thumbnail.Width,
            HeightInPixels = thumbnail.Height,
        };
    }

    public static EntityThumbnail ConvertRepositoryDtoToThumbnail(RepositoryThumbnailDto thumbnailDto)
    {
        return new EntityThumbnail
        {
            Url = thumbnailDto.Url,
            Width = thumbnailDto.WidthInPixels,
            Height = thumbnailDto.HeightInPixels,
        };
    }

    public static RepositoryChannelDto ConvertChannelToRepositoryDto(Channel channel)
    {
        return new RepositoryChannelDto
        {
            SiteId = channel.SiteId,
            Url = channel.Url,
            Name = channel.Name,
            Thumbnail = ConvertThumbnailToRepositoryDto(channel.Thumbnail),
            Description = channel.Description,
            IsBlacklisted = channel.Blacklisted,
            IsAddedBySingleVideo = channel.IsAddedBySingleVideo,
            IsNotFoundOnVideoSite = channel.NotFound,
            LastCheckedForNewVideosAt = channel.LastCheckedOut,
            LatestVideoPublishedAt = channel.LastActivityDate,
            OriginalResponseFromVideoSite = channel.OriginalResponse,
        };
    }

    public static YoutubeChannel ConvertRepositoryDtoToYoutubeChannel(RepositoryChannelDto channelDto)
    {
        return new YoutubeChannel
        {
            SiteId = channelDto.SiteId,
            Url = channelDto.Url,
            Name = channelDto.Name,
            Thumbnail = ConvertRepositoryDtoToThumbnail(channelDto.Thumbnail),
            Description = channelDto.Description,
            Blacklisted = channelDto.IsBlacklisted,
            IsAddedBySingleVideo = channelDto.IsAddedBySingleVideo,
            NotFound = channelDto.IsNotFoundOnVideoSite,
            LastCheckedOut = channelDto.LastCheckedForNewVideosAt,
            LastActivityDate = channelDto.LatestVideoPublishedAt,
            OriginalResponse = channelDto.OriginalResponseFromVideoSite,
        };
    }

    public static RepositoryVideoMetadataDto ConvertVideoToRepositoryDto(Metadata video)
    {
        return new RepositoryVideoMetadataDto
        {
            SiteId = video.SiteId,
            Url = video.Url,
            Title = video.Title,
            Thumbnail = ConvertThumbnailToRepositoryDto(video.Thumbnail),
            Description = video.Description,
            DownloadStatus = ConvertToRepositoryVideoDownloadStatus(video.Status),
            IsBeingDownloaded = video.IsBeingDownloaded,
            PublishedAt = video.PublishedAt,
            Duration = video.Duration,
            ChannelSiteId = video.ChannelSiteId,
            OriginalResponseFromVideoSite = video.OriginalResponse,
        };
    }

    public static YoutubeMetadata ConvertRepositoryDtoToYoutubeMetadata(RepositoryVideoMetadataDto videoDto)
    {
        return new YoutubeMetadata
        {
            SiteId = videoDto.SiteId,
            Url = videoDto.Url,
            Title = videoDto.Title,
            Thumbnail = ConvertRepositoryDtoToThumbnail(videoDto.Thumbnail),
            Description = videoDto.Description,
            Status = ConvertToMetadataStatus(videoDto.DownloadStatus),
            IsBeingDownloaded = videoDto.IsBeingDownloaded,
            PublishedAt = videoDto.PublishedAt,
            Duration = videoDto.Duration,
            ChannelSiteId = videoDto.ChannelSiteId,
            OriginalResponse = videoDto.OriginalResponseFromVideoSite,
        };
    }

    public static RepositoryVideoDownloadStatus ConvertToRepositoryVideoDownloadStatus(MetadataStatus status)
    {
        return status switch
        {
            MetadataStatus.New => RepositoryVideoDownloadStatus.New,
            MetadataStatus.ToDownload => RepositoryVideoDownloadStatus.ToDownload,
            MetadataStatus.Wait => RepositoryVideoDownloadStatus.Wait,
            MetadataStatus.Ignore => RepositoryVideoDownloadStatus.Ignore,
            MetadataStatus.Downloaded => RepositoryVideoDownloadStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No repository video download status matches this metadata status."),
        };
    }

    public static MetadataStatus ConvertToMetadataStatus(RepositoryVideoDownloadStatus status)
    {
        return status switch
        {
            RepositoryVideoDownloadStatus.New => MetadataStatus.New,
            RepositoryVideoDownloadStatus.ToDownload => MetadataStatus.ToDownload,
            RepositoryVideoDownloadStatus.Wait => MetadataStatus.Wait,
            RepositoryVideoDownloadStatus.Ignore => MetadataStatus.Ignore,
            RepositoryVideoDownloadStatus.Downloaded => MetadataStatus.Downloaded,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No metadata status matches this repository video download status."),
        };
    }

    public static RepositoryLogEntryType ConvertToRepositoryLogEntryType(OPMFLogType logType)
    {
        return logType switch
        {
            OPMFLogType.Info => RepositoryLogEntryType.Info,
            OPMFLogType.Warning => RepositoryLogEntryType.Warning,
            OPMFLogType.Error => RepositoryLogEntryType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, "No repository log entry type matches this log type."),
        };
    }

    public static OPMFLogType ConvertToOPMFLogType(RepositoryLogEntryType logEntryType)
    {
        return logEntryType switch
        {
            RepositoryLogEntryType.Info => OPMFLogType.Info,
            RepositoryLogEntryType.Warning => OPMFLogType.Warning,
            RepositoryLogEntryType.Error => OPMFLogType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logEntryType), logEntryType, "No log type matches this repository log entry type."),
        };
    }

    public static RepositoryLogEntryDto ConvertLogToRepositoryDto(OPMFLog log)
    {
        return new RepositoryLogEntryDto
        {
            Id = log.Id,
            Message = log.Message,
            LogType = ConvertToRepositoryLogEntryType(log.Type),
            CreatedAt = log.DateCreated,
            ExceptionText = log.ExceptionObject,
            VariablesAsIndentedJson = ConvertVariablesToIndentedJson(log.Variables),
        };
    }

    public static OPMFError ConvertRepositoryDtoToOPMFError(RepositoryErrorLogEntryToRecordDto errorToRecord, DateTime dateCreated)
    {
        return new OPMFError
        {
            Message = errorToRecord.Message,
            Type = OPMFLogType.Error,
            ExceptionObject = errorToRecord.ExceptionText,
            DateCreated = dateCreated,
            Variables = errorToRecord.VariablesAsJsonText.ToDictionary(variable => variable.Key, variable => (object)variable.Value),
        };
    }

    private static string ConvertVariablesToIndentedJson(Dictionary<string, object>? variables)
    {
        if (variables == null)
        {
            return JsonSerializer.Serialize<object?>(null, IndentedJsonOptions);
        }
        var variablesWithJsonTextEmbeddedAsJson = variables.ToDictionary(variable => variable.Key, variable => EmbedValueAsJsonWhenItIsTextOfAJsonObjectOrArray(variable.Value));
        return JsonSerializer.Serialize(variablesWithJsonTextEmbeddedAsJson, IndentedJsonOptions);
    }

    private static object? EmbedValueAsJsonWhenItIsTextOfAJsonObjectOrArray(object? value)
    {
        if (value is not string text)
        {
            return value;
        }
        try
        {
            JsonNode? parsedText = JsonNode.Parse(text);
            return parsedText is JsonObject or JsonArray ? parsedText : value;
        }
        catch (JsonException)
        {
            return value;
        }
    }
}
