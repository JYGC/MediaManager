using System.Collections.Generic;
using MediaManager.Repositories;

namespace OPMF.UnitTests.Support
{
    public record ExternalProcessStart(string ExecutableFileName, string Arguments);

    public class RecordingExternalProcessStarter : IExternalProcessStarter
    {
        public List<ExternalProcessStart> ProcessesStarted { get; } = new List<ExternalProcessStart>();

        public void StartProcessWithExecutableFileNameAndArguments(string executableFileName, string arguments)
        {
            ProcessesStarted.Add(new ExternalProcessStart(executableFileName, arguments));
        }
    }
}
