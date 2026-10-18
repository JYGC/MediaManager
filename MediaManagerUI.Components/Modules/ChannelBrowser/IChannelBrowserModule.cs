using MediaManager.Dtos.Application;

namespace MediaManagerUI.Modules.ChannelBrowser
{
    public interface IChannelBrowserModule
    {
        bool IsLoading { get; }
        List<ApplicationChannelDto> Results { get; }
        void ScheduleChannelBacklistChange(ApplicationChannelDto channel);
        Task GetResultsAsync();
    }
}
