using MediaManager.Dtos.Application;
using MediaManager.Initializations;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class GetSingleModule(IVideoBrowsingUseCases videoBrowsingUseCases, TimeProvider timeProvider)
        : VideoBrowserModuleBase(videoBrowsingUseCases, timeProvider), IVideoBrowserModule, INetSearchableVideoBrowserModule
    {
        public string? SearchVideoUrl { get; set; }
        public ApplicationVideoDownloadStatus[] UnselectableMetadataStatuses => [ApplicationVideoDownloadStatus.Downloaded];

        public async Task GetResultsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchVideoUrl))
            {
                return;
            }

            Results = [];
            _skip = 0;

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Results.Add(_videoBrowsingUseCases.GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(SearchVideoUrl));
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
