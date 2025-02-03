using System;
using System.IO;

namespace OPMF.Settings
{
    public interface IReadonlySettings
    {
        string GetLocalAppFolderPath();
        string GetCredentialPath();
        string GetConfigFilePath();
        string GetDownloadFolderPath();
        string GetDatabaseFolderPath();
        string GetDatabasePath();
        string GetBinFolderPath();
        string GetYoutubeDLPath();
        string GetTextLogFile();
    }

    public class ReadonlySettings : IReadonlySettings
    {
        protected string _appFolderName = "OffPeakMediaFetcher";

        public string GetLocalAppFolderPath()
        {
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appFolderName);
        }

        public string GetCredentialPath()
        {
            return Path.Join(GetLocalAppFolderPath(), "Token.json");
        }

        public string GetConfigFilePath()
        {
            return Path.Join(GetLocalAppFolderPath(), "config.json");
        }

        public string GetDownloadFolderPath()
        {
            return Path.Join(GetLocalAppFolderPath(), "Downloads");
        }

        public string GetDatabaseFolderPath()
        {
            return Path.Join(GetLocalAppFolderPath(), "Databases");
        }

        public string GetDatabasePath()
        {
            return Path.Join(GetDatabaseFolderPath(), "OPMF.db");
        }

        public string GetBinFolderPath()
        {
            return Path.Join(GetLocalAppFolderPath(), "Bin");
        }

        public string GetYoutubeDLPath()
        {
            return Path.Join(GetBinFolderPath(), "youtube-dl.exe");
        }

        public string GetTextLogFile()
        {
            return Path.Join(GetLocalAppFolderPath(), "log.txt");
        }
    }

    public class ReadOnlyDevSettings : ReadonlySettings
    {
        public ReadOnlyDevSettings()
        {
            _appFolderName = string.Join("", _appFolderName, "Dev");
        }
    }

    public class ReadOnlyTestSettings : ReadonlySettings
    {
        public ReadOnlyTestSettings()
        {
            string candidateAppFolderName;
            var i = 0;
            do
            {
                candidateAppFolderName = string.Join("", _appFolderName, $"Test{i}");
                i++;
            }
            while (Directory.Exists(candidateAppFolderName));
            
            _appFolderName = candidateAppFolderName;
        }
    }
}
