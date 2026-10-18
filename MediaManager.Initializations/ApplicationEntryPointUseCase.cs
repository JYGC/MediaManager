namespace MediaManager.Initializations;

public interface IApplicationEntryPointUseCase
{
    void RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(Action runApplicationAfterStartup);
}

public class ApplicationEntryPointUseCase(
    IApplicationStartupUseCase applicationStartupUseCase,
    Lazy<IUnhandledErrorRecordingUseCase> unhandledErrorRecordingUseCaseCreatedOnlyWhenAnErrorMustBeRecorded,
    TextWriter standardOutput) : IApplicationEntryPointUseCase
{
    public void RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(Action runApplicationAfterStartup)
    {
        try
        {
            applicationStartupUseCase.InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig();
            runApplicationAfterStartup();
        }
        catch (Exception unhandledException)
        {
            unhandledErrorRecordingUseCaseCreatedOnlyWhenAnErrorMustBeRecorded.Value.RecordUnhandledError(unhandledException);
            standardOutput.WriteLine(unhandledException.Message);
        }
    }
}
