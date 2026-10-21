using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPMF.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AudioQualityChoice
    {
        [EnumMember(Value = "best-aac")] BestAac,
        [EnumMember(Value = "best-any-codec")] BestOfAnyCodec,
        [EnumMember(Value = "smallest-aac")] SmallestAac,
    }
}
