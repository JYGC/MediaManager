using System.Collections.Generic;
using OPMF.Database;
using OPMF.Entities;
using Xunit;

namespace OPMF.ContractTests
{
    public class IDatabaseCollectionContractTests
    {
        [Fact]
        public void GetBySiteId_TakesString_ReturnsItemType()
        {
            var closed = typeof(IDatabaseCollection<Metadata>);
            var method = closed.GetMethod(nameof(IDatabaseCollection<Metadata>.GetBySiteId));
            Assert.NotNull(method);
            Assert.Equal(typeof(Metadata), method.ReturnType);
            Assert.Equal(typeof(string), method.GetParameters()[0].ParameterType);
        }
    }

    public class IChannelDbCollectionContractTests
    {
        [Fact]
        public void ExtendsIDatabaseCollection()
        {
            var closed = typeof(IChannelDbCollection<Channel>);
            Assert.Contains(typeof(IDatabaseCollection<Channel>), closed.GetInterfaces());
        }

        [Theory]
        [InlineData(nameof(IChannelDbCollection<Channel>.GetManyBySiteIds))]
        [InlineData(nameof(IChannelDbCollection<Channel>.GetAll))]
        [InlineData(nameof(IChannelDbCollection<Channel>.GetNotBacklisted))]
        [InlineData(nameof(IChannelDbCollection<Channel>.GetManyByWordInName))]
        [InlineData(nameof(IChannelDbCollection<Channel>.InsertOrUpdate))]
        [InlineData(nameof(IChannelDbCollection<Channel>.UpdateLastCheckedOutAndActivity))]
        [InlineData(nameof(IChannelDbCollection<Channel>.UpdateBlackListStatus))]
        public void DefinesMethod(string name)
        {
            Assert.NotNull(typeof(IChannelDbCollection<Channel>).GetMethod(name));
        }

        [Fact]
        public void YoutubeChannelDbCollection_ImplementsContract()
        {
            Assert.Contains(typeof(IChannelDbCollection<Channel>), typeof(YoutubeChannelDbCollection).GetInterfaces());
        }
    }

    public class IMetadataDbCollectionContractTests
    {
        [Fact]
        public void ExtendsIDatabaseCollection()
        {
            var closed = typeof(IMetadataDbCollection<Metadata>);
            Assert.Contains(typeof(IDatabaseCollection<Metadata>), closed.GetInterfaces());
        }

        [Theory]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.GetToDownload))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.GetNew))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.GetToDownloadAndWait))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.GetManyByWordInTitle))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.GetManyByChannelSiteIdAndWordInTitle))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.InsertNew))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.UpdateStatus))]
        [InlineData(nameof(IMetadataDbCollection<Metadata>.UpdateIsBeingProcessed))]
        public void DefinesMethod(string name)
        {
            Assert.NotNull(typeof(IMetadataDbCollection<Metadata>).GetMethod(name));
        }

        [Fact]
        public void YoutubeMetadataDbCollection_ImplementsContract()
        {
            Assert.Contains(typeof(IMetadataDbCollection<Metadata>), typeof(YoutubeMetadataDbCollection).GetInterfaces());
        }

        [Fact]
        public void UpdateIsBeingProcessed_OptionalIsProcessedValue_DefaultsNull()
        {
            var method = typeof(IMetadataDbCollection<Metadata>).GetMethod(nameof(IMetadataDbCollection<Metadata>.UpdateIsBeingProcessed));
            Assert.NotNull(method);
            var args = method.GetParameters();
            Assert.Equal(2, args.Length);
            Assert.Equal(typeof(IEnumerable<Metadata>), args[0].ParameterType);
            Assert.Equal(typeof(bool?), args[1].ParameterType);
            Assert.True(args[1].HasDefaultValue);
            Assert.Null(args[1].DefaultValue);
        }
    }
}
