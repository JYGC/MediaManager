using System;
using MediaManager.Dtos.Application;
using MediaManager.Dtos.Services;
using MediaManager.Initializations;
using Xunit;

namespace OPMF.UnitTests.Initializations
{
    public class ServiceDtoAndApplicationDtoConverterTests
    {
        private static readonly DateTime SomeDate = new DateTime(2026, 9, 1, 10, 30, 0);
        private static readonly DateTime SomeLaterDate = new DateTime(2026, 9, 2, 11, 45, 0);

        private static ServiceChannelDto ServiceChannelWithEveryMemberSet() => new ServiceChannelDto
        {
            SiteId = "channel-1",
            Url = "https://www.youtube.com/channel/channel-1",
            Name = "The channel",
            Thumbnail = new ServiceThumbnailDto { Url = "https://thumbnails/channel", WidthInPixels = 120, HeightInPixels = 90 },
            Description = "About the channel",
            IsBlacklisted = true,
            IsAddedBySingleVideo = true,
            IsNotFoundOnVideoSite = true,
            LastCheckedForNewVideosAt = SomeDate,
            LatestVideoPublishedAt = SomeLaterDate,
        };

        private static ServiceVideoMetadataDto ServiceVideoWithEveryMemberSet() => new ServiceVideoMetadataDto
        {
            SiteId = "video-1",
            Url = "https://www.youtube.com/watch?v=video-1",
            Title = "The video",
            Thumbnail = new ServiceThumbnailDto { Url = "https://thumbnails/video", WidthInPixels = 320, HeightInPixels = 180 },
            Description = "About the video",
            DownloadStatus = ServiceVideoDownloadStatus.Wait,
            IsBeingDownloaded = true,
            PublishedAt = SomeDate,
            Duration = new TimeSpan(1, 2, 3),
            ChannelSiteId = "channel-1",
        };

        [Fact]
        public void AServiceChannelIsConvertedToAnApplicationChannelWithEveryMemberMapped()
        {
            var channelDto = ServiceDtoAndApplicationDtoConverter.ConvertChannelToApplicationDto(ServiceChannelWithEveryMemberSet());

            Assert.Equal("channel-1", channelDto.SiteId);
            Assert.Equal("https://www.youtube.com/channel/channel-1", channelDto.Url);
            Assert.Equal("The channel", channelDto.Name);
            Assert.Equal("https://thumbnails/channel", channelDto.Thumbnail.Url);
            Assert.Equal(120, channelDto.Thumbnail.WidthInPixels);
            Assert.Equal(90, channelDto.Thumbnail.HeightInPixels);
            Assert.Equal("About the channel", channelDto.Description);
            Assert.True(channelDto.IsBlacklisted);
            Assert.True(channelDto.IsAddedBySingleVideo);
            Assert.True(channelDto.IsNotFoundOnVideoSite);
            Assert.Equal(SomeDate, channelDto.LastCheckedForNewVideosAt);
            Assert.Equal(SomeLaterDate, channelDto.LatestVideoPublishedAt);
        }

        [Fact]
        public void AnApplicationChannelIsConvertedBackToAServiceChannelWithEveryMemberMapped()
        {
            var applicationChannel = ServiceDtoAndApplicationDtoConverter.ConvertChannelToApplicationDto(ServiceChannelWithEveryMemberSet());

            var channelDto = ServiceDtoAndApplicationDtoConverter.ConvertChannelToServiceDto(applicationChannel);

            Assert.Equal("channel-1", channelDto.SiteId);
            Assert.Equal("https://www.youtube.com/channel/channel-1", channelDto.Url);
            Assert.Equal("The channel", channelDto.Name);
            Assert.Equal("https://thumbnails/channel", channelDto.Thumbnail.Url);
            Assert.Equal(120, channelDto.Thumbnail.WidthInPixels);
            Assert.Equal(90, channelDto.Thumbnail.HeightInPixels);
            Assert.Equal("About the channel", channelDto.Description);
            Assert.True(channelDto.IsBlacklisted);
            Assert.True(channelDto.IsAddedBySingleVideo);
            Assert.True(channelDto.IsNotFoundOnVideoSite);
            Assert.Equal(SomeDate, channelDto.LastCheckedForNewVideosAt);
            Assert.Equal(SomeLaterDate, channelDto.LatestVideoPublishedAt);
        }

        [Fact]
        public void AServiceVideoIsConvertedToAnApplicationVideoWithEveryMemberMapped()
        {
            var videoDto = ServiceDtoAndApplicationDtoConverter.ConvertVideoToApplicationDto(ServiceVideoWithEveryMemberSet());

            Assert.Equal("video-1", videoDto.SiteId);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", videoDto.Url);
            Assert.Equal("The video", videoDto.Title);
            Assert.Equal("https://thumbnails/video", videoDto.Thumbnail.Url);
            Assert.Equal(320, videoDto.Thumbnail.WidthInPixels);
            Assert.Equal(180, videoDto.Thumbnail.HeightInPixels);
            Assert.Equal("About the video", videoDto.Description);
            Assert.Equal(ApplicationVideoDownloadStatus.Wait, videoDto.DownloadStatus);
            Assert.True(videoDto.IsBeingDownloaded);
            Assert.Equal(SomeDate, videoDto.PublishedAt);
            Assert.Equal(new TimeSpan(1, 2, 3), videoDto.Duration);
            Assert.Equal("channel-1", videoDto.ChannelSiteId);
        }

