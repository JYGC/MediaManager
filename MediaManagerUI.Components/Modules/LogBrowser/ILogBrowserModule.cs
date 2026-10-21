using MediaManager.Dtos.Application;

namespace MediaManagerUI.Modules.LogBrowser
{
    public interface ILogBrowserModule
    {
        bool IsLoading { get; }
        List<ApplicationLogEntryDto> Results { get; }
        Task GetResultsAsync(DateTime startDateTime, DateTime endDateTime);
    }
}
