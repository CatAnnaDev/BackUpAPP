using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpAPP.Config
{
    internal class UpdateConfig
    {
        public static void UpdateConfigFile(ListBox item)
        {

            List<string> list = new List<string>();

            foreach (var data in item.Items)
            {
                list.Add((string)data);
            }       

            var json = string.Empty;

            if (File.Exists(ConfigInit.ConfigPath))
            {
                json = JsonConvert.SerializeObject(Update(list.ToArray()), Formatting.Indented);
                File.WriteAllText("Config.json", json, new UTF8Encoding(false));
            }
        }

        static ConfigData Update(string[] tmp) => new ConfigData
        {
            Path = tmp
        };
    }
}
