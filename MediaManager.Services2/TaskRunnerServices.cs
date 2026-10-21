using MediaManager.Repositories;

namespace MediaManager.Services2
{
    public interface ITaskRunnerServices
    {
        void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId);
    }
    public class TaskRunnerServices(ITaskRunnerProcessRepository taskRunnerProcessRepository) : ITaskRunnerServices
    {
        public void StartTaskRunnerProcessThatDownloadsVideoWithSiteId(string videoSiteId)
        {
            taskRunnerProcessRepository.StartTaskRunnerProcessThatDownloadsVideoWithSiteId(videoSiteId);
        }
    }
}
