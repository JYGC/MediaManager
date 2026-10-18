using System.IO;
using OPMF.Settings;

namespace OPMF.UnitTests.Support
{
    public class TemporaryFolderReadonlySettings : IReadonlySettings
    {
        private readonly string _appFolderPath;

        public TemporaryFolderReadonlySettings(string temporaryFolderPath)
        {
            _appFolderPath = Path.Join(temporaryFolderPath, "OffPeakMediaFetcherAppFolder");
        }

        public string GetLocalAppFolderPath() => _appFolderPath;
        public string GetCredentialPath() => Path.Join(_appFolderPath, "Token.json");
        public string GetConfigFilePath() => Path.Join(_appFolderPath, "config.json");
        public string GetDownloadFolderPath() => Path.Join(_appFolderPath, "Downloads");
        public string GetDatabaseFolderPath() => Path.Join(_appFolderPath, "Databases");
        public string GetDatabasePath() => Path.Join(GetDatabaseFolderPath(), "OPMF.db");
        public string GetBinFolderPath() => Path.Join(_appFolderPath, "Bin");
        public string GetYoutubeDLPath() => Path.Join(GetBinFolderPath(), "youtube-dl.exe");
        public string GetYtDlpExecutablePath() => Path.Join(GetBinFolderPath(), "yt-dlp.exe");
        public string GetTextLogFile() => Path.Join(_appFolderPath, "log.txt");
    }
}
