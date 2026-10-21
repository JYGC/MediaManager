using OPMF.Downloader.YTDownloader;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class VideoFileNameRulesTests
    {
        [Fact]
        public void APlainTitleStaysAsItIs()
        {
            Assert.Equal("A plain title", VideoFileNameRules.CreateFileNameWithoutExtensionFromTitle("A plain title"));
        }

        [Fact]
        public void ForwardSlashesQuotesAndPercentSignsAreReplacedAsTheyAlwaysWere()
        {
            Assert.Equal("aforward slashb ''quoted'' 100percent", VideoFileNameRules.CreateFileNameWithoutExtensionFromTitle("a/b \"quoted\" 100%"));
        }

        [Theory]
        [InlineData("a<b", "a#b")]
        [InlineData("a>b", "a#b")]
        [InlineData("a:b", "a#b")]
        [InlineData("a|b", "a#b")]
        [InlineData("a?b", "a#b")]
        [InlineData("a*b", "a#b")]
        public void EachCharacterTheExecutablesRenameBecomesAHash(string title, string expectedFileName)
        {
            Assert.Equal(expectedFileName, VideoFileNameRules.CreateFileNameWithoutExtensionFromTitle(title));
        }

        [Fact]
        public void ATitleWithEveryCharacterGetsTheSameNameTheExecutableProduced()
        {
            Assert.Equal("Title# a# b# c# d# e# f", VideoFileNameRules.CreateFileNameWithoutExtensionFromTitle("Title: a? b| c* d< e> f"));
        }
    }
}
