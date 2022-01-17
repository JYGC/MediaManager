﻿using System.Collections.Generic;

namespace OPMF.Downloader
{
    public interface IDownloader<TItem> where TItem : Entities.IId
    {
        void Download(List<TItem> items);
    }
}
