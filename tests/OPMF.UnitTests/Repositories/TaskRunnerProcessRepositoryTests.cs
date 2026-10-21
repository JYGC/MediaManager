using MediaManager.Repositories;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Repositories
{
    public class TaskRunnerProcessRepositoryTests
    {
        [Fact]
        public void TheTaskRunnerExecutableIsStartedWithTheVideosArgumentAndTheSiteId()
        {
            var externalProcessStarter = new RecordingExternalProcessStarter();
            var taskRunnerProcessRepository = new TaskRunnerProcessRepository(externalProcessStarter);

            taskRunnerProcessRepository.StartTaskRunnerProcessThatDownloadsVideoWithSiteId("abc123");

            var processStarted = Assert.Single(externalProcessStarter.ProcessesStarted);
            Assert.Equal("OffPeakMediaFetcher.exe", processStarted.ExecutableFileName);
            Assert.Equal("videos abc123", processStarted.Arguments);
        }
    }
}
