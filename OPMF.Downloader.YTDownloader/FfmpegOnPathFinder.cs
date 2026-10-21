using System;
using System.IO;
using System.Linq;

namespace OPMF.Downloader.YTDownloader
{
    public interface IFfmpegOnPathFinder
    {
        bool IsFfmpegFoundInAFolderOnPath();
    }

    public class FfmpegOnPathFinder(string pathEnvironmentVariableValue, Func<string, bool> fileExists) : IFfmpegOnPathFinder
    {
        private const string FfmpegFileName = "ffmpeg.exe";

        public bool IsFfmpegFoundInAFolderOnPath()
        {
            return pathEnvironmentVariableValue
                .Split(';')
                .Select(folderOnPath => folderOnPath.Trim().Trim('"').Trim())
                .Where(folderOnPath => folderOnPath.Length > 0)
                .Any(folderOnPath => fileExists(Path.Join(folderOnPath, FfmpegFileName)));
        }
    }
}
