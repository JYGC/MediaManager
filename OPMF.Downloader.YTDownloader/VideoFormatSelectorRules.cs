using System;
using OPMF.Settings;

namespace OPMF.Downloader.YTDownloader
{
    public static class VideoFormatSelectorRules
    {
        public static string BuildFormatSelectorThatBothExecutablesUnderstand(DownloadQualityChoices downloadQualityChoices)
        {
            int? limitInPixelsOnTheShorterSide = GetLimitInPixelsOnTheShorterSide(downloadQualityChoices.MaximumVideoResolution);
            string videoStream = BuildVideoStream(downloadQualityChoices.VideoCodec, limitInPixelsOnTheShorterSide);
            string audioStream = BuildAudioStream(downloadQualityChoices.AudioQuality);
            string preMergedFile = BuildPreMergedFile(limitInPixelsOnTheShorterSide);
            return $"({videoStream})+({audioStream})/{preMergedFile}/({videoStream})";
        }

        private static int? GetLimitInPixelsOnTheShorterSide(MaximumVideoResolutionChoice maximumVideoResolution)
        {
            return maximumVideoResolution switch
            {
                MaximumVideoResolutionChoice.NoLimit => null,
                MaximumVideoResolutionChoice.UpTo2160p => 2160,
                MaximumVideoResolutionChoice.UpTo1440p => 1440,
                MaximumVideoResolutionChoice.UpTo1080p => 1080,
                MaximumVideoResolutionChoice.UpTo720p => 720,
                MaximumVideoResolutionChoice.UpTo480p => 480,
                MaximumVideoResolutionChoice.UpTo360p => 360,
                _ => throw new ArgumentOutOfRangeException(nameof(maximumVideoResolution), maximumVideoResolution, "No limit in pixels matches this maximum video resolution."),
            };
        }

        // [width<=N][height>N] can only match a vertical stream, whose shorter side is its width; landscape and square streams fall through to [height<=N].
        private static string BuildVideoStream(VideoCodecChoice videoCodec, int? limitInPixelsOnTheShorterSide)
        {
            bool preferH264 = videoCodec == VideoCodecChoice.H264;
            if (limitInPixelsOnTheShorterSide == null)
            {
                return preferH264 ? "bestvideo[vcodec^=avc1]/bestvideo" : "bestvideo";
            }
            int limit = limitInPixelsOnTheShorterSide.Value;
            string withinTheLimitOnAnyCodec = $"bestvideo[width<={limit}][height>{limit}]/bestvideo[height<={limit}]/worstvideo";
            return preferH264
                ? $"bestvideo[vcodec^=avc1][width<={limit}][height>{limit}]/bestvideo[vcodec^=avc1][height<={limit}]/{withinTheLimitOnAnyCodec}"
                : withinTheLimitOnAnyCodec;
        }

        private static string BuildAudioStream(AudioQualityChoice audioQuality)
        {
            return audioQuality switch
            {
                AudioQualityChoice.BestAac => "bestaudio[acodec^=mp4a][format_id!$=-drc]/bestaudio[acodec^=mp4a]/bestaudio[format_id!$=-drc]/bestaudio",
                AudioQualityChoice.BestOfAnyCodec => "bestaudio[format_id!$=-drc]/bestaudio",
                AudioQualityChoice.SmallestAac => "worstaudio[acodec^=mp4a][format_id!$=-drc]/worstaudio[acodec^=mp4a]/worstaudio[format_id!$=-drc]/worstaudio",
                _ => throw new ArgumentOutOfRangeException(nameof(audioQuality), audioQuality, "No audio stream selector matches this audio quality."),
            };
        }

        private static string BuildPreMergedFile(int? limitInPixelsOnTheShorterSide)
        {
            if (limitInPixelsOnTheShorterSide == null)
            {
                return "best";
            }
            int limit = limitInPixelsOnTheShorterSide.Value;
            return $"best[width<={limit}][height>{limit}]/best[height<={limit}]/worst";
        }
    }
}
