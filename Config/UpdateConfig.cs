namespace BackUpAPP.Config
{
    internal class UpdateConfig
    {
        static ConfigInit cfg;

        public static void UpdateConfigFile(ListBox item)
        {
            cfg = new ConfigInit();

            foreach (var data in item.Items)
            {
                cfg.WritePathConfig(data.ToString());
            }

        }

        internal static void UpdatePath(string selectedPath)
        {
           
        }
    }
}
