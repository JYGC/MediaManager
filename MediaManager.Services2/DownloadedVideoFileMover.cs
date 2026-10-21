namespace MediaManager.Services2;

public interface IDownloadedVideoFileMover
{
    void MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(string sourceFolderPath, string destinationFolderPath, IEnumerable<string> fileExtensionsToMove);
}

public class DownloadedVideoFileMover : IDownloadedVideoFileMover
{
    public void MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(string sourceFolderPath, string destinationFolderPath, IEnumerable<string> fileExtensionsToMove)
    {
        IEnumerable<string> sourceFilePaths = Array.Empty<string>();

        foreach (string fileExtension in fileExtensionsToMove)
        {
            sourceFilePaths = sourceFilePaths.Concat(Directory.GetFiles(sourceFolderPath).Where(filePath => filePath.EndsWith("." + fileExtension) && !DownloadedFileNameRules.IsNameOfStreamFileLeftBeforeMerging(Path.GetFileName(filePath))));
        }

        foreach (string sourceFilePath in sourceFilePaths)
        {
            FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
            string destinationFilePath = Path.Join(destinationFolderPath, sourceFileInfo.Name);
            if (!new FileInfo(destinationFilePath).Exists)
            {
                sourceFileInfo.MoveTo(destinationFilePath);
            }
        }
    }
}
