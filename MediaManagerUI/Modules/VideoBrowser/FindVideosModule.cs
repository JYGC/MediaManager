using MediaManager.Services2;
using OPMF.Entities;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class FindVideosModule(
        IMetadataServices metadataServices,
        IChannelMetadataServices channelMetadataServices) : VideoBrowserModuleBase(metadataServices), IVideoBrowserModule, IDatabaseSearchableVideoBrowserModule
    {
        private readonly IChannelMetadataServices _channelMetadataServices = channelMetadataServices;

        public string? SearchVideoTitle { get; set; }
        public string? SearchChannelName { get; set; }

        public MetadataStatus[] UnselectableMetadataStatuses => [ MetadataStatus.New, MetadataStatus.Downloaded ];

        public async Task GetResultsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchVideoTitle) && string.IsNullOrWhiteSpace(SearchChannelName))
            {
                return;
            }

            Results = [];
            _skip = 0;

            List<ChannelMetadata>? resultsChuck = null;

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    do
                    {
                        resultsChuck = string.IsNullOrWhiteSpace(SearchChannelName)
                            ? _channelMetadataServices.GetByTitleContainingWord(SearchVideoTitle, _skip, _pageSize)
                            : _channelMetadataServices.GetByChannelAndTitleContainingWord(SearchChannelName, SearchVideoTitle,
                                _skip, _pageSize);
                        Results.AddRange(resultsChuck);
                        _skip += _pageSize;
                    }
                    while (resultsChuck != null && resultsChuck.Count() == _pageSize);
                    _metadataIdResultsMap = Results.ToDictionary(cm => cm.Metadata.Id, cm => cm.Metadata);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
