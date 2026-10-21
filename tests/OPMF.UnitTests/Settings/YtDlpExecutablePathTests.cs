using System.IO;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Settings
{
    public class YtDlpExecutablePathTests
    {
        public static TheoryData<IReadonlySettings> SettingsOfEveryKind => new TheoryData<IReadonlySettings>
        {
            new ReadonlySettings(),
            new ReadOnlyDevSettings(),
            new ReadOnlyTestSettings(),
            new TemporaryFolderReadonlySettings(Path.GetTempPath()),
        };

        [Theory]
        [MemberData(nameof(SettingsOfEveryKind))]
        public void TheYtDlpExecutableIsInTheBinFolderUnderTheAppFolder(IReadonlySettings readonlySettings)
        {
            Assert.Equal(Path.Join(readonlySettings.GetLocalAppFolderPath(), "Bin", "yt-dlp.exe"), readonlySettings.GetYtDlpExecutablePath());
        }

        [Theory]
        [MemberData(nameof(SettingsOfEveryKind))]
        public void TheYoutubeDlExecutableStaysInTheSameBinFolderAsTheYtDlpExecutable(IReadonlySettings readonlySettings)
        {
            Assert.Equal(Path.Join(readonlySettings.GetBinFolderPath(), "youtube-dl.exe"), readonlySettings.GetYoutubeDLPath());
            Assert.Equal(readonlySettings.GetBinFolderPath(), Path.GetDirectoryName(readonlySettings.GetYtDlpExecutablePath()));
        }
    }
}
