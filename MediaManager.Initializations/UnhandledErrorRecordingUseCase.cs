using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface IUnhandledErrorRecordingUseCase
{
    void RecordUnhandledError(Exception unhandledException);
}

public class UnhandledErrorRecordingUseCase(ILogServices logServices) : IUnhandledErrorRecordingUseCase
{
    public void RecordUnhandledError(Exception unhandledException)
    {
        logServices.RecordError(unhandledException.Message, unhandledException.ToString());
    }
}
