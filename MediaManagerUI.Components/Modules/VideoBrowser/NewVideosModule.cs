using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class NewVideosModule(IVideoBrowsingUseCases videoBrowsingUseCases, TimeProvider timeProvider)
        : VideoBrowserModuleBase(videoBrowsingUseCases, timeProvider), IVideoBrowserModule
    {
        public ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses => [ApplicationVideoDownloadStatus.Downloaded];

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
                        resultsChuck = _videoBrowsingUseCases.GetNewVideosWithChannels(_skip, _pageSize);
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
