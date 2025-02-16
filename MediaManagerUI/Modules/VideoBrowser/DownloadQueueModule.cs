﻿using MediaManager.Services2;
using OPMF.Entities;

namespace MediaManagerUI.Modules.VideoBrowser
{
    public class DownloadQueueModule(
        IMetadataServices metadataServices,
        IChannelMetadataServices channelMetadataServices) : VideoBrowserModuleBase(metadataServices), IVideoBrowserModule
    {
        private readonly IChannelMetadataServices _channelMetadataServices = channelMetadataServices;

        public MetadataStatus[] UnselectableMetadataStatuses => [MetadataStatus.New, MetadataStatus.Downloaded];

        public async Task GetResultsAsync()
        {
            Results = [];
            _skip = 0;

            List<ChannelMetadata>? resultsChuck = null;

            IsLoading = true;
            await Task.Run(() =>
            {
                do
                {
                    resultsChuck = _channelMetadataServices.GetToDownloadAndWait(_skip, _pageSize);
                    Results.AddRange(resultsChuck);
                    _skip += _pageSize;
                }
                while (resultsChuck != null && resultsChuck.Count() == _pageSize);
                _metadataIdResultsMap = Results.ToDictionary(cm => cm.Metadata.Id, cm => cm.Metadata);
            });

            IsLoading = false;
        }
    }
}
