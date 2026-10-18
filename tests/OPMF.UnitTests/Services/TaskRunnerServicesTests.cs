using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class TaskRunnerServicesTests
    {
        [Fact]
        public void StartTaskRunnerProcessThatDownloadsVideoWithSiteIdPassesTheSiteIdToTheRepository()
        {
            var fakeTaskRunnerProcessRepository = new FakeTaskRunnerProcessRepository();
            var taskRunnerServices = new TaskRunnerServices(fakeTaskRunnerProcessRepository);

            taskRunnerServices.StartTaskRunnerProcessThatDownloadsVideoWithSiteId("abc123");

            Assert.Equal(new[] { "abc123" }, fakeTaskRunnerProcessRepository.VideoSiteIdsTheTaskRunnerWasStartedFor);
        }
    }
}
