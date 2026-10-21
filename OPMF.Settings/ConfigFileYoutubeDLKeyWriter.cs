using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OPMF.Settings
{
    public class ConfigFileYoutubeDLKeyWriter : IConfigFileYoutubeDLKeyWriter
    {
        private const string YoutubeDLKeyNameInConfigFile = nameof(AppConfig.YoutubeDL);

        public void WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(string configFilePath, string youtubeDLKeyName, object value)
        {
            var configFileJson = JObject.Parse(File.ReadAllText(configFilePath));
            if (configFileJson[YoutubeDLKeyNameInConfigFile] is not JObject youtubeDLJson)
            {
                youtubeDLJson = new JObject();
                configFileJson[YoutubeDLKeyNameInConfigFile] = youtubeDLJson;
            }
            youtubeDLJson[youtubeDLKeyName] = JToken.FromObject(value);

            var temporaryFilePath = configFilePath + ".tmp";
            File.WriteAllText(temporaryFilePath, configFileJson.ToString(Formatting.Indented));
            File.Move(temporaryFilePath, configFilePath, overwrite: true);
        }
    }
}
