using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;

namespace BackUpAPP.Config
{
    internal class ConfigInit
    {

        string queryReadFolders = "Select Paths from Paths;";
        string queryInsertFolder = "INSERT OR IGNORE into Paths (Paths) VALUES(\"{0}\");";
        string queryDeleteFolders = "DELETE FROM Paths WHERE Paths=\"{0}\";";
        string CreateTables = "CREATE TABLE \"Paths\" (\"Paths\"TEXT UNIQUE); CREATE TABLE \"Settings\" (\"BackUpPath\" TEXT UNIQUE);";


        public static string ConfigPath = "Data Source=Settings.db";
        public static ConfigData Config { get; set; }

        public void UpdatePathConfig(string Cmd)
        {
            using (var conn = new SqliteConnection(ConfigPath))
            {
                conn.Open();
                using (var WriteCMD = new SqliteCommand(string.Format(queryDeleteFolders, Cmd), conn))
                {
                    WriteCMD.ExecuteNonQuery();
                }
            }
        }

        public void WritePathConfig(string Cmd)
        {
            using (var conn = new SqliteConnection(ConfigPath))
            {
                conn.Open();
                using (var WriteCMD = new SqliteCommand(string.Format(queryInsertFolder, Cmd), conn))
                {
                    try
                    {
                        WriteCMD.ExecuteNonQuery();
                    }
                    catch { }

                }
            }
        }

        public List<string> TMPPath = new List<string>();

        public void ReadConfig()
        {


            using (var conn = new SqliteConnection(ConfigPath))
            {

                if (!File.Exists("Settings.db"))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand(CreateTables, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                conn.Open();

                using (var cmd = new SqliteCommand(queryReadFolders, conn))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        TMPPath.Add(reader.GetString(0));
                        new ConfigData()
                        {
                            Path = TMPPath,
                            BackUpPath = ""
                        };
                    }
                }
            }
        }
    }
}

