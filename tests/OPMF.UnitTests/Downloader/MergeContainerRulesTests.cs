using System;
using OPMF.Downloader.YTDownloader;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class MergeContainerRulesTests
    {
        [Theory]
        [InlineData("mp4")]
        [InlineData("mkv")]
        public void Mp4AndMkvAreTheContainersDownloadsCanBeMergedInto(string videoExtension)
        {
            Assert.Equal(videoExtension, MergeContainerRules.GetMergeContainerOfVideoExtensionOrThrow(videoExtension));
        }

        [Theory]
        [InlineData("webm")]
        [InlineData("avi")]
        [InlineData("MP4 ")]
        [InlineData("")]
        public void AnyOtherVideoExtensionThrowsAnInvalidOperationExceptionNamingTheValueAndTheAllowedOnes(string videoExtension)
        {
            var exception = Assert.Throws<InvalidOperationException>(() => MergeContainerRules.GetMergeContainerOfVideoExtensionOrThrow(videoExtension));

            Assert.Equal(
                "VideoExtension in config.json is \"" + videoExtension + "\", but downloads can only be merged into mp4 or mkv. Set VideoExtension under YoutubeDL to mp4 or mkv.",
                exception.Message);
        }
    }
}
