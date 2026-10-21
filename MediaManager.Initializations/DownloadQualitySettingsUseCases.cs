using MediaManager.Dtos.Application;
using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface IDownloadQualitySettingsUseCases
{
    ApplicationDownloadQualityDto GetAppWideDownloadQuality();
    void SaveAppWideVideoCodecChoice(ApplicationVideoCodecChoice videoCodecChoice);
    void SaveAppWideMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice maximumVideoResolutionChoice);
    void SaveAppWideAudioQualityChoice(ApplicationAudioQualityChoice audioQualityChoice);
}

public class DownloadQualitySettingsUseCases(IDownloadQualitySettingsServices downloadQualitySettingsServices) : IDownloadQualitySettingsUseCases
{
    public ApplicationDownloadQualityDto GetAppWideDownloadQuality()
    {
        return ServiceDtoAndApplicationDtoConverter.ConvertDownloadQualityToApplicationDto(downloadQualitySettingsServices.GetAppWideDownloadQualityFromConfigFile());
    }

    public void SaveAppWideVideoCodecChoice(ApplicationVideoCodecChoice videoCodecChoice)
    {
        downloadQualitySettingsServices.SaveAppWideVideoCodecChoice(ServiceDtoAndApplicationDtoConverter.ConvertToServiceVideoCodecChoice(videoCodecChoice));
    }

    public void SaveAppWideMaximumVideoResolutionChoice(ApplicationMaximumVideoResolutionChoice maximumVideoResolutionChoice)
    {
        downloadQualitySettingsServices.SaveAppWideMaximumVideoResolutionChoice(ServiceDtoAndApplicationDtoConverter.ConvertToServiceMaximumVideoResolutionChoice(maximumVideoResolutionChoice));
    }

    public void SaveAppWideAudioQualityChoice(ApplicationAudioQualityChoice audioQualityChoice)
    {
        downloadQualitySettingsServices.SaveAppWideAudioQualityChoice(ServiceDtoAndApplicationDtoConverter.ConvertToServiceAudioQualityChoice(audioQualityChoice));
    }
}
