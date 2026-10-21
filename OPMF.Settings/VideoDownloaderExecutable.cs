using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPMF.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VideoDownloaderExecutable
    {
        [EnumMember(Value = "youtube-dl")] YoutubeDl,
        [EnumMember(Value = "yt-dlp")] YtDlp,
    }
}
