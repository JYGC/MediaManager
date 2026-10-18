using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class DownloadQueueModule(IVideoBrowsingUseCases videoBrowsingUseCases, TimeProvider timeProvider)
        : VideoBrowserModuleBase(videoBrowsingUseCases, timeProvider), IVideoBrowserModule
    {
        public ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses => [ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded];

        public async Task GetResultsAsync()
        {
            Results = [];
            _skip = 0;

            List<ApplicationVideoWithChannelDto>? resultsChuck = null;

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    do
                    {
                        resultsChuck = _videoBrowsingUseCases.GetVideosWithChannelsQueuedToDownloadOrWaiting(_skip, _pageSize);
                        Results.AddRange(resultsChuck);
                        _skip += _pageSize;
                    }
                    while (resultsChuck != null && resultsChuck.Count() == _pageSize);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
