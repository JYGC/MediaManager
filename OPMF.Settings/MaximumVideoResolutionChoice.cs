using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPMF.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MaximumVideoResolutionChoice
    {
        [EnumMember(Value = "none")] NoLimit,
        [EnumMember(Value = "2160p")] UpTo2160p,
        [EnumMember(Value = "1440p")] UpTo1440p,
        [EnumMember(Value = "1080p")] UpTo1080p,
        [EnumMember(Value = "720p")] UpTo720p,
        [EnumMember(Value = "480p")] UpTo480p,
        [EnumMember(Value = "360p")] UpTo360p,
    }
}
