using MediaManager.Services2;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class DownloadedFileNameRulesTests
    {
        [Theory]
        [InlineData("x.f160.mp4")]
        [InlineData("x.f139.m4a")]
        [InlineData("x.f140-drc.m4a")]
        [InlineData("x.f251-12.webm")]
        [InlineData("Market show# Buy pork belly.f137.mp4")]
        public void AFileNamedWithAFormatIdBeforeItsExtensionIsAStreamFileLeftBeforeMerging(string fileName)
        {
            Assert.True(DownloadedFileNameRules.IsNameOfStreamFileLeftBeforeMerging(fileName));
        }

        [Theory]
        [InlineData("x.mp4")]
        [InlineData("x.vtt")]
        [InlineData("Market show# Buy pork belly.mp4")]
        [InlineData("My video.final.mp4")]
        [InlineData("Version 2.f.mp4")]
        public void AnyOtherFileIsNotAStreamFile(string fileName)
        {
            Assert.False(DownloadedFileNameRules.IsNameOfStreamFileLeftBeforeMerging(fileName));
        }
    }
}
