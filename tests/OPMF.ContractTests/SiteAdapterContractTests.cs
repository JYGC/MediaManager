using System.Collections.Generic;
using OPMF.Entities;
using OPMF.SiteAdapter;
using Xunit;

namespace OPMF.ContractTests
{
    public class IMetadataFetcherContractTests
    {
        [Fact]
        public void GenericParameters_AreConstrainedToChannelAndMetadata()
        {
            var typeArgs = typeof(IMetadataFetcher<,>).GetGenericArguments();
            Assert.Equal(2, typeArgs.Length);

            var channelConstraints = typeArgs[0].GetGenericParameterConstraints();
            Assert.Contains(typeof(Channel), channelConstraints);

            var metadataConstraints = typeArgs[1].GetGenericParameterConstraints();
            Assert.Contains(typeof(Metadata), metadataConstraints);
        }

        [Fact]
        public void FetchMetadata_TakesChannelListByRef_ReturnsVideoInfoList()
        {
            var closed = typeof(IMetadataFetcher<Channel, Metadata>);
            var method = closed.GetMethod(nameof(IMetadataFetcher<Channel, Metadata>.FetchMetadata));
            Assert.NotNull(method);
            Assert.Equal(typeof(List<Metadata>), method.ReturnType);
            var args = method.GetParameters();
            Assert.Single(args);
            Assert.True(args[0].ParameterType.IsByRef);
            Assert.Equal(typeof(List<Channel>).MakeByRefType(), args[0].ParameterType);
        }
    }

    public class ISiteChannelFinderContractTests
    {
        [Fact]
        public void FindChannelById_TakesStringArray_ReturnsChannelList()
        {
            var method = typeof(ISiteChannelFinder).GetMethod(nameof(ISiteChannelFinder.FindChannelById));
            Assert.NotNull(method);
            Assert.Equal(typeof(List<Channel>), method.ReturnType);
            var args = method.GetParameters();
            Assert.Single(args);
            Assert.Equal(typeof(string[]), args[0].ParameterType);
        }

        [Fact]
        public void FindChannelByName_TakesStringArray_ReturnsChannelList()
        {
            var method = typeof(ISiteChannelFinder).GetMethod(nameof(ISiteChannelFinder.FindChannelByName));
            Assert.NotNull(method);
            Assert.Equal(typeof(List<Channel>), method.ReturnType);
            var args = method.GetParameters();
            Assert.Single(args);
            Assert.Equal(typeof(string[]), args[0].ParameterType);
        }

        [Fact]
        public void ImportChannels_TakesNoArgs_ReturnsChannelList()
        {
            var method = typeof(ISiteChannelFinder).GetMethod(nameof(ISiteChannelFinder.ImportChannels));
            Assert.NotNull(method);
            Assert.Equal(typeof(List<Channel>), method.ReturnType);
            Assert.Empty(method.GetParameters());
        }
    }

    public class ISiteVideoMetadataGetterContractTests
    {
        [Fact]
        public void GetSiteIdFromURL_TakesStringReturnsString()
        {
            var method = typeof(ISiteVideoMetadataGetter).GetMethod(nameof(ISiteVideoMetadataGetter.GetSiteIdFromURL));
            Assert.NotNull(method);
            Assert.Equal(typeof(string), method.ReturnType);
            Assert.Single(method.GetParameters());
            Assert.Equal(typeof(string), method.GetParameters()[0].ParameterType);
        }

        [Fact]
        public void GetVideoByURL_TakesStringReturnsMetadataChannelTuple()
        {
            var method = typeof(ISiteVideoMetadataGetter).GetMethod(nameof(ISiteVideoMetadataGetter.GetVideoByURL));
            Assert.NotNull(method);
            Assert.Equal(typeof((Metadata, Channel)), method.ReturnType);
            Assert.Single(method.GetParameters());
            Assert.Equal(typeof(string), method.GetParameters()[0].ParameterType);
        }
    }
}
