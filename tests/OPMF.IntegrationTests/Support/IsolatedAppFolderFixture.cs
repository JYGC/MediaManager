using System;
using System.IO;
using OPMF.Settings;

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

        public IsolatedAppFolderFixture()
        {
            AppFolderName = "OPMFIntTest_" + Guid.NewGuid().ToString("N");
            ConfigHelper.ReadonlySettings = new IsolatedAppFolderSettings(AppFolderName);
            OPMF.Filesystem.FolderSetup.EstablishFolders();
            ConfigHelper.EstablishConfig();
        }

        public void Dispose()
        {
            var path = ConfigHelper.ReadonlySettings.GetLocalAppFolderPath();
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
