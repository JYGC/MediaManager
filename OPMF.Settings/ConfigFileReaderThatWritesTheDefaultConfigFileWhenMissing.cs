using System.IO;
using Newtonsoft.Json;

namespace OPMF.Settings
{
    public class ConfigFileReaderThatWritesTheDefaultConfigFileWhenMissing
    {
        public AppConfig ReadConfigFileOrWriteDefaultConfigFileWhenMissing(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configFilePath).Trim());
            }

            var defaultAppConfig = new AppConfig();
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultAppConfig, Formatting.Indented));
            return defaultAppConfig;
        }
    }
}
