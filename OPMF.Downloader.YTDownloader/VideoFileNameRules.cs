namespace OPMF.Downloader.YTDownloader
{
    public static class VideoFileNameRules
    {
        private const char CharacterThatTheExecutablesPutInPlaceOfEachCharacterTheyCannotUseInAFileName = '#';
        private static readonly char[] CharactersThatTheExecutablesRenameInAFileName = ['<', '>', ':', '|', '?', '*'];

        public static string CreateFileNameWithoutExtensionFromTitle(string title)
        {
            string fileName = title.Replace("/", "forward slash");
            fileName = fileName.Replace("\"", "''");
            fileName = fileName.Replace("%", "percent");
            foreach (char characterTheExecutablesRename in CharactersThatTheExecutablesRenameInAFileName)
            {
                fileName = fileName.Replace(characterTheExecutablesRename, CharacterThatTheExecutablesPutInPlaceOfEachCharacterTheyCannotUseInAFileName);
            }
            return fileName;
        }
    }
}
