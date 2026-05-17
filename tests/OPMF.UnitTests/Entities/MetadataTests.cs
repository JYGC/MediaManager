using System;
using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class MetadataTests
    {
        [Fact]
        public void DefaultStatus_IsNew()
        {
            var metadata = new Metadata();
            Assert.Equal(MetadataStatus.New, metadata.Status);
        }

        [Fact]
        public void DefaultIsBeingDownloaded_IsFalse()
        {
            var metadata = new Metadata();
            Assert.False(metadata.IsBeingDownloaded);
        }

        [Fact]
        public void DefaultThumbnail_IsInitialized()
        {
            var metadata = new Metadata();
            Assert.NotNull(metadata.Thumbnail);
            Assert.Null(metadata.Thumbnail.Url);
            Assert.Equal(0, metadata.Thumbnail.Width);
            Assert.Equal(0, metadata.Thumbnail.Height);
        }

        [Fact]
        public void YoutubeMetadata_InheritsMetadataDefaults()
        {
            var metadata = new YoutubeMetadata();
            Assert.Equal(MetadataStatus.New, metadata.Status);
            Assert.False(metadata.IsBeingDownloaded);
            Assert.NotNull(metadata.Thumbnail);
        }

        [Fact]
        public void Metadata_AllStatusValuesAssignable()
        {
            var metadata = new Metadata();
            foreach (var status in Enum.GetValues<MetadataStatus>())
            {
                metadata.Status = status;
                Assert.Equal(status, metadata.Status);
            }
        }

        [Fact]
        public void MetadataStatusEnum_DefinesExpectedMembers()
        {
            var values = Enum.GetValues<MetadataStatus>();
            Assert.Contains(MetadataStatus.New, values);
            Assert.Contains(MetadataStatus.ToDownload, values);
            Assert.Contains(MetadataStatus.Wait, values);
            Assert.Contains(MetadataStatus.Ignore, values);
            Assert.Contains(MetadataStatus.Downloaded, values);
            Assert.Equal(5, values.Length);
        }
    }
}
