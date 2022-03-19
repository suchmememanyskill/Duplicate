﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LegendaryMapperV2.Services
{
    public class ProtonManager
    {
        public bool CanUseProton => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && GetProtonPaths().Count > 0;

        public Dictionary<string, string> GetProtonPaths()
        {
            string steamApps = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local/share/Steam/steamapps/common");

            if (!Directory.Exists(steamApps))
                return new();

            Dictionary<string, string> entries = new();

            foreach (var enumerateDirectory in Directory.EnumerateDirectories(steamApps))
            {
                if (Path.GetFileName(enumerateDirectory).Contains("Proton"))
                {
                    string version = Path.GetFileName(enumerateDirectory).Split(' ', 2).Last();
                    entries.Add(version, enumerateDirectory);
                }
            }

            return entries;
        }
    }
}