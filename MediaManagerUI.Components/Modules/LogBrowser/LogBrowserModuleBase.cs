using MediaManager.Dtos.Application;

namespace MediaManagerUI.Modules.LogBrowser
{
    public class LogBrowserModuleBase
    {
        public bool IsLoading { get; protected set; } = false;
        public List<ApplicationLogEntryDto> Results { get; protected set; } = [];
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
