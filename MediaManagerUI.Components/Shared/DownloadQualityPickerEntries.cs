using MediaManager.Dtos.Application;

namespace MediaManagerUI.Shared
{
    // A null Choice is the entry "Same as Settings (...)", so it is a real entry of the list
    public record DownloadQualityPickerEntry<TChoice>(TChoice? Choice, string Label) where TChoice : struct;

    public static class DownloadQualityPickerEntries
    {
        public static List<DownloadQualityPickerEntry<ApplicationVideoCodecChoice>> ForAppWideVideoCodec()
        {
            return
            [
                new(ApplicationVideoCodecChoice.H264, "H.264 (plays everywhere)"),
                new(ApplicationVideoCodecChoice.AnyCodec, "Any codec (AV1, VP9 or H.264)"),
            ];
        }

        public static List<DownloadQualityPickerEntry<ApplicationVideoCodecChoice>> ForVideoCodecOverride(ApplicationVideoCodecChoice appWideVideoCodec)
        {
            return [new(null, SameAsSettingsLabel(ShortNameOf(appWideVideoCodec))), .. ForAppWideVideoCodec()];
        }

        public static List<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>> ForAppWideMaximumVideoResolution()
        {
            return
            [
                new(ApplicationMaximumVideoResolutionChoice.NoLimit, "No limit"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo2160p, "2160p (4K)"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo1440p, "1440p"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo1080p, "1080p"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo720p, "720p"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo480p, "480p"),
                new(ApplicationMaximumVideoResolutionChoice.UpTo360p, "360p"),
            ];
        }

        public static List<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>> ForMaximumVideoResolutionOverride(ApplicationMaximumVideoResolutionChoice appWideMaximumVideoResolution)
        {
            return [new(null, SameAsSettingsLabel(ShortNameOf(appWideMaximumVideoResolution))), .. ForAppWideMaximumVideoResolution()];
        }

        public static List<DownloadQualityPickerEntry<ApplicationAudioQualityChoice>> ForAppWideAudioQuality()
        {
            return
            [
                new(ApplicationAudioQualityChoice.BestAac, "Best AAC (about 128 kbps)"),
                new(ApplicationAudioQualityChoice.BestOfAnyCodec, "Best of any codec (usually Opus, about 160 kbps)"),
                new(ApplicationAudioQualityChoice.SmallestAac, "Smallest AAC (about 50 kbps)"),
            ];
        }

        public static List<DownloadQualityPickerEntry<ApplicationAudioQualityChoice>> ForAudioQualityOverride(ApplicationAudioQualityChoice appWideAudioQuality)
        {
            return [new(null, SameAsSettingsLabel(ShortNameOf(appWideAudioQuality))), .. ForAppWideAudioQuality()];
        }

        public static string ShortNameOf(ApplicationVideoCodecChoice choice)
        {
            return choice switch
            {
                ApplicationVideoCodecChoice.H264 => "H.264",
                ApplicationVideoCodecChoice.AnyCodec => "Any codec",
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No short name matches this video codec choice."),
            };
        }

        public static string ShortNameOf(ApplicationMaximumVideoResolutionChoice choice)
        {
            return choice switch
            {
                ApplicationMaximumVideoResolutionChoice.NoLimit => "No limit",
                ApplicationMaximumVideoResolutionChoice.UpTo2160p => "2160p",
                ApplicationMaximumVideoResolutionChoice.UpTo1440p => "1440p",
                ApplicationMaximumVideoResolutionChoice.UpTo1080p => "1080p",
                ApplicationMaximumVideoResolutionChoice.UpTo720p => "720p",
                ApplicationMaximumVideoResolutionChoice.UpTo480p => "480p",
                ApplicationMaximumVideoResolutionChoice.UpTo360p => "360p",
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No short name matches this maximum video resolution choice."),
            };
        }

        public static string ShortNameOf(ApplicationAudioQualityChoice choice)
        {
            return choice switch
            {
                ApplicationAudioQualityChoice.BestAac => "Best AAC",
                ApplicationAudioQualityChoice.BestOfAnyCodec => "Best of any codec",
                ApplicationAudioQualityChoice.SmallestAac => "Smallest AAC",
                _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No short name matches this audio quality choice."),
            };
        }

        private static string SameAsSettingsLabel(string shortNameOfTheSettingsValue) => "Same as Settings (" + shortNameOfTheSettingsValue + ")";
    }
}
