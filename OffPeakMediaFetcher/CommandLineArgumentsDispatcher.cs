using MediaManager.Initializations;

namespace OffPeakMediaFetcher
{
    public class CommandLineArgumentsDispatcher
    {
        private readonly ICommandLineTaskUseCases _commandLineTaskUseCases;

        public CommandLineArgumentsDispatcher(ICommandLineTaskUseCases commandLineTaskUseCases)
        {
            _commandLineTaskUseCases = commandLineTaskUseCases;
        }

        public void DispatchCommandLineArgumentsToUseCase(string[] commandLineArguments)
        {
            if (commandLineArguments.Length >= 1)
            {
                switch (commandLineArguments[0])
                {
                    case "videos":
                        if (commandLineArguments.Length == 2)
                        {
                            _commandLineTaskUseCases.DownloadFirstVideoWhoseSiteIdContains(commandLineArguments[1]);
                            break;
                        }
                        _commandLineTaskUseCases.DownloadEveryVideoQueuedToDownload();
                        break;
                    case "metadata":
                        _commandLineTaskUseCases.FetchAndStoreNewVideoMetadataForEveryChannelThatIsNotBlacklisted();
                        break;
                    default:
                        throw new System.Exception("Invalid argument.");
                }
            }
            else
            {
                throw new System.Exception("Appropriate arguments required.");
            }
        }
    }
}
