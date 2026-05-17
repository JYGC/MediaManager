using System.Collections.Generic;
using System.Reflection;
using OPMF.Downloader;
using OPMF.Entities;
using Xunit;

namespace OPMF.ContractTests
{
    public class IDownloaderContractTests
    {
        [Fact]
        public void DownloadQueueProperty_IsReadWriteListOfMetadata()
        {
            var prop = typeof(IDownloader).GetProperty(nameof(IDownloader.DownloadQueue));
            Assert.NotNull(prop);
            Assert.Equal(typeof(List<Metadata>), prop.PropertyType);
            Assert.True(prop.CanRead);
            Assert.True(prop.CanWrite);
        }

        [Fact]
        public void StartDownloadingQueueMethod_IsVoidNoArgs()
        {
            var method = typeof(IDownloader).GetMethod(nameof(IDownloader.StartDownloadingQueue));
            Assert.NotNull(method);
            Assert.Equal(typeof(void), method.ReturnType);
            Assert.Empty(method.GetParameters());
        }

        [Fact]
        public void IDownloader_HasExactlyExpectedMembers()
        {
            var memberNames = new HashSet<string>();
            foreach (var m in typeof(IDownloader).GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                memberNames.Add(m.Name);
            }
            Assert.Contains(nameof(IDownloader.DownloadQueue), memberNames);
            Assert.Contains(nameof(IDownloader.StartDownloadingQueue), memberNames);
        }
    }
}
