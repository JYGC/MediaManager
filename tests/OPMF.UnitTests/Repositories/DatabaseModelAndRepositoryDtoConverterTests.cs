using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MediaManager.Dtos.Repositories;
using MediaManager.Repositories;
using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Repositories
{
    public class DatabaseModelAndRepositoryDtoConverterTests
    {
        private static readonly DateTime SomeDate = new DateTime(2026, 9, 1, 10, 30, 0);
        private static readonly DateTime SomeLaterDate = new DateTime(2026, 9, 2, 11, 45, 0);

        private static YoutubeChannel ChannelWithEveryMemberSet(object originalResponse = null)
        {
            return new YoutubeChannel
            {
                SiteId = "channel-1",
                Url = "https://www.youtube.com/channel/channel-1",
                Name = "The channel",
                Thumbnail = new EntityThumbnail { Url = "https://thumbnails/channel", Width = 120, Height = 90 },
                Description = "About the channel",
                Blacklisted = true,
                IsAddedBySingleVideo = true,
                NotFound = true,
                LastCheckedOut = SomeDate,
                LastActivityDate = SomeLaterDate,
                OriginalResponse = originalResponse,
            };
        }

        private static YoutubeMetadata VideoWithEveryMemberSet(object originalResponse = null)
        {
            return new YoutubeMetadata
            {
                SiteId = "video-1",
                Url = "https://www.youtube.com/watch?v=video-1",
                Title = "The video",
                Thumbnail = new EntityThumbnail { Url = "https://thumbnails/video", Width = 320, Height = 180 },
                Description = "About the video",
                Status = MetadataStatus.Wait,
                IsBeingDownloaded = true,
                PublishedAt = SomeDate,
                Duration = new TimeSpan(1, 2, 3),
                ChannelSiteId = "channel-1",
                OriginalResponse = originalResponse,
            };
        }

        [Fact]
        public void AChannelIsConvertedToARepositoryDtoWithEveryMemberMapped()
        {
            var originalResponse = new object();

            var channelDto = DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto(ChannelWithEveryMemberSet(originalResponse));

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
            Assert.Same(originalResponse, channelDto.OriginalResponseFromVideoSite);
        }

        [Fact]
        public void ARepositoryChannelDtoIsConvertedToAYoutubeChannelWithEveryMemberMapped()
        {
            var originalResponse = new object();
            var channelDto = DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto(ChannelWithEveryMemberSet(originalResponse));

            var channel = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeChannel(channelDto);

            Assert.Equal("channel-1", channel.SiteId);
            Assert.Equal("channel-1", channel.Id);
            Assert.Equal("https://www.youtube.com/channel/channel-1", channel.Url);
            Assert.Equal("The channel", channel.Name);
            Assert.Equal("https://thumbnails/channel", channel.Thumbnail.Url);
            Assert.Equal(120, channel.Thumbnail.Width);
            Assert.Equal(90, channel.Thumbnail.Height);
            Assert.Equal("About the channel", channel.Description);
            Assert.True(channel.Blacklisted);
            Assert.True(channel.IsAddedBySingleVideo);
            Assert.True(channel.NotFound);
            Assert.Equal(SomeDate, channel.LastCheckedOut);
            Assert.Equal(SomeLaterDate, channel.LastActivityDate);
            Assert.Same(originalResponse, channel.OriginalResponse);
        }

        [Fact]
        public void ARepositoryChannelDtoIsAlwaysConvertedToTheYoutubeChannelType()
        {
            var channel = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeChannel(new RepositoryChannelDto { SiteId = "channel-1" });

            Assert.Equal(typeof(YoutubeChannel), channel.GetType());
        }

        [Fact]
        public void AChannelWithNoDescriptionAndNoDatesKeepsThemNull()
        {
            var channelDto = DatabaseModelAndRepositoryDtoConverter.ConvertChannelToRepositoryDto(new YoutubeChannel { SiteId = "channel-1", Thumbnail = new EntityThumbnail() });

            var channel = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeChannel(channelDto);

            Assert.Null(channel.Description);
            Assert.Null(channel.LastCheckedOut);
            Assert.Null(channel.LastActivityDate);
        }

        [Fact]
        public void AVideoIsConvertedToARepositoryDtoWithEveryMemberMapped()
        {
            var originalResponse = new object();

            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(VideoWithEveryMemberSet(originalResponse));

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
            Assert.Same(originalResponse, videoDto.OriginalResponseFromVideoSite);
        }

        [Fact]
        public void ARepositoryVideoDtoIsConvertedToAYoutubeMetadataWithEveryMemberMapped()
        {
            var originalResponse = new object();
            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(VideoWithEveryMemberSet(originalResponse));

            var video = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto);

            Assert.Equal("video-1", video.SiteId);
            Assert.Equal("video-1", video.Id);
            Assert.Equal("https://www.youtube.com/watch?v=video-1", video.Url);
            Assert.Equal("The video", video.Title);
            Assert.Equal("https://thumbnails/video", video.Thumbnail.Url);
            Assert.Equal(320, video.Thumbnail.Width);
            Assert.Equal(180, video.Thumbnail.Height);
            Assert.Equal("About the video", video.Description);
            Assert.Equal(MetadataStatus.Wait, video.Status);
            Assert.True(video.IsBeingDownloaded);
            Assert.Equal(SomeDate, video.PublishedAt);
            Assert.Equal(new TimeSpan(1, 2, 3), video.Duration);
            Assert.Equal("channel-1", video.ChannelSiteId);
            Assert.Same(originalResponse, video.OriginalResponse);
        }

        [Fact]
        public void ARepositoryVideoDtoIsAlwaysConvertedToTheYoutubeMetadataType()
        {
            var video = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(new RepositoryVideoMetadataDto { SiteId = "video-1" });

            Assert.Equal(typeof(YoutubeMetadata), video.GetType());
        }

        [Theory]
        [InlineData(EntityVideoCodecChoice.H264, RepositoryVideoCodecChoice.H264)]
        [InlineData(EntityVideoCodecChoice.AnyCodec, RepositoryVideoCodecChoice.AnyCodec)]
        public void EveryVideoCodecChoiceOverrideRoundTripsBetweenTheVideoAndTheRepositoryDto(EntityVideoCodecChoice entityChoice, RepositoryVideoCodecChoice expectedDtoChoice)
        {
            var video = new YoutubeMetadata { SiteId = "v", VideoCodecChoiceOverride = entityChoice };

            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(video);
            var videoConvertedBack = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto);

            Assert.Equal(expectedDtoChoice, videoDto.VideoCodecChoiceOverride);
            Assert.Equal(entityChoice, videoConvertedBack.VideoCodecChoiceOverride);
        }

        [Theory]
        [InlineData(EntityMaximumVideoResolutionChoice.NoLimit, RepositoryMaximumVideoResolutionChoice.NoLimit)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo2160p, RepositoryMaximumVideoResolutionChoice.UpTo2160p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo1440p, RepositoryMaximumVideoResolutionChoice.UpTo1440p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo1080p, RepositoryMaximumVideoResolutionChoice.UpTo1080p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo720p, RepositoryMaximumVideoResolutionChoice.UpTo720p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo480p, RepositoryMaximumVideoResolutionChoice.UpTo480p)]
        [InlineData(EntityMaximumVideoResolutionChoice.UpTo360p, RepositoryMaximumVideoResolutionChoice.UpTo360p)]
        public void EveryMaximumVideoResolutionChoiceOverrideRoundTripsBetweenTheVideoAndTheRepositoryDto(EntityMaximumVideoResolutionChoice entityChoice, RepositoryMaximumVideoResolutionChoice expectedDtoChoice)
        {
            var video = new YoutubeMetadata { SiteId = "v", MaximumVideoResolutionChoiceOverride = entityChoice };

            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(video);
            var videoConvertedBack = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto);

            Assert.Equal(expectedDtoChoice, videoDto.MaximumVideoResolutionChoiceOverride);
            Assert.Equal(entityChoice, videoConvertedBack.MaximumVideoResolutionChoiceOverride);
        }

        [Theory]
        [InlineData(EntityAudioQualityChoice.BestAac, RepositoryAudioQualityChoice.BestAac)]
        [InlineData(EntityAudioQualityChoice.BestOfAnyCodec, RepositoryAudioQualityChoice.BestOfAnyCodec)]
        [InlineData(EntityAudioQualityChoice.SmallestAac, RepositoryAudioQualityChoice.SmallestAac)]
        public void EveryAudioQualityChoiceOverrideRoundTripsBetweenTheVideoAndTheRepositoryDto(EntityAudioQualityChoice entityChoice, RepositoryAudioQualityChoice expectedDtoChoice)
        {
            var video = new YoutubeMetadata { SiteId = "v", AudioQualityChoiceOverride = entityChoice };

            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(video);
            var videoConvertedBack = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto);

            Assert.Equal(expectedDtoChoice, videoDto.AudioQualityChoiceOverride);
            Assert.Equal(entityChoice, videoConvertedBack.AudioQualityChoiceOverride);
        }

        [Fact]
        public void VideoOverridesThatAreNullStayNullInBothDirections()
        {
            var videoDto = DatabaseModelAndRepositoryDtoConverter.ConvertVideoToRepositoryDto(new YoutubeMetadata { SiteId = "v" });
            var videoConvertedBack = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto);

            Assert.Null(videoDto.VideoCodecChoiceOverride);
            Assert.Null(videoDto.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoDto.AudioQualityChoiceOverride);
            Assert.Null(videoConvertedBack.VideoCodecChoiceOverride);
            Assert.Null(videoConvertedBack.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoConvertedBack.AudioQualityChoiceOverride);
        }

        [Fact]
        public void ARepositoryVideoCodecChoiceWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            var videoDto = new RepositoryVideoMetadataDto { SiteId = "v", VideoCodecChoiceOverride = (RepositoryVideoCodecChoice)99 };

            Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToYoutubeMetadata(videoDto));
        }

        [Fact]
        public void EveryVideoDownloadStatusRoundTripsThroughTheRepositoryEnum()
        {
            foreach (var status in Enum.GetValues<MetadataStatus>())
            {
                var statusAfterTheRoundTrip = DatabaseModelAndRepositoryDtoConverter.ConvertToMetadataStatus(DatabaseModelAndRepositoryDtoConverter.ConvertToRepositoryVideoDownloadStatus(status));

                Assert.Equal(status, statusAfterTheRoundTrip);
            }
        }

        [Fact]
        public void TheRepositoryVideoDownloadStatusMembersHaveTheSameNamesAsTheDatabaseModelMembers()
        {
            foreach (var status in Enum.GetValues<MetadataStatus>())
            {
                Assert.Equal(status.ToString(), DatabaseModelAndRepositoryDtoConverter.ConvertToRepositoryVideoDownloadStatus(status).ToString());
            }
        }

        [Fact]
        public void AVideoDownloadStatusWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseModelAndRepositoryDtoConverter.ConvertToRepositoryVideoDownloadStatus((MetadataStatus)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseModelAndRepositoryDtoConverter.ConvertToMetadataStatus((RepositoryVideoDownloadStatus)99));
        }

        [Fact]
        public void EveryLogTypeRoundTripsThroughTheRepositoryEnumKeepingItsName()
        {
            foreach (var logType in Enum.GetValues<OPMFLogType>())
            {
                var repositoryLogEntryType = DatabaseModelAndRepositoryDtoConverter.ConvertToRepositoryLogEntryType(logType);

                Assert.Equal(logType.ToString(), repositoryLogEntryType.ToString());
                Assert.Equal(logType, DatabaseModelAndRepositoryDtoConverter.ConvertToOPMFLogType(repositoryLogEntryType));
            }
        }

        [Fact]
        public void ALogTypeWithNoMappingThrowsAnArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseModelAndRepositoryDtoConverter.ConvertToRepositoryLogEntryType((OPMFLogType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseModelAndRepositoryDtoConverter.ConvertToOPMFLogType((RepositoryLogEntryType)99));
        }

        [Fact]
        public void ALogEntryIsConvertedToARepositoryDtoWithEveryMemberMapped()
        {
            var log = new OPMFLog
            {
                Id = "log-1",
                Message = "something happened",
                Type = OPMFLogType.Warning,
                DateCreated = SomeDate,
                ExceptionObject = "the exception text",
                Variables = new Dictionary<string, object> { { "key", "value" } },
            };

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            Assert.Equal("log-1", logDto.Id);
            Assert.Equal("something happened", logDto.Message);
            Assert.Equal(RepositoryLogEntryType.Warning, logDto.LogType);
            Assert.Equal(SomeDate, logDto.CreatedAt);
            Assert.Equal("the exception text", logDto.ExceptionText);
        }

        [Fact]
        public void LogVariablesWithOldObjectValuesBecomeTheSameIndentedJsonTheUiRendersToday()
        {
            var log = new OPMFLog
            {
                Variables = new Dictionary<string, object>
                {
                    { "metadatas", new List<Metadata> { VideoWithEveryMemberSet() } },
                    { "metadata.Title", "The video" },
                },
            };
            var indentedJsonTheUiRendersToday = JsonSerializer.Serialize(log.Variables, new JsonSerializerOptions { WriteIndented = true });

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            Assert.Equal(indentedJsonTheUiRendersToday, logDto.VariablesAsIndentedJson);
        }

        [Fact]
        public void LogVariablesWithJsonTextValuesAreEmbeddedAsJson()
        {
            var log = new OPMFLog
            {
                Variables = new Dictionary<string, object>
                {
                    { "channels", "[{\"SiteId\":\"channel-1\"}]" },
                    { "metadatas", "{\"count\":2}" },
                },
            };

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            var variablesJson = JsonNode.Parse(logDto.VariablesAsIndentedJson).AsObject();
            Assert.Equal("channel-1", (string)variablesJson["channels"].AsArray()[0]["SiteId"]);
            Assert.Equal(2, (int)variablesJson["metadatas"]["count"]);
        }

        [Fact]
        public void LogVariablesWithPlainTextValuesStayText()
        {
            var log = new OPMFLog
            {
                Variables = new Dictionary<string, object>
                {
                    { "metadata.Title", "The video" },
                    { "numberAsText", "2024" },
                    { "notJson", "{ not json" },
                },
            };

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            var variablesJson = JsonNode.Parse(logDto.VariablesAsIndentedJson).AsObject();
            Assert.Equal("The video", (string)variablesJson["metadata.Title"]);
            Assert.Equal("2024", (string)variablesJson["numberAsText"]);
            Assert.Equal("{ not json", (string)variablesJson["notJson"]);
        }

        [Fact]
        public void LogVariablesAreIndented()
        {
            var log = new OPMFLog { Variables = new Dictionary<string, object> { { "key", "value" } } };

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            Assert.Equal("{\n  \"key\": \"value\"\n}", logDto.VariablesAsIndentedJson.ReplaceLineEndings("\n"));
        }

        [Fact]
        public void ALogEntryWithNoVariablesGivesTheTextNullAsTheUiRendersToday()
        {
            var log = new OPMFLog { Variables = null };

            var logDto = DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto(log);

            Assert.Equal("null", logDto.VariablesAsIndentedJson);
        }

        [Fact]
        public void AnErrorToRecordIsConvertedToAnOPMFErrorWithTheGivenDateAndVariablesAsStrings()
        {
            var errorToRecord = new RepositoryErrorLogEntryToRecordDto
            {
                Message = "it failed",
                ExceptionText = "the full exception text",
                VariablesAsJsonText = new Dictionary<string, string> { { "channels", "[]" }, { "metadatas", "null" } },
            };

            var error = DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToOPMFError(errorToRecord, SomeDate);

            Assert.Equal(typeof(OPMFError), error.GetType());
            Assert.Equal(OPMFLogType.Error, error.Type);
            Assert.Equal("it failed", error.Message);
            Assert.Equal("the full exception text", error.ExceptionObject);
            Assert.Equal(SomeDate, error.DateCreated);
            Assert.Equal(new[] { "channels", "metadatas" }, error.Variables.Keys.OrderBy(key => key));
            Assert.Equal("[]", error.Variables["channels"]);
            Assert.IsType<string>(error.Variables["metadatas"]);
        }
    }
}
