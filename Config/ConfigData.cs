using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpAPP.Config
{
    internal class ConfigData
    {
        public string Comment { get; set; }
        public string BackUpPath { get; set; }
        public string[] Path { get; set; }
    }
}
