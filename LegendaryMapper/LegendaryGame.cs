﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LegendaryMapper
{
    public class LegendaryGame
    {
        public string AppName { get; private set; }
        public string AppTitle { get; private set; }
        public string InstalledVersion { get; private set; }
        public string AvailableVersion { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public double InstallSize { get; private set; } // In GiB
        public string InstallPath { get; private set; }
        public bool ExtendedInfo { get; private set; }

        public LegendaryGame(string[] items)
        {
            // Extended: App name,App title,Installed version,Available version,Update available,Install size,Install path
            // Standard: App name,App title,Version,Is DLC

            if (items.Length == 4)
            {
                ExtendedInfo = false;
                AppName = items[0];
                AppTitle = items[1];
                AvailableVersion = items[2];
            }
            else if (items.Length == 7)
            {
                ExtendedInfo = true;
                AppName = items[0];
                AppTitle = items[1];
                InstalledVersion = items[2];
                AvailableVersion = items[3];
                UpdateAvailable = InstalledVersion != AvailableVersion;
                InstallSize = Math.Round(long.Parse(items[5]) / 1024 / 1024 / 1024 * 100f) / 100;
                InstallPath = items[6];
            }
            else
            {
                throw new Exception("Unimplemented");
            }
            
        }
    }
}