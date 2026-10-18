using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class FindVideosModule(IVideoBrowsingUseCases videoBrowsingUseCases, TimeProvider timeProvider)
        : VideoBrowserModuleBase(videoBrowsingUseCases, timeProvider), IVideoBrowserModule, IDatabaseSearchableVideoBrowserModule
    {
        public string? SearchVideoTitle { get; set; }
        public string? SearchChannelName { get; set; }

        public ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses => [ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded];

        public async Task GetResultsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchVideoTitle) && string.IsNullOrWhiteSpace(SearchChannelName))
            {
                return;
            }

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
                        resultsChuck = string.IsNullOrWhiteSpace(SearchChannelName)
                            ? _videoBrowsingUseCases.GetVideosWithChannelsWhereVideoTitleContainsWord(SearchVideoTitle!, _skip, _pageSize)
                            : _videoBrowsingUseCases.GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(SearchChannelName, SearchVideoTitle!,
                                _skip, _pageSize);
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
