using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.ChannelBrowser
{
    public class AllChannelsModule : IChannelBrowserModule
    {
        private readonly IChannelBrowsingUseCases _channelBrowsingUseCases;

        public bool IsLoading { get; protected set; } = false;
        public List<ApplicationChannelDto> Results { get; protected set; } = [];

        public void ScheduleChannelBacklistChange(ApplicationChannelDto channel)
        {
            _channelStatusUpdateQueue.Add(channel);
        }

        public async Task GetResultsAsync()
        {
            Results = [];

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Results = _channelBrowsingUseCases.GetAllChannels();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        public AllChannelsModule(IChannelBrowsingUseCases channelBrowsingUseCases, TimeProvider timeProvider)
        {
            _channelBrowsingUseCases = channelBrowsingUseCases;

            _timer = timeProvider.CreateTimer(UpdateChannelStatuses, null, _timeBetweenInterval, _timeBetweenInterval);
        }

        private readonly List<ApplicationChannelDto> _channelStatusUpdateQueue = [];

        private void UpdateChannelStatuses(object? state)
        {
            if (_channelStatusUpdateQueue.Count == 0)
            {
                return;
            }
            else
            {
                var channelStatusUpdateQueueSize = _channelStatusUpdateQueue.Count;
                var channels = _channelStatusUpdateQueue.GetRange(0, channelStatusUpdateQueueSize);
                _channelStatusUpdateQueue.RemoveRange(0, channelStatusUpdateQueueSize);
                _channelBrowsingUseCases.UpdateBlacklistFlagOfChannels(channels);
            }
        }
        private static readonly TimeSpan _timeBetweenInterval = TimeSpan.FromSeconds(1);
        private readonly ITimer _timer;
    }
}
