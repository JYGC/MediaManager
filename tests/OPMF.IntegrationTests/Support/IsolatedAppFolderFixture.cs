using System;
using System.IO;
using OPMF.Database;
using OPMF.Filesystem;
using OPMF.Settings;
using OPMF.TextLogging;

namespace OPMF.IntegrationTests.Support
{
    public class IsolatedAppFolderSettings : ReadonlySettings
    {
        public IsolatedAppFolderSettings(string folderName)
        {
            _appFolderName = folderName;
        }
    }

    public class IsolatedAppFolderFixture : IDisposable
    {
        public string AppFolderName { get; }
        public IsolatedAppFolderSettings ReadonlySettings { get; }
        public AppConfig AppConfig { get; }
        public IDatabaseConnectionOpener DatabaseConnectionOpener { get; }

        public IsolatedAppFolderFixture()
        {
            AppFolderName = "OPMFIntTest_" + Guid.NewGuid().ToString("N");
            ReadonlySettings = new IsolatedAppFolderSettings(AppFolderName);
            new MissingFolderCreator().CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(ReadonlySettings);
            AppConfig = new ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing().ReadConfigFileOrWriteDefaultConfigFileWhenMissing(ReadonlySettings.GetConfigFilePath());
            DatabaseConnectionOpener = new SharedModeDatabaseFileConnectionOpener(
                ReadonlySettings.GetDatabasePath(),
                new TextLog(ReadonlySettings.GetTextLogFile(), TimeProvider.System));
        }

        public void Dispose()
        {
            var path = ReadonlySettings.GetLocalAppFolderPath();
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch (IOException)
            {
            }
        }
    }
}
