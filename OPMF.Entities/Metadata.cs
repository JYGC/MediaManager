﻿using System;

namespace OPMF.Entities
{
    public enum MetadataStatus
    {
        New,
        ToDownload,
        Wait,
        Ignore,
        Downloaded
    }

    public class Metadata : SiteIdAsId
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public EntityThumbnail Thumbnail { get; set; } = new EntityThumbnail { Url = null, Width = 0, Height = 0 };
        public string Description { get; set; }
        public MetadataStatus Status { get; set; } = MetadataStatus.New;
        public bool IsBeingDownloaded { get; set; } = false;
        public DateTime PublishedAt { get; set; }
        public TimeSpan Duration { get; set; } // Video extension table
        public string ChannelSiteId { get; set; }
        public object OriginalResponse { get; set; }
    }
}
