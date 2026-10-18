using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface ICommandLineTaskUseCases
{
    void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();
    void DownloadEveryVideoQueuedToDownload();
    void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt);
}

public class CommandLineTaskUseCases(
    INewVideoMetadataFetchServices newVideoMetadataFetchServices,
    IVideoDownloadServices videoDownloadServices) : ICommandLineTaskUseCases
{
    public void FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted()
    {
        newVideoMetadataFetchServices.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();
    }

    public void DownloadEveryVideoQueuedToDownload()
    {
        videoDownloadServices.DownloadEveryVideoQueuedToDownload();
    }

    public void DownloadFirstVideoWhoseSiteIdContains(string siteIdOrPartOfIt)
    {
        videoDownloadServices.DownloadFirstVideoWhoseSiteIdContains(siteIdOrPartOfIt);
    }
}
