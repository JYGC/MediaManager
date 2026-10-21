using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPMF.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VideoCodecChoice
    {
        [EnumMember(Value = "h264")] H264,
        [EnumMember(Value = "any")] AnyCodec,
    }
}
