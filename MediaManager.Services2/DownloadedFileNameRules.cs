using System.Text.RegularExpressions;

namespace MediaManager.Services2;

public static partial class DownloadedFileNameRules
{
    // yt-dlp saves each stream as <name>.f<format id>.<extension>, such as x.f137.mp4, x.f140-drc.m4a or x.f251-12.webm, until it has merged them
    [GeneratedRegex(@"\.f\d[0-9A-Za-z]*(-[0-9A-Za-z]+)*\.[0-9A-Za-z]+$")]
    private static partial Regex StreamFileNameEnding();

    public static bool IsNameOfStreamFileLeftBeforeMerging(string fileName)
    {
        return StreamFileNameEnding().IsMatch(fileName);
    }
}
