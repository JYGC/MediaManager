using System;
using System.IO;
using OPMF.Settings;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class ReadonlySettingsTests
    {
        [Fact]
        public void LocalAppFolder_IsUnderLocalAppData_AndNamedOffPeakMediaFetcher()
        {
            var s = new ReadonlySettings();
            var path = s.GetLocalAppFolderPath();
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Assert.StartsWith(localAppData, path);
            Assert.EndsWith("OffPeakMediaFetcher", path);
        }

        [Fact]
        public void CredentialPath_IsTokenJsonUnderAppFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "Token.json"), s.GetCredentialPath());
        }

        [Fact]
        public void ConfigFilePath_IsConfigJsonUnderAppFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "config.json"), s.GetConfigFilePath());
        }

        [Fact]
        public void DownloadFolder_IsDownloadsUnderAppFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "Downloads"), s.GetDownloadFolderPath());
        }

        [Fact]
        public void DatabasePath_IsDatabaseFileUnderDatabaseFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetDatabaseFolderPath(), "OPMF.db"), s.GetDatabasePath());
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "Databases"), s.GetDatabaseFolderPath());
        }

        [Fact]
        public void BinFolder_AndYoutubeDLPath_UnderAppFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "Bin"), s.GetBinFolderPath());
            Assert.Equal(Path.Join(s.GetBinFolderPath(), "youtube-dl.exe"), s.GetYoutubeDLPath());
        }

        [Fact]
        public void TextLogFile_IsLogTxtUnderAppFolder()
        {
            var s = new ReadonlySettings();
            Assert.Equal(Path.Join(s.GetLocalAppFolderPath(), "log.txt"), s.GetTextLogFile());
        }

        [Fact]
        public void DevSettings_AppendsDev()
        {
            var s = new ReadOnlyDevSettings();
            Assert.EndsWith("OffPeakMediaFetcherDev", s.GetLocalAppFolderPath());
        }

        [Fact]
        public void TestSettings_AppendsTest()
        {
            var s = new ReadOnlyTestSettings();
            Assert.EndsWith("OffPeakMediaFetcherTest", s.GetLocalAppFolderPath());
        }

        [Fact]
        public void AllPathsDerive_FromAppFolderName()
        {
            var s = new ReadOnlyTestSettings();
            var appFolder = s.GetLocalAppFolderPath();
            Assert.StartsWith(appFolder, s.GetConfigFilePath());
            Assert.StartsWith(appFolder, s.GetCredentialPath());
            Assert.StartsWith(appFolder, s.GetDownloadFolderPath());
            Assert.StartsWith(appFolder, s.GetDatabaseFolderPath());
            Assert.StartsWith(appFolder, s.GetDatabasePath());
            Assert.StartsWith(appFolder, s.GetBinFolderPath());
            Assert.StartsWith(appFolder, s.GetYoutubeDLPath());
            Assert.StartsWith(appFolder, s.GetTextLogFile());
        }
    }
}