        [Fact]
        public void AnApplicationVideoIsConvertedBackToAServiceVideoWithEveryMemberMapped()
        {
            var applicationVideo = ServiceDtoAndApplicationDtoConverter.ConvertVideoToApplicationDto(ServiceVideoWithEveryMemberSet());

            var videoDto = ServiceDtoAndApplicationDtoConverter.ConvertVideoToServiceDto(applicationVideo);

            Assert.Equal("video-1", videoDto.SiteId);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", videoDto.Url);
            Assert.Equal("The video", videoDto.Title);
            Assert.Equal("https://thumbnails/video", videoDto.Thumbnail.Url);
            Assert.Equal(320, videoDto.Thumbnail.WidthInPixels);
            Assert.Equal(180, videoDto.Thumbnail.HeightInPixels);
            Assert.Equal("About the video", videoDto.Description);
            Assert.Equal(ServiceVideoDownloadStatus.Wait, videoDto.DownloadStatus);
            Assert.True(videoDto.IsBeingDownloaded);
            Assert.Equal(SomeDate, videoDto.PublishedAt);
            Assert.Equal(new TimeSpan(1, 2, 3), videoDto.Duration);
            Assert.Equal("channel-1", videoDto.ChannelSiteId);
        }

        [Fact]
        public void EveryVideoDownloadStatusRoundTripsKeepingItsName()
        {
            foreach (var serviceStatus in Enum.GetValues<ServiceVideoDownloadStatus>())
            {
                var applicationStatus = ServiceDtoAndApplicationDtoConverter.ConvertToApplicationVideoDownloadStatus(serviceStatus);

                Assert.Equal(serviceStatus.ToString(), applicationStatus.ToString());
                Assert.Equal(serviceStatus, ServiceDtoAndApplicationDtoConverter.ConvertToServiceVideoDownloadStatus(applicationStatus));
            }
        }

        [Fact]
        public void AVideoDownloadStatusWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ServiceDtoAndApplicationDtoConverter.ConvertToApplicationVideoDownloadStatus((ServiceVideoDownloadStatus)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => ServiceDtoAndApplicationDtoConverter.ConvertToServiceVideoDownloadStatus((ApplicationVideoDownloadStatus)99));
        }

        [Fact]
        public void EveryLogEntryTypeIsConvertedKeepingItsName()
        {
            foreach (var serviceLogEntryType in Enum.GetValues<ServiceLogEntryType>())
            {
                Assert.Equal(serviceLogEntryType.ToString(), ServiceDtoAndApplicationDtoConverter.ConvertToApplicationLogEntryType(serviceLogEntryType).ToString());
            }
        }

        [Fact]
        public void ALogEntryTypeWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ServiceDtoAndApplicationDtoConverter.ConvertToApplicationLogEntryType((ServiceLogEntryType)99));
        }

        [Fact]
        public void AServiceLogEntryIsConvertedToAnApplicationLogEntryWithEveryMemberMapped()
        {
            var serviceLogEntry = new ServiceLogEntryDto
            {
                Id = "log-1",
                Message = "something happened",
                LogType = ServiceLogEntryType.Error,
                CreatedAt = SomeDate,
                ExceptionText = "the exception text",
                VariablesAsIndentedJson = "{\n  \"key\": \"value\"\n}",
            };

            var logEntry = ServiceDtoAndApplicationDtoConverter.ConvertLogEntryToApplicationDto(serviceLogEntry);

            Assert.Equal("log-1", logEntry.Id);
            Assert.Equal("something happened", logEntry.Message);
            Assert.Equal(ApplicationLogEntryType.Error, logEntry.LogType);
            Assert.Equal(SomeDate, logEntry.CreatedAt);
            Assert.Equal("the exception text", logEntry.ExceptionText);
            Assert.Equal("{\n  \"key\": \"value\"\n}", logEntry.VariablesAsIndentedJson);
        }

        [Fact]
        public void AServiceVideoWithItsChannelIsConvertedToAnApplicationVideoWithItsChannel()
        {
            var serviceVideoWithChannel = new ServiceVideoWithChannelDto { Video = ServiceVideoWithEveryMemberSet(), Channel = ServiceChannelWithEveryMemberSet() };

            var videoWithChannel = ServiceDtoAndApplicationDtoConverter.ConvertVideoWithChannelToApplicationDto(serviceVideoWithChannel);

            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Equal("The channel", videoWithChannel.Channel.Name);
        }

        [Fact]
        public void AServiceVideoWithNoChannelIsConvertedToAnApplicationVideoWithANullChannel()
        {
            var serviceVideoWithChannel = new ServiceVideoWithChannelDto { Video = ServiceVideoWithEveryMemberSet(), Channel = null };

            var videoWithChannel = ServiceDtoAndApplicationDtoConverter.ConvertVideoWithChannelToApplicationDto(serviceVideoWithChannel);

            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Null(videoWithChannel.Channel);
        }
    }
}
