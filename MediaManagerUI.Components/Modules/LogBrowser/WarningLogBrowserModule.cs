using MediaManager.Initializations;

namespace MediaManagerUI.Modules.LogBrowser
{
    public class WarningLogBrowserModule(ILogBrowsingUseCases _logBrowsingUseCases) : LogBrowserModuleBase, ILogBrowserModule
    {
        public async Task GetResultsAsync(DateTime startDateTime, DateTime endDateTime)
        {
            Results = [];

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Results = _logBrowsingUseCases.GetWarningsLoggedBetween(startDateTime, endDateTime);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
