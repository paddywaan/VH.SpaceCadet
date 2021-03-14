using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VH.SpaceCadet.EquipmentLevelGate
{
    public static class Config
    {
        public static ConfigFile GeneralSettings;

        static Config()
        {
            GeneralSettings = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, Main.MODNAME + ".cfg"), true);
        }
    }
}
