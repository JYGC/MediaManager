using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NYoutubeDL;

namespace OPMF.Downloader.YTDownloader
{
    public class YTDownloader : IDownloader
    {
        private List<DownloadInstance> __downloadInstances;
        public List<Entities.Metadata> DownloadQueue { get; set; }

        public YTDownloader()
        {
            // Setup YoutubeDL
            YTDLSetup.EstablishYoutubeDLExec();
            YoutubeDL youtubeDL = new YoutubeDL();
            youtubeDL.Options.GeneralOptions.Update = Settings.ConfigHelper.Config.YoutubeDL.CheckForBinaryUpdates;

            youtubeDL.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            youtubeDL.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
        }

        public void StartDownloadingQueue()
        {
            // Create download instance
            __downloadInstances = new List<DownloadInstance>();
            int noOfInstances = (
                DownloadQueue.Count < Settings.ConfigHelper.Config.YoutubeDL.MaxNoOfParallelDownloads
            ) ? DownloadQueue.Count : Settings.ConfigHelper.Config.YoutubeDL.MaxNoOfParallelDownloads;
            for (int i = 0; i < noOfInstances; i++)
            {
                __downloadInstances.Add(new DownloadInstance() { ScreenPosition = i });
            }

            int currentMetadataIndex = 0;
            while (currentMetadataIndex < DownloadQueue.Count || __downloadInstances.Count > 0)
            {
                // Needs to decrement as we are removing elements from instances
                for (int i = __downloadInstances.Count - 1; i >= 0; i--)
                {
                    // Leave instances alone that are still downloading
                    if (__downloadInstances[i].NotDownloading ||
                        DateTime.Now.Subtract(__downloadInstances[i].StartDateTime).TotalSeconds > Settings.ConfigHelper.Config.StopDownloadInstanceAfterSeconds)
                    {
                        __downloadInstances[i].StopDownloadThread();
                        if (currentMetadataIndex < DownloadQueue.Count)
                        {
                            __downloadInstances[i].Download(DownloadQueue[currentMetadataIndex]);
                            currentMetadataIndex++;
                        }
                        else
                        {
                            // When there is no more videos to download, remove instances that are not downloading
                            __downloadInstances[i].CleanUpInstance();
                            __downloadInstances.Remove(__downloadInstances[i]);
                        }
                    }
                }
                Thread.Sleep(5000); // replace this
            }
        }
    }

    public class DownloadInstance
    {
        private const int TITLE_DISPLAY_LENGTH = 40;

        private Thread __thread;
        private YoutubeDL __youtubeDL;
        private string __downloadError;
        private string __title;

        public DateTime StartDateTime { get; } = DateTime.Now; // Rough fix for frozen DownloadInstance
        public bool NotDownloading { get; set; } = true;
        public int ScreenPosition { get; set; }

        public DownloadInstance()
        {
            __youtubeDL = new YoutubeDL();
            __youtubeDL.Options.SubtitleOptions.AllSubs = Settings.ConfigHelper.Config.YoutubeDL.GetSubtitles;
            __youtubeDL.Options.VideoFormatOptions.Format = NYoutubeDL.Helpers.Enums.VideoFormat.best;
            __youtubeDL.YoutubeDlPath = Settings.ConfigHelper.ReadonlySettings.GetYoutubeDLPath();

            __youtubeDL.StandardOutputEvent += (sender, output) =>
            {
                WriteOnLine(ScreenPosition, output.StartsWith("[download]") ? string.Format("{0} Video: {1}...", output, __TruncateStrIfTooLong(__title)) : output);
            };
            __youtubeDL.StandardErrorEvent += (sender, errorOutput) => __downloadError = errorOutput;
        }

        public void CleanUpInstance()
        {
            WriteOnLine(ScreenPosition, "download instance not needed. removed...");
        }

        public void Download(Entities.Metadata metadata)
        {
            NotDownloading = false;
            __downloadError = null;
            __youtubeDL.Options.FilesystemOptions.Output = Path.Join(Settings.ConfigHelper.ReadonlySettings.GetDownloadFolderPath(),
                                                                     __ItemNameSanitizer(metadata.Title) + "." + Settings.ConfigHelper.Config.YoutubeDL.VideoExtension);
            __thread = new Thread(() =>
            {
                __title = metadata.Title;
                try
                {
                    __youtubeDL.Download(metadata.Url);
                    if (string.IsNullOrEmpty(__downloadError) ||
                        Settings.ConfigHelper.Config.YoutubeDL.SafeMessages.Contains(__downloadError.Trim()))
                    {
                        metadata.Status = Entities.MetadataStatus.Downloaded;
                    }
                    else
                    {
                        Logging.Logger.GetCurrent().LogEntry(new Entities.OPMFLog
                        {
                            Message = __downloadError,
                            Type = Entities.OPMFLogType.Warning,
                            Variables = new Dictionary<string, object>
                            {
                                { "metadata.Title", __title },
                                { "metadata.Url", metadata.Url }
                            }
                        });
                        WriteOnLine(ScreenPosition, "An error has occured. See error log...");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Logger.GetCurrent().LogEntry(new Entities.OPMFError(ex)
                    {
                        Variables = new Dictionary<string, object>
                        {
                            { "metadata.Title", __title },
                            { "metadata.Url", metadata.Url }
                        }
                    });
                }
                NotDownloading = true;
            });
            __thread.Start();
        }

        public void WriteOnLine(int position, string message)
        {
            Console.SetCursorPosition(0, position);
            Console.WriteLine(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, position);
            Console.WriteLine(message);
        }

        public void StopDownloadThread()
        {
            if (__thread != null) __thread.Interrupt();
        }

        private string __TruncateStrIfTooLong(string str)
        {
            return str.Length <= TITLE_DISPLAY_LENGTH ? str : str.Substring(0, TITLE_DISPLAY_LENGTH);
        }

        private string __ItemNameSanitizer(string itemName)
        {
            string sanitizeredItemName = itemName.Replace("/", "forward slash");
            sanitizeredItemName = sanitizeredItemName.Replace("\"", "''");
            sanitizeredItemName = sanitizeredItemName.Replace("%", "percent");

            return sanitizeredItemName;
        }
    }
}
