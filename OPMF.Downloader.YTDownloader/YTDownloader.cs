﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NYoutubeDL;

namespace OPMF.Downloader.YTDownloader
{
    public class YTDownloader : IDownloader<Entities.IMetadata>
    {
        public YTDownloader()
        {
            // Setup YoutubeDL
            YTDLSetup.EstablishYoutubeDLExec();
            YoutubeDL youtubeDL = new YoutubeDL();
            youtubeDL.Options.GeneralOptions.Update = Settings.ConfigHelper.Config.YoutubeDL.CheckForBinaryUpdates;

            youtubeDL.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            youtubeDL.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
        }

        private class DownloadInstance
        {
            private YoutubeDL __youtubeDL;
            private string __downloadError;

            public bool NotDownloading { get; set; } = true;

            public DownloadInstance()
            {
                __youtubeDL = new YoutubeDL();
                __youtubeDL.Options.SubtitleOptions.AllSubs = Settings.ConfigHelper.Config.YoutubeDL.GetSubtitles;
                __youtubeDL.Options.VideoFormatOptions.Format = NYoutubeDL.Helpers.Enums.VideoFormat.best;
                __youtubeDL.YoutubeDlPath = Settings.ConfigHelper.ReadonlySettings.GetYoutubeDLPath();

                __youtubeDL.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
                __youtubeDL.StandardErrorEvent += (sender, errorOutput) => __downloadError = errorOutput;
            }

            public void Download(Entities.IMetadata metadata)
            {
                NotDownloading = false;
                __downloadError = null;
                __youtubeDL.Options.FilesystemOptions.Output = Path.Join(Settings.ConfigHelper.ReadonlySettings.GetDownloadFolderPath(),
                                                                         __ItemNameSanitizer(metadata.Title) + "." + Settings.ConfigHelper.Config.YoutubeDL.VideoExtension);

                Thread thread = new Thread(() =>
                {
                    Console.WriteLine("Downloading: " + metadata.Title);
                    __youtubeDL.Download(metadata.Url);
                    if (string.IsNullOrEmpty(__downloadError))
                    {
                        metadata.Status = Entities.MetadataStatus.Downloaded;
                    }
                    else
                    {
                        Console.WriteLine("Error: " + __downloadError);
                    }
                    NotDownloading = true;
                });
                thread.Start();
            }

            private string __ItemNameSanitizer(string itemName)
            {
                string sanitizeredItemName = itemName.Replace("/", "forward slash");
                sanitizeredItemName = sanitizeredItemName.Replace("\"", "''");
                sanitizeredItemName = sanitizeredItemName.Replace("%", "percent");

                return sanitizeredItemName;
            }
        }

        public void Download(List<Entities.IMetadata> items)
        {
            int numOfInstances = 5;
            List<DownloadInstance> instances = new List<DownloadInstance>();
            for (int i = 0; i < numOfInstances; i++)
            {
                instances.Add(new DownloadInstance());
            }

            int currentMetadataIndex = 0;
            while (currentMetadataIndex < items.Count || instances.Count > 0)
            {
                // Needs to decrement as we are removing elements from instances
                for (int i = instances.Count - 1; i >= 0; i--)
                {
                    if (instances[i].NotDownloading)
                    {
                        if (currentMetadataIndex < items.Count)
                        {
                            instances[i].Download(items[currentMetadataIndex]);
                            currentMetadataIndex++;
                        }
                        else
                        {
                            instances.Remove(instances[i]);
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }
    }
}
