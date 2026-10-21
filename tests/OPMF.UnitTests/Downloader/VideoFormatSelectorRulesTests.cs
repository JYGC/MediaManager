using System.Collections.Generic;
using OPMF.Downloader.YTDownloader;
using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Downloader
{
    public class VideoFormatSelectorRulesTests
    {
        private static readonly Dictionary<VideoCodecChoice, string> VideoStreamWhenThereIsNoLimit = new Dictionary<VideoCodecChoice, string>
        {
            [VideoCodecChoice.H264] = "bestvideo[vcodec^=avc1]/bestvideo",
            [VideoCodecChoice.AnyCodec] = "bestvideo",
        };

        private static readonly Dictionary<VideoCodecChoice, string> VideoStreamWhenThereIsALimitOfNPixels = new Dictionary<VideoCodecChoice, string>
        {
            [VideoCodecChoice.H264] = "bestvideo[vcodec^=avc1][width<=N][height>N]/bestvideo[vcodec^=avc1][height<=N]/bestvideo[width<=N][height>N]/bestvideo[height<=N]/worstvideo",
            [VideoCodecChoice.AnyCodec] = "bestvideo[width<=N][height>N]/bestvideo[height<=N]/worstvideo",
        };

        private static readonly Dictionary<AudioQualityChoice, string> AudioStream = new Dictionary<AudioQualityChoice, string>
        {
            [AudioQualityChoice.BestAac] = "bestaudio[acodec^=mp4a][format_id!$=-drc]/bestaudio[acodec^=mp4a]/bestaudio[format_id!$=-drc]/bestaudio",
            [AudioQualityChoice.BestOfAnyCodec] = "bestaudio[format_id!$=-drc]/bestaudio",
            [AudioQualityChoice.SmallestAac] = "worstaudio[acodec^=mp4a][format_id!$=-drc]/worstaudio[acodec^=mp4a]/worstaudio[format_id!$=-drc]/worstaudio",
        };

        private const string PreMergedFileWhenThereIsNoLimit = "best";
        private const string PreMergedFileWhenThereIsALimitOfNPixels = "best[width<=N][height>N]/best[height<=N]/worst";

        private static readonly Dictionary<MaximumVideoResolutionChoice, int?> LimitInPixels = new Dictionary<MaximumVideoResolutionChoice, int?>
        {
            [MaximumVideoResolutionChoice.NoLimit] = null,
            [MaximumVideoResolutionChoice.UpTo2160p] = 2160,
            [MaximumVideoResolutionChoice.UpTo1440p] = 1440,
            [MaximumVideoResolutionChoice.UpTo1080p] = 1080,
            [MaximumVideoResolutionChoice.UpTo720p] = 720,
            [MaximumVideoResolutionChoice.UpTo480p] = 480,
            [MaximumVideoResolutionChoice.UpTo360p] = 360,
        };

        public static IEnumerable<object[]> EveryCombinationOfCodecLimitAndAudio()
        {
            foreach (var codec in System.Enum.GetValues<VideoCodecChoice>())
            {
                foreach (var limit in System.Enum.GetValues<MaximumVideoResolutionChoice>())
                {
                    foreach (var audio in System.Enum.GetValues<AudioQualityChoice>())
                    {
                        yield return new object[] { codec, limit, audio };
                    }
                }
            }
        }

        private static string ExpectedSelector(VideoCodecChoice codec, MaximumVideoResolutionChoice limit, AudioQualityChoice audio)
        {
            var limitInPixels = LimitInPixels[limit];
            var videoStream = limitInPixels == null
                ? VideoStreamWhenThereIsNoLimit[codec]
                : VideoStreamWhenThereIsALimitOfNPixels[codec].Replace("N", limitInPixels.ToString());
            var preMergedFile = limitInPixels == null
                ? PreMergedFileWhenThereIsNoLimit
                : PreMergedFileWhenThereIsALimitOfNPixels.Replace("N", limitInPixels.ToString());
            return $"({videoStream})+({AudioStream[audio]})/{preMergedFile}/({videoStream})";
        }

        [Fact]
        public void TheTableCoversEveryCombinationOfCodecLimitAndAudio()
        {
            Assert.Equal(2 * 7 * 3, new List<object[]>(EveryCombinationOfCodecLimitAndAudio()).Count);
        }

        [Theory]
        [MemberData(nameof(EveryCombinationOfCodecLimitAndAudio))]
        public void TheSelectorIsBuiltFromTheVideoStreamTheAudioStreamAndThePreMergedFileOfTheChoices(VideoCodecChoice codec, MaximumVideoResolutionChoice limit, AudioQualityChoice audio)
        {
            var selector = VideoFormatSelectorRules.BuildFormatSelectorThatBothExecutablesUnderstand(new DownloadQualityChoices(codec, limit, audio));

            Assert.Equal(ExpectedSelector(codec, limit, audio), selector);
        }

        [Fact]
        public void TheDefaultChoicesGiveTheBestH264VideoWithTheBestAacAudioAndNoLimit()
        {
            var selector = VideoFormatSelectorRules.BuildFormatSelectorThatBothExecutablesUnderstand(
                new DownloadQualityChoices(VideoCodecChoice.H264, MaximumVideoResolutionChoice.NoLimit, AudioQualityChoice.BestAac));

            Assert.Equal(
                "(bestvideo[vcodec^=avc1]/bestvideo)+(bestaudio[acodec^=mp4a][format_id!$=-drc]/bestaudio[acodec^=mp4a]/bestaudio[format_id!$=-drc]/bestaudio)/best/(bestvideo[vcodec^=avc1]/bestvideo)",
                selector);
        }

        [Fact]
        public void TheExampleOfTheDesignFor720pH264AndBestAacIsBuiltExactly()
        {
            var selector = VideoFormatSelectorRules.BuildFormatSelectorThatBothExecutablesUnderstand(
                new DownloadQualityChoices(VideoCodecChoice.H264, MaximumVideoResolutionChoice.UpTo720p, AudioQualityChoice.BestAac));

            Assert.Equal(
                "(bestvideo[vcodec^=avc1][width<=720][height>720]/bestvideo[vcodec^=avc1][height<=720]/bestvideo[width<=720][height>720]/bestvideo[height<=720]/worstvideo)+(bestaudio[acodec^=mp4a][format_id!$=-drc]/bestaudio[acodec^=mp4a]/bestaudio[format_id!$=-drc]/bestaudio)/best[width<=720][height>720]/best[height<=720]/worst/(bestvideo[vcodec^=avc1][width<=720][height>720]/bestvideo[vcodec^=avc1][height<=720]/bestvideo[width<=720][height>720]/bestvideo[height<=720]/worstvideo)",
                selector);
        }
    }
}
