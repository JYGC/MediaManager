namespace MediaManager.Repositories;

public interface ITaskRunnerProcessRepository
{
    void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId);
}

public class TaskRunnerProcessRepository(IExternalProcessStarter externalProcessStarter) : ITaskRunnerProcessRepository
{
    private const string TaskRunnerExecutableFileName = "OffPeakMediaFetcher.exe";

    public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId)
    {
        externalProcessStarter.StartProcessWithExecutableFileNameAndArguments(TaskRunnerExecutableFileName, $"videos {videoSiteId}");
    }
}
