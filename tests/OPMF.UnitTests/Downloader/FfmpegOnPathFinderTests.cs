using System.Collections.Generic;
using System.IO;
using OPMF.Downloader.YTDownloader;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class FfmpegOnPathFinderTests
    {
        private static FfmpegOnPathFinder FinderWhereFfmpegExistsIn(string pathEnvironmentVariableValue, params string[] foldersHoldingFfmpeg)
        {
            var existingFfmpegFiles = new List<string>();
            foreach (var folder in foldersHoldingFfmpeg)
            {
                existingFfmpegFiles.Add(Path.Join(folder, "ffmpeg.exe"));
            }
            return new FfmpegOnPathFinder(pathEnvironmentVariableValue, existingFfmpegFiles.Contains);
        }

        [Fact]
        public void FfmpegIsFoundWhenItIsInTheSecondFolderOnPath()
        {
            var finder = FinderWhereFfmpegExistsIn(@"C:\Windows;C:\Tools\ffmpeg\bin;C:\Other", @"C:\Tools\ffmpeg\bin");

            Assert.True(finder.IsFfmpegFoundInAFolderOnPath());
        }

        [Fact]
        public void FfmpegIsNotFoundWhenNoFolderOnPathHoldsIt()
        {
            var finder = FinderWhereFfmpegExistsIn(@"C:\Windows;C:\Other", @"C:\NotOnPath");

            Assert.False(finder.IsFfmpegFoundInAFolderOnPath());
        }

        [Fact]
        public void FfmpegIsNotFoundWhenPathIsEmpty()
        {
            var finder = FinderWhereFfmpegExistsIn(string.Empty, @"C:\Tools\ffmpeg\bin");

            Assert.False(finder.IsFfmpegFoundInAFolderOnPath());
        }

        [Fact]
        public void SurroundingSpacesAndQuotesOfAFolderOnPathAreIgnored()
        {
            var finder = FinderWhereFfmpegExistsIn(@"C:\Windows;  ""C:\Program Files\ffmpeg\bin""  ", @"C:\Program Files\ffmpeg\bin");

            Assert.True(finder.IsFfmpegFoundInAFolderOnPath());
        }

        [Fact]
        public void EmptyEntriesBetweenSemicolonsAreSkippedAndNeverMeanTheCurrentFolder()
        {
            var checkedFilePaths = new List<string>();
            var finder = new FfmpegOnPathFinder(@"C:\Windows;;  ;C:\Other;", filePath => { checkedFilePaths.Add(filePath); return false; });

            Assert.False(finder.IsFfmpegFoundInAFolderOnPath());
            Assert.Equal(new[] { Path.Join(@"C:\Windows", "ffmpeg.exe"), Path.Join(@"C:\Other", "ffmpeg.exe") }, checkedFilePaths);
        }
    }
}
