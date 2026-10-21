using System;
using System.Collections.Generic;
using System.IO;

using Google.Apis.YouTube.v3.Data;

using Newtonsoft.Json;

namespace OPMF.SiteAdapter.Youtube
{
    public class YoutubeMetadataFetcher : IMetadataFetcher<Entities.Channel, Entities.Metadata>
    {
        private readonly string __videoUrlFormat = "https://www.youtube.com/watch?v={0}";

        private readonly IYoutubeDataApi __youtubeDataApi;
        private readonly int __newChannelPastVideoDayLimit;
        private readonly TimeProvider __timeProvider;
        private readonly Logging.Logger __logger;
        private readonly TextWriter __standardOutput;

        public YoutubeMetadataFetcher(IYoutubeDataApi youtubeDataApi, int newChannelPastVideoDayLimit, TimeProvider timeProvider, Logging.Logger logger, TextWriter standardOutput)
        {
            __youtubeDataApi = youtubeDataApi;
            __newChannelPastVideoDayLimit = newChannelPastVideoDayLimit;
            __timeProvider = timeProvider;
            __logger = logger;
            __standardOutput = standardOutput;
        }

        public List<Entities.Metadata> FetchMetadata(ref List<Entities.Channel> channels)
        {
            List<Entities.Metadata> fetchedVideoMetadatas = new List<Entities.Metadata>();
            foreach (Entities.YoutubeChannel channel in channels)
            {
                __standardOutput.WriteLine("fetching video metadata for youtube channel: " + channel.Name);
                DateTime publishedAfter = channel.LastCheckedOut.HasValue ? channel.LastCheckedOut.Value.AddDays(-2) : Now().AddDays(-__newChannelPastVideoDayLimit);
                bool channelWasCheckedWithoutAnyActivityFailing = true;
                DateTime? lastActivityDate = channel.LastActivityDate;
                DateTime timeTheLastPageWasFetched;
                string nextPageToken = null;
                do
                {
                    ActivityListResponse response = __youtubeDataApi.ListOnePageOfActivitiesOfChannelPublishedAfter(channel.SiteId, publishedAfter, nextPageToken);
                    timeTheLastPageWasFetched = Now();
                    IList<Activity> activities = response.Items;
                    foreach (Activity activity in activities)
                    {
                        try
                        {
                            if (activity.ContentDetails.Upload == null)
                            {
                                __standardOutput.WriteLine("no video id detected. skip");
                            }
                            else
                            {
                                DateTime publishedAt = Convert.ToDateTime(activity.Snippet.PublishedAt);
                                __standardOutput.WriteLine("fetched: " + activity.Snippet.Title);
                                fetchedVideoMetadatas.Add(new Entities.YoutubeMetadata()
                                {
                                    SiteId = activity.ContentDetails.Upload.VideoId,
                                    Url = string.Format(__videoUrlFormat, activity.ContentDetails.Upload.VideoId),
                                    Title = activity.Snippet.Title,
                                    Thumbnail = new Entities.EntityThumbnail
                                    {
                                        Url = activity.Snippet.Thumbnails.Default__.Url,
                                        Width = activity.Snippet.Thumbnails.Default__.Width.Value,
                                        Height = activity.Snippet.Thumbnails.Default__.Height.Value,
                                    },
                                    Description = activity.Snippet.Description,
                                    ChannelSiteId = activity.Snippet.ChannelId,
                                    PublishedAt = publishedAt
                                });

                                lastActivityDate = (!lastActivityDate.HasValue || lastActivityDate.Value.CompareTo(publishedAt) == -1) ? publishedAt : lastActivityDate;
                            }
                        }
                        catch (Exception e)
                        {
                            channelWasCheckedWithoutAnyActivityFailing = false;
                            __standardOutput.WriteLine("exception thrown: " + e.Message + "\n" +
                                              "object json:\n" +
                                              JsonConvert.SerializeObject(activity) + "\n" +
                                              "skipping channel");
                            __logger.LogEntry(new Entities.OPMFLog
                            {
                                Message = "skipping channel: " + e.Message,
                                DateCreated = Now(),
                                Variables = new Dictionary<string, object>
                                {
                                    { "activity object json", JsonConvert.SerializeObject(activity) },
                                    { "exception thrown", e },
                                    { "nextPageToken", nextPageToken }
                                },
                                Type = Entities.OPMFLogType.Info
                            });
                        }
                    }
                    nextPageToken = response.NextPageToken;
                }
                while (nextPageToken != null);

                if (channelWasCheckedWithoutAnyActivityFailing)
                {
                    channel.LastCheckedOut = timeTheLastPageWasFetched;
                    channel.LastActivityDate = lastActivityDate;
                }
            }

            return fetchedVideoMetadatas;
        }

        private DateTime Now()
        {
            return __timeProvider.GetLocalNow().LocalDateTime;
        }
    }
}
