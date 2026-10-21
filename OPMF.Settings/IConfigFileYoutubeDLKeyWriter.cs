namespace OPMF.Settings
{
    public interface IConfigFileYoutubeDLKeyWriter
    {
        void WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(string configFilePath, string youtubeDLKeyName, object value);
    }
}
