using Newtonsoft.Json;
using System.Text;

namespace BackUpAPP.Config
{
    internal class ConfigInit
    {
        public static string ConfigPath { get; set; } = "Config.json";
        public static ConfigData? Config { get; set; }

        public async Task InitializeAsync()
        {
            var json = string.Empty;

            if (!File.Exists(ConfigPath))
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
                File.WriteAllText("Config.json", json, new UTF8Encoding(false));

                await Task.Delay(50);
            }

            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<ConfigData>(json);
        }

        private static ConfigData GenerateNewConfig() => new ConfigData
        {
            BackUpPath = "", // WIP
            Path = new string[] { }

        };
    }
}

