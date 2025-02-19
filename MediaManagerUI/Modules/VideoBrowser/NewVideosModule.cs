using MediaManager.Services2;
using OPMF.Entities;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class NewVideosModule(
        IMetadataServices metadataServices,
        IChannelMetadataServices channelMetadataServices) : VideoBrowserModuleBase(metadataServices), IVideoBrowserModule
    {
        private readonly IChannelMetadataServices _channelMetadataServices = channelMetadataServices;

        public MetadataStatus[] UnselectableMetadataStatuses => [MetadataStatus.Downloaded];

        public async Task GetResultsAsync()
        {
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
                        resultsChuck = _channelMetadataServices.GetNew(_skip, _pageSize);
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
