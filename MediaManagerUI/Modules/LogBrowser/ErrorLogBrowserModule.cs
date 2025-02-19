using MediaManager.Services2;

namespace MediaManagerUI.Modules.LogBrowser
{
    public class ErrorLogBrowserModule(ILogServices _logServices) : LogBrowserModuleBase, ILogBrowserModule
    {
        public async Task GetResultsAsync(DateTime startDateTime, DateTime endDateTime)
        {
            Results = [];

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Results = _logServices.GetErrors(startDateTime, endDateTime);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
