using MediaManager.Services2;
using OPMF.Entities;

namespace MediaManagerUI.Modules.ChannelBrowser
{
    public class AllChannelsModule : IChannelBrowserModule
    {
        private readonly IChannelServices _channelServices;

        public bool IsLoading { get; protected set; } = false;
        public List<Channel> Results { get; protected set; } = [];

        public void ScheduleChannelBacklistChange(Channel channel)
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
                    Results = _channelServices.GetAll();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        public AllChannelsModule(IChannelServices channelServices)
        {
            _channelServices = channelServices;

            _timer = new System.Timers.Timer(_timeBetweenInterval);
            _timer.Elapsed += UpdateChannelStatuses;
            _timer.Enabled = true;
        }

        private readonly List<Channel> _channelStatusUpdateQueue = [];

        private void UpdateChannelStatuses(object? source, System.Timers.ElapsedEventArgs e)
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
                _channelServices.UpdateBlackListStatus(channels);
            }
        }
        private const int _timeBetweenInterval = 1000;
        private readonly System.Timers.Timer _timer;
    }
}
