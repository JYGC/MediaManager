namespace OPMF.Settings
{
    public static class ConfigHelper
    {
        public static IReadonlySettings CreateReadonlySettingsForThisBuildConfiguration()
        {
#if DEBUG
            return new ReadOnlyDevSettings();
#else
            return new ReadonlySettings();
#endif
        }
    }
}
