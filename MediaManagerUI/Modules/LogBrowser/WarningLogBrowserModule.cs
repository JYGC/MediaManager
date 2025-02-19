using MediaManager.Services2;

namespace MediaManagerUI.Modules.LogBrowser
{
    public class WarningLogBrowserModule(ILogServices _logServices) : LogBrowserModuleBase, ILogBrowserModule
    {
        public async Task GetResultsAsync(DateTime startDateTime, DateTime endDateTime)
        {
            Results = [];

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Results = _logServices.GetWarnings(startDateTime, endDateTime);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
