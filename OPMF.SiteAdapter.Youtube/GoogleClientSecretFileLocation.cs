using System;
using System.IO;

namespace OPMF.SiteAdapter.Youtube
{
    public static class GoogleClientSecretFileLocation
    {
        public static string GetClientSecretFilePathInFolderOfEntryExecutable(string entryAssemblyCodeBase, string googleClientSecretPathFromConfig)
        {
            Uri location = new Uri(entryAssemblyCodeBase);
            FileInfo execFileInfo = new FileInfo(location.AbsolutePath);
            return Path.Join(execFileInfo.Directory.FullName, googleClientSecretPathFromConfig);
        }
    }
}
