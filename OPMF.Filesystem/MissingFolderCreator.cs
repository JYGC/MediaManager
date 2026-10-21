using System.IO;
using OPMF.Settings;

namespace OPMF.Filesystem
{
    public interface IMissingFolderCreator
    {
        void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(IReadonlySettings readonlySettings);
        void CreateFolderWhenMissing(string folderPath);
    }

    public class MissingFolderCreator : IMissingFolderCreator
    {
        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(IReadonlySettings readonlySettings)
        {
            CreateFolderWhenMissing(readonlySettings.GetLocalAppFolderPath());
            CreateFolderWhenMissing(readonlySettings.GetDownloadFolderPath());
            CreateFolderWhenMissing(readonlySettings.GetDatabaseFolderPath());
            CreateFolderWhenMissing(readonlySettings.GetBinFolderPath());
        }

        public void CreateFolderWhenMissing(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
