using System.Diagnostics;

namespace MediaManager.Repositories;

public interface IExternalProcessStarter
{
    void StartProcessWithExecutableFileNameAndArguments(string executableFileName, string arguments);
}

public class ExternalProcessStarter : IExternalProcessStarter
{
    public void StartProcessWithExecutableFileNameAndArguments(string executableFileName, string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = executableFileName;
        process.StartInfo.Arguments = arguments;
        process.Start();
    }
}
