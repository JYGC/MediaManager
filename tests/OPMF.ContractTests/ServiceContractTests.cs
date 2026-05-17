using System;
using System.Collections.Generic;
using MediaManager.Services2;
using OPMF.Entities;
using Xunit;

namespace OPMF.ContractTests
{
    public class IChannelMetadataServicesContractTests
    {
        [Fact]
        public void Implementation_IsRegistered()
        {
            Assert.Contains(typeof(IChannelMetadataServices), typeof(ChannelMetadataServices).GetInterfaces());
        }

        [Theory]
        [InlineData(nameof(IChannelMetadataServices.GetByChannelAndTitleContainingWord))]
        [InlineData(nameof(IChannelMetadataServices.GetByTitleContainingWord))]
        [InlineData(nameof(IChannelMetadataServices.GetNew))]
        [InlineData(nameof(IChannelMetadataServices.GetToDownloadAndWait))]
        [InlineData(nameof(IChannelMetadataServices.GetVideoByUrl))]
        public void DefinesMethod_ReturningListOfChannelMetadata(string name)
        {
            var method = typeof(IChannelMetadataServices).GetMethod(name);
            Assert.NotNull(method);
            Assert.Equal(typeof(List<ChannelMetadata>), method.ReturnType);
        }
    }

    public class IChannelServicesContractTests
    {
        [Fact]
        public void Implementation_IsRegistered()
        {
            Assert.Contains(typeof(IChannelServices), typeof(ChannelServices).GetInterfaces());
        }

        [Fact]
        public void GetAll_ReturnsChannelList()
        {
            var method = typeof(IChannelServices).GetMethod(nameof(IChannelServices.GetAll));
            Assert.NotNull(method);
            Assert.Equal(typeof(List<Channel>), method.ReturnType);
            Assert.Empty(method.GetParameters());
        }

        [Fact]
        public void UpdateBlackListStatus_TakesChannelEnumerable_Void()
        {
            var method = typeof(IChannelServices).GetMethod(nameof(IChannelServices.UpdateBlackListStatus));
            Assert.NotNull(method);
            Assert.Equal(typeof(void), method.ReturnType);
            Assert.Equal(typeof(IEnumerable<Channel>), method.GetParameters()[0].ParameterType);
        }
    }

    public class IMetadataServicesContractTests
    {
        [Fact]
        public void Implementation_IsRegistered()
        {
            Assert.Contains(typeof(IMetadataServices), typeof(MetadataServices).GetInterfaces());
        }

        [Theory]
        [InlineData(nameof(IMetadataServices.UpdateStatus))]
        [InlineData(nameof(IMetadataServices.UpdateIsBeingProcessed))]
        [InlineData(nameof(IMetadataServices.DownloadNow))]
        public void DefinesMethod_Void(string name)
        {
            var method = typeof(IMetadataServices).GetMethod(name);
            Assert.NotNull(method);
            Assert.Equal(typeof(void), method.ReturnType);
        }
    }

    public class ITaskRunnerServicesContractTests
    {
        [Fact]
        public void Implementation_IsRegistered()
        {
            Assert.Contains(typeof(ITaskRunnerServices), typeof(TaskRunnerServices).GetInterfaces());
        }

        [Fact]
        public void DownloadOneVideo_TakesMetadata_Void()
        {
            var method = typeof(ITaskRunnerServices).GetMethod(nameof(ITaskRunnerServices.DownloadOneVideo));
            Assert.NotNull(method);
            Assert.Equal(typeof(void), method.ReturnType);
            Assert.Equal(typeof(Metadata), method.GetParameters()[0].ParameterType);
        }
    }

    public class ILogServicesContractTests
    {
        [Fact]
        public void Implementation_IsRegistered()
        {
            Assert.Contains(typeof(ILogServices), typeof(LogServices).GetInterfaces());
        }

        [Theory]
        [InlineData(nameof(ILogServices.GetWarnings))]
        [InlineData(nameof(ILogServices.GetErrors))]
        public void Method_TakesTwoDateTimes_ReturnsLogList(string name)
        {
            var method = typeof(ILogServices).GetMethod(name);
            Assert.NotNull(method);
            Assert.Equal(typeof(List<OPMFLog>), method.ReturnType);
            var args = method.GetParameters();
            Assert.Equal(2, args.Length);
            Assert.Equal(typeof(DateTime), args[0].ParameterType);
            Assert.Equal(typeof(DateTime), args[1].ParameterType);
        }
    }
}
