using System;
using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;
using MediaManager.Services2;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class RepositoryDtoAndServiceDtoConverterTests
    {
        private static readonly DateTime SomeDate = new DateTime(2026, 9, 1, 10, 30, 0);
        private static readonly DateTime SomeLaterDate = new DateTime(2026, 9, 2, 11, 45, 0);

        private static RepositoryChannelDto RepositoryChannelWithEveryMemberSet() => new RepositoryChannelDto
        {
            SiteId = "channel-1",
            Url = "https://www.youtube.com/channel/channel-1",
            Name = "The channel",
            Thumbnail = new RepositoryThumbnailDto { Url = "https://thumbnails/channel", WidthInPixels = 120, HeightInPixels = 90 },
            Description = "About the channel",
            IsBlacklisted = true,
            IsAddedBySingleVideo = true,
            IsNotFoundOnVideoSite = true,
            LastCheckedForNewVideosAt = SomeDate,
            LatestVideoPublishedAt = SomeLaterDate,
            OriginalResponseFromVideoSite = new object(),
        };

        private static RepositoryVideoMetadataDto RepositoryVideoWithEveryMemberSet() => new RepositoryVideoMetadataDto
        {
            SiteId = "video-1",
            Url = "https://www.youtube.com/watch?v=video-1",
            Title = "The video",
            Thumbnail = new RepositoryThumbnailDto { Url = "https://thumbnails/video", WidthInPixels = 320, HeightInPixels = 180 },
            Description = "About the video",
            DownloadStatus = RepositoryVideoDownloadStatus.Wait,
            IsBeingDownloaded = true,
            PublishedAt = SomeDate,
            Duration = new TimeSpan(1, 2, 3),
            ChannelSiteId = "channel-1",
            OriginalResponseFromVideoSite = new object(),
        };

        [Theory]
        [InlineData(RepositoryVideoCodecChoice.H264, ServiceVideoCodecChoice.H264)]
        [InlineData(RepositoryVideoCodecChoice.AnyCodec, ServiceVideoCodecChoice.AnyCodec)]
        public void EveryVideoCodecChoiceOverrideRoundTripsBetweenTheRepositoryAndTheServiceDto(RepositoryVideoCodecChoice repositoryChoice, ServiceVideoCodecChoice expectedServiceChoice)
        {
            var videoDto = new RepositoryVideoMetadataDto { SiteId = "v", VideoCodecChoiceOverride = repositoryChoice };

            var serviceVideo = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(videoDto);
            var videoConvertedBack = RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo);

            Assert.Equal(expectedServiceChoice, serviceVideo.VideoCodecChoiceOverride);
            Assert.Equal(repositoryChoice, videoConvertedBack.VideoCodecChoiceOverride);
        }

        [Theory]
        [InlineData(RepositoryMaximumVideoResolutionChoice.NoLimit, ServiceMaximumVideoResolutionChoice.NoLimit)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo2160p, ServiceMaximumVideoResolutionChoice.UpTo2160p)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo1440p, ServiceMaximumVideoResolutionChoice.UpTo1440p)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo1080p, ServiceMaximumVideoResolutionChoice.UpTo1080p)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo720p, ServiceMaximumVideoResolutionChoice.UpTo720p)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo480p, ServiceMaximumVideoResolutionChoice.UpTo480p)]
        [InlineData(RepositoryMaximumVideoResolutionChoice.UpTo360p, ServiceMaximumVideoResolutionChoice.UpTo360p)]
        public void EveryMaximumVideoResolutionChoiceOverrideRoundTripsBetweenTheRepositoryAndTheServiceDto(RepositoryMaximumVideoResolutionChoice repositoryChoice, ServiceMaximumVideoResolutionChoice expectedServiceChoice)
        {
            var videoDto = new RepositoryVideoMetadataDto { SiteId = "v", MaximumVideoResolutionChoiceOverride = repositoryChoice };

            var serviceVideo = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(videoDto);
            var videoConvertedBack = RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo);

            Assert.Equal(expectedServiceChoice, serviceVideo.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(repositoryChoice, videoConvertedBack.MaximumVideoResolutionChoiceOverride);
        }

        [Theory]
        [InlineData(RepositoryAudioQualityChoice.BestAac, ServiceAudioQualityChoice.BestAac)]
        [InlineData(RepositoryAudioQualityChoice.BestOfAnyCodec, ServiceAudioQualityChoice.BestOfAnyCodec)]
        [InlineData(RepositoryAudioQualityChoice.SmallestAac, ServiceAudioQualityChoice.SmallestAac)]
        public void EveryAudioQualityChoiceOverrideRoundTripsBetweenTheRepositoryAndTheServiceDto(RepositoryAudioQualityChoice repositoryChoice, ServiceAudioQualityChoice expectedServiceChoice)
        {
            var videoDto = new RepositoryVideoMetadataDto { SiteId = "v", AudioQualityChoiceOverride = repositoryChoice };

            var serviceVideo = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(videoDto);
            var videoConvertedBack = RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo);

            Assert.Equal(expectedServiceChoice, serviceVideo.AudioQualityChoiceOverride);
            Assert.Equal(repositoryChoice, videoConvertedBack.AudioQualityChoiceOverride);
        }

        [Fact]
        public void VideoOverridesThatAreNullStayNullInBothDirections()
        {
            var serviceVideo = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(new RepositoryVideoMetadataDto { SiteId = "v" });
            var videoConvertedBack = RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo);

            Assert.Null(serviceVideo.VideoCodecChoiceOverride);
            Assert.Null(serviceVideo.MaximumVideoResolutionChoiceOverride);
            Assert.Null(serviceVideo.AudioQualityChoiceOverride);
            Assert.Null(videoConvertedBack.VideoCodecChoiceOverride);
            Assert.Null(videoConvertedBack.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoConvertedBack.AudioQualityChoiceOverride);
        }

        [Fact]
        public void AServiceAudioQualityChoiceWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            var serviceVideo = new ServiceVideoMetadataDto { SiteId = "v", AudioQualityChoiceOverride = (ServiceAudioQualityChoice)99 };

            Assert.Throws<ArgumentOutOfRangeException>(() => RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo));
        }

        [Fact]
        public void ARepositoryChannelIsConvertedToAServiceChannelWithEveryMemberMapped()
        {
            var channelDto = RepositoryDtoAndServiceDtoConverter.ConvertChannelToServiceDto(RepositoryChannelWithEveryMemberSet());

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
        public void AServiceChannelIsConvertedToARepositoryChannelWithEveryMemberMappedAndNoOriginalResponse()
        {
            var serviceChannel = RepositoryDtoAndServiceDtoConverter.ConvertChannelToServiceDto(RepositoryChannelWithEveryMemberSet());

            var channelDto = RepositoryDtoAndServiceDtoConverter.ConvertChannelToRepositoryDto(serviceChannel);

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
            Assert.Null(channelDto.OriginalResponseFromVideoSite);
        }

        [Fact]
        public void ARepositoryVideoIsConvertedToAServiceVideoWithEveryMemberMapped()
        {
            var videoDto = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(RepositoryVideoWithEveryMemberSet());

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
        public void AServiceVideoIsConvertedToARepositoryVideoWithEveryMemberMappedAndNoOriginalResponse()
        {
            var serviceVideo = RepositoryDtoAndServiceDtoConverter.ConvertVideoToServiceDto(RepositoryVideoWithEveryMemberSet());

            var videoDto = RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(serviceVideo);

            Assert.Equal("video-1", videoDto.SiteId);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", videoDto.Url);
            Assert.Equal("The video", videoDto.Title);
            Assert.Equal("https://thumbnails/video", videoDto.Thumbnail.Url);
            Assert.Equal(320, videoDto.Thumbnail.WidthInPixels);
            Assert.Equal(180, videoDto.Thumbnail.HeightInPixels);
            Assert.Equal("About the video", videoDto.Description);
            Assert.Equal(RepositoryVideoDownloadStatus.Wait, videoDto.DownloadStatus);
            Assert.True(videoDto.IsBeingDownloaded);
            Assert.Equal(SomeDate, videoDto.PublishedAt);
            Assert.Equal(new TimeSpan(1, 2, 3), videoDto.Duration);
            Assert.Equal("channel-1", videoDto.ChannelSiteId);
            Assert.Null(videoDto.OriginalResponseFromVideoSite);
        }

        [Fact]
        public void EveryVideoDownloadStatusRoundTripsKeepingItsName()
        {
            foreach (var repositoryStatus in Enum.GetValues<RepositoryVideoDownloadStatus>())
            {
                var serviceStatus = RepositoryDtoAndServiceDtoConverter.ConvertToServiceVideoDownloadStatus(repositoryStatus);

                Assert.Equal(repositoryStatus.ToString(), serviceStatus.ToString());
                Assert.Equal(repositoryStatus, RepositoryDtoAndServiceDtoConverter.ConvertToRepositoryVideoDownloadStatus(serviceStatus));
            }
        }

        [Fact]
        public void AVideoDownloadStatusWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RepositoryDtoAndServiceDtoConverter.ConvertToServiceVideoDownloadStatus((RepositoryVideoDownloadStatus)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => RepositoryDtoAndServiceDtoConverter.ConvertToRepositoryVideoDownloadStatus((ServiceVideoDownloadStatus)99));
        }

        [Fact]
        public void EveryLogEntryTypeIsConvertedKeepingItsName()
        {
            foreach (var repositoryLogEntryType in Enum.GetValues<RepositoryLogEntryType>())
            {
                Assert.Equal(repositoryLogEntryType.ToString(), RepositoryDtoAndServiceDtoConverter.ConvertToServiceLogEntryType(repositoryLogEntryType).ToString());
            }
        }

        [Fact]
        public void ALogEntryTypeWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RepositoryDtoAndServiceDtoConverter.ConvertToServiceLogEntryType((RepositoryLogEntryType)99));
        }

        [Fact]
        public void ARepositoryLogEntryIsConvertedToAServiceLogEntryWithEveryMemberMapped()
        {
            var repositoryLogEntry = new RepositoryLogEntryDto
            {
                Id = "log-1",
                Message = "something happened",
                LogType = RepositoryLogEntryType.Warning,
                CreatedAt = SomeDate,
                ExceptionText = "the exception text",
                VariablesAsIndentedJson = "{\n  \"key\": \"value\"\n}",
            };

            var logEntry = RepositoryDtoAndServiceDtoConverter.ConvertLogEntryToServiceDto(repositoryLogEntry);

            Assert.Equal("log-1", logEntry.Id);
            Assert.Equal("something happened", logEntry.Message);
            Assert.Equal(ServiceLogEntryType.Warning, logEntry.LogType);
            Assert.Equal(SomeDate, logEntry.CreatedAt);
            Assert.Equal("the exception text", logEntry.ExceptionText);
            Assert.Equal("{\n  \"key\": \"value\"\n}", logEntry.VariablesAsIndentedJson);
        }

        [Fact]
        public void AVideoWithItsChannelIsConvertedToAServiceVideoWithItsChannel()
        {
            var videoWithChannel = RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(RepositoryVideoWithEveryMemberSet(), RepositoryChannelWithEveryMemberSet());

            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Equal("The channel", videoWithChannel.Channel.Name);
        }

        [Fact]
        public void AVideoWithNoChannelIsConvertedToAServiceVideoWithANullChannel()
        {
            var videoWithChannel = RepositoryDtoAndServiceDtoConverter.ConvertVideoWithChannelToServiceDto(RepositoryVideoWithEveryMemberSet(), null);

            Assert.Equal("The video", videoWithChannel.Video.Title);
            Assert.Null(videoWithChannel.Channel);
        }
    }
}
