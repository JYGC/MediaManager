using MediaManager.Dtos.Services;
using MediaManager.Repositories;
using OPMF.Settings;

namespace MediaManager.Services2;

public interface IDownloadQualitySettingsServices
{
    ServiceDownloadQualityDto GetAppWideDownloadQualityFromConfigFile();
    void SaveAppWideVideoCodecChoice(ServiceVideoCodecChoice videoCodecChoice);
    void SaveAppWideMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice maximumVideoResolutionChoice);
    void SaveAppWideAudioQualityChoice(ServiceAudioQualityChoice audioQualityChoice);
}

public class DownloadQualitySettingsServices(
    IReadonlySettings readonlySettings,
    Func<string, AppConfig> readConfigFileOrWriteDefaultConfigFileWhenMissing,
    IConfigFileYoutubeDLKeyWriter configFileYoutubeDLKeyWriter,
    ILogRepository logRepository) : IDownloadQualitySettingsServices
{
    public ServiceDownloadQualityDto GetAppWideDownloadQualityFromConfigFile()
    {
        YoutubeDLConfig youtubeDLConfig = readConfigFileOrWriteDefaultConfigFileWhenMissing(readonlySettings.GetConfigFilePath()).YoutubeDL;
        return new ServiceDownloadQualityDto
        {
            VideoCodecToPreferWhenDownloading = ConvertToServiceVideoCodecChoice(youtubeDLConfig.VideoCodecToPreferWhenDownloading),
            MaximumVideoResolutionOnTheShorterSide = ConvertToServiceMaximumVideoResolutionChoice(youtubeDLConfig.MaximumVideoResolutionOnTheShorterSide),
            AudioQualityToDownload = ConvertToServiceAudioQualityChoice(youtubeDLConfig.AudioQualityToDownload),
        };
    }

    public void SaveAppWideVideoCodecChoice(ServiceVideoCodecChoice videoCodecChoice)
    {
        WriteYoutubeDLKeyOrRecordErrorAndRethrow(nameof(YoutubeDLConfig.VideoCodecToPreferWhenDownloading), ConvertToVideoCodecChoice(videoCodecChoice));
    }

    public void SaveAppWideMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice maximumVideoResolutionChoice)
    {
        WriteYoutubeDLKeyOrRecordErrorAndRethrow(nameof(YoutubeDLConfig.MaximumVideoResolutionOnTheShorterSide), ConvertToMaximumVideoResolutionChoice(maximumVideoResolutionChoice));
    }

    public void SaveAppWideAudioQualityChoice(ServiceAudioQualityChoice audioQualityChoice)
    {
        WriteYoutubeDLKeyOrRecordErrorAndRethrow(nameof(YoutubeDLConfig.AudioQualityToDownload), ConvertToAudioQualityChoice(audioQualityChoice));
    }

    private void WriteYoutubeDLKeyOrRecordErrorAndRethrow<TChoice>(string youtubeDLKeyName, TChoice value) where TChoice : struct, Enum
    {
        try
        {
            configFileYoutubeDLKeyWriter.WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(readonlySettings.GetConfigFilePath(), youtubeDLKeyName, value);
        }
        catch (Exception exception)
        {
            logRepository.RecordErrorLogEntry(ErrorLogEntryToRecordCreator.CreateWithVariablesSerializedAsJsonText(
                exception,
                new Dictionary<string, object?> { { "youtubeDLKeyName", youtubeDLKeyName }, { "value", value.ToString() } }));
            throw;
        }
    }

    private static ServiceVideoCodecChoice ConvertToServiceVideoCodecChoice(VideoCodecChoice choice)
    {
        return choice switch
        {
            VideoCodecChoice.H264 => ServiceVideoCodecChoice.H264,
            VideoCodecChoice.AnyCodec => ServiceVideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service video codec choice matches this video codec choice."),
        };
    }

    private static VideoCodecChoice ConvertToVideoCodecChoice(ServiceVideoCodecChoice choice)
    {
        return choice switch
        {
            ServiceVideoCodecChoice.H264 => VideoCodecChoice.H264,
            ServiceVideoCodecChoice.AnyCodec => VideoCodecChoice.AnyCodec,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No video codec choice matches this service video codec choice."),
        };
    }

    private static ServiceMaximumVideoResolutionChoice ConvertToServiceMaximumVideoResolutionChoice(MaximumVideoResolutionChoice choice)
    {
        return choice switch
        {
            MaximumVideoResolutionChoice.NoLimit => ServiceMaximumVideoResolutionChoice.NoLimit,
            MaximumVideoResolutionChoice.UpTo2160p => ServiceMaximumVideoResolutionChoice.UpTo2160p,
            MaximumVideoResolutionChoice.UpTo1440p => ServiceMaximumVideoResolutionChoice.UpTo1440p,
            MaximumVideoResolutionChoice.UpTo1080p => ServiceMaximumVideoResolutionChoice.UpTo1080p,
            MaximumVideoResolutionChoice.UpTo720p => ServiceMaximumVideoResolutionChoice.UpTo720p,
            MaximumVideoResolutionChoice.UpTo480p => ServiceMaximumVideoResolutionChoice.UpTo480p,
            MaximumVideoResolutionChoice.UpTo360p => ServiceMaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service maximum video resolution choice matches this maximum video resolution choice."),
        };
    }

    private static MaximumVideoResolutionChoice ConvertToMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice choice)
    {
        return choice switch
        {
            ServiceMaximumVideoResolutionChoice.NoLimit => MaximumVideoResolutionChoice.NoLimit,
            ServiceMaximumVideoResolutionChoice.UpTo2160p => MaximumVideoResolutionChoice.UpTo2160p,
            ServiceMaximumVideoResolutionChoice.UpTo1440p => MaximumVideoResolutionChoice.UpTo1440p,
            ServiceMaximumVideoResolutionChoice.UpTo1080p => MaximumVideoResolutionChoice.UpTo1080p,
            ServiceMaximumVideoResolutionChoice.UpTo720p => MaximumVideoResolutionChoice.UpTo720p,
            ServiceMaximumVideoResolutionChoice.UpTo480p => MaximumVideoResolutionChoice.UpTo480p,
            ServiceMaximumVideoResolutionChoice.UpTo360p => MaximumVideoResolutionChoice.UpTo360p,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No maximum video resolution choice matches this service maximum video resolution choice."),
        };
    }

    private static ServiceAudioQualityChoice ConvertToServiceAudioQualityChoice(AudioQualityChoice choice)
    {
        return choice switch
        {
            AudioQualityChoice.BestAac => ServiceAudioQualityChoice.BestAac,
            AudioQualityChoice.BestOfAnyCodec => ServiceAudioQualityChoice.BestOfAnyCodec,
            AudioQualityChoice.SmallestAac => ServiceAudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No service audio quality choice matches this audio quality choice."),
        };
    }

    private static AudioQualityChoice ConvertToAudioQualityChoice(ServiceAudioQualityChoice choice)
    {
        return choice switch
        {
            ServiceAudioQualityChoice.BestAac => AudioQualityChoice.BestAac,
            ServiceAudioQualityChoice.BestOfAnyCodec => AudioQualityChoice.BestOfAnyCodec,
            ServiceAudioQualityChoice.SmallestAac => AudioQualityChoice.SmallestAac,
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "No audio quality choice matches this service audio quality choice."),
        };
    }
}
