using System;
using OPMF.Entities;
using OPMF.Settings;

namespace OPMF.Downloader.YTDownloader
{
    public static class DownloadQualityChoiceRules
    {
        public static DownloadQualityChoices ChooseDownloadQualityOfVideo(Metadata video, YoutubeDLConfig youtubeDLConfig)
        {
            return new DownloadQualityChoices(
                video.VideoCodecChoiceOverride == null ? youtubeDLConfig.VideoCodecToPreferWhenDownloading : ConvertToVideoCodecChoice(video.VideoCodecChoiceOverride.Value),
                video.MaximumVideoResolutionChoiceOverride == null ? youtubeDLConfig.MaximumVideoResolutionOnTheShorterSide : ConvertToMaximumVideoResolutionChoice(video.MaximumVideoResolutionChoiceOverride.Value),
                video.AudioQualityChoiceOverride == null ? youtubeDLConfig.AudioQualityToDownload : ConvertToAudioQualityChoice(video.AudioQualityChoiceOverride.Value));
        }

        private static VideoCodecChoice ConvertToVideoCodecChoice(EntityVideoCodecChoice choice)
        {
            return choice switch
            {
                EntityVideoCodecChoice.H264 => VideoCodecChoice.H264,
                EntityVideoCodecChoice.AnyCodec => VideoCodecChoice.AnyCodec,
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No video codec choice of the config file matches this video codec choice."),
            };
        }

        private static MaximumVideoResolutionChoice ConvertToMaximumVideoResolutionChoice(EntityMaximumVideoResolutionChoice choice)
        {
            return choice switch
            {
                EntityMaximumVideoResolutionChoice.NoLimit => MaximumVideoResolutionChoice.NoLimit,
                EntityMaximumVideoResolutionChoice.UpTo2160p => MaximumVideoResolutionChoice.UpTo2160p,
                EntityMaximumVideoResolutionChoice.UpTo1440p => MaximumVideoResolutionChoice.UpTo1440p,
                EntityMaximumVideoResolutionChoice.UpTo1080p => MaximumVideoResolutionChoice.UpTo1080p,
                EntityMaximumVideoResolutionChoice.UpTo720p => MaximumVideoResolutionChoice.UpTo720p,
                EntityMaximumVideoResolutionChoice.UpTo480p => MaximumVideoResolutionChoice.UpTo480p,
                EntityMaximumVideoResolutionChoice.UpTo360p => MaximumVideoResolutionChoice.UpTo360p,
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No maximum video resolution choice of the config file matches this maximum video resolution choice."),
            };
        }

        private static AudioQualityChoice ConvertToAudioQualityChoice(EntityAudioQualityChoice choice)
        {
            return choice switch
            {
                EntityAudioQualityChoice.BestAac => AudioQualityChoice.BestAac,
                EntityAudioQualityChoice.BestOfAnyCodec => AudioQualityChoice.BestOfAnyCodec,
                EntityAudioQualityChoice.SmallestAac => AudioQualityChoice.SmallestAac,
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No audio quality choice of the config file matches this audio quality choice."),
            };
        }
    }
}
