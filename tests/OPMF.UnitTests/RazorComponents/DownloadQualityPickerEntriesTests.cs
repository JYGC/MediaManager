using System.Linq;
using MediaManager.Dtos.Application;
using MediaManagerUI.Shared;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class DownloadQualityPickerEntriesTests
    {
        [Fact]
        public void TheAppWideVideoCodecEntriesAreH264ThenAnyCodecWithTheirLabels()
        {
            var entries = DownloadQualityPickerEntries.ForAppWideVideoCodec();

            Assert.Equal(
                new[]
                {
                    (ApplicationVideoCodecChoice?)ApplicationVideoCodecChoice.H264,
                    ApplicationVideoCodecChoice.AnyCodec,
                },
                entries.Select(entry => entry.Choice));
            Assert.Equal(new[] { "H.264 (plays everywhere)", "Any codec (AV1, VP9 or H.264)" }, entries.Select(entry => entry.Label));
        }

        [Fact]
        public void TheAppWideMaximumVideoResolutionEntriesGoFromNoLimitDownTo360p()
        {
            var entries = DownloadQualityPickerEntries.ForAppWideMaximumVideoResolution();

            Assert.Equal(
                new[]
                {
                    (ApplicationMaximumVideoResolutionChoice?)ApplicationMaximumVideoResolutionChoice.NoLimit,
                    ApplicationMaximumVideoResolutionChoice.UpTo2160p,
                    ApplicationMaximumVideoResolutionChoice.UpTo1440p,
                    ApplicationMaximumVideoResolutionChoice.UpTo1080p,
                    ApplicationMaximumVideoResolutionChoice.UpTo720p,
                    ApplicationMaximumVideoResolutionChoice.UpTo480p,
                    ApplicationMaximumVideoResolutionChoice.UpTo360p,
                },
                entries.Select(entry => entry.Choice));
            Assert.Equal(new[] { "No limit", "2160p (4K)", "1440p", "1080p", "720p", "480p", "360p" }, entries.Select(entry => entry.Label));
        }

        [Fact]
        public void TheAppWideAudioQualityEntriesAreBestAacThenBestOfAnyCodecThenSmallestAac()
        {
            var entries = DownloadQualityPickerEntries.ForAppWideAudioQuality();

            Assert.Equal(
                new[]
                {
                    (ApplicationAudioQualityChoice?)ApplicationAudioQualityChoice.BestAac,
                    ApplicationAudioQualityChoice.BestOfAnyCodec,
                    ApplicationAudioQualityChoice.SmallestAac,
                },
                entries.Select(entry => entry.Choice));
            Assert.Equal(
                new[] { "Best AAC (about 128 kbps)", "Best of any codec (usually Opus, about 160 kbps)", "Smallest AAC (about 50 kbps)" },
                entries.Select(entry => entry.Label));
        }

        [Theory]
        [InlineData(ApplicationVideoCodecChoice.H264, "Same as Settings (H.264)")]
        [InlineData(ApplicationVideoCodecChoice.AnyCodec, "Same as Settings (Any codec)")]
        public void ThePerVideoVideoCodecEntriesStartWithSameAsSettingsNamingTheSettingsValue(ApplicationVideoCodecChoice appWideChoice, string expectedFirstLabel)
        {
            var entries = DownloadQualityPickerEntries.ForVideoCodecOverride(appWideChoice);

            Assert.Null(entries[0].Choice);
            Assert.Equal(expectedFirstLabel, entries[0].Label);
            Assert.Equal(DownloadQualityPickerEntries.ForAppWideVideoCodec(), entries.Skip(1).ToList());
        }

        [Theory]
        [InlineData(ApplicationMaximumVideoResolutionChoice.NoLimit, "Same as Settings (No limit)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo2160p, "Same as Settings (2160p)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo1440p, "Same as Settings (1440p)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo1080p, "Same as Settings (1080p)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo720p, "Same as Settings (720p)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo480p, "Same as Settings (480p)")]
        [InlineData(ApplicationMaximumVideoResolutionChoice.UpTo360p, "Same as Settings (360p)")]
        public void ThePerVideoMaximumVideoResolutionEntriesStartWithSameAsSettingsNamingTheSettingsValue(ApplicationMaximumVideoResolutionChoice appWideChoice, string expectedFirstLabel)
        {
            var entries = DownloadQualityPickerEntries.ForMaximumVideoResolutionOverride(appWideChoice);

            Assert.Null(entries[0].Choice);
            Assert.Equal(expectedFirstLabel, entries[0].Label);
            Assert.Equal(DownloadQualityPickerEntries.ForAppWideMaximumVideoResolution(), entries.Skip(1).ToList());
        }

        [Theory]
        [InlineData(ApplicationAudioQualityChoice.BestAac, "Same as Settings (Best AAC)")]
        [InlineData(ApplicationAudioQualityChoice.BestOfAnyCodec, "Same as Settings (Best of any codec)")]
        [InlineData(ApplicationAudioQualityChoice.SmallestAac, "Same as Settings (Smallest AAC)")]
        public void ThePerVideoAudioQualityEntriesStartWithSameAsSettingsNamingTheSettingsValue(ApplicationAudioQualityChoice appWideChoice, string expectedFirstLabel)
        {
            var entries = DownloadQualityPickerEntries.ForAudioQualityOverride(appWideChoice);

            Assert.Null(entries[0].Choice);
            Assert.Equal(expectedFirstLabel, entries[0].Label);
            Assert.Equal(DownloadQualityPickerEntries.ForAppWideAudioQuality(), entries.Skip(1).ToList());
        }
    }
}
