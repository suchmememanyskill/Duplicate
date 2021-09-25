using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace VDFMapper
{
    public static class GetSteamShortcutPath
    {
        public static string GetUserDataPath()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(
                    (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null),
                    "userdata"
                    );
            }
        
            return null;
        }

        public static int GetCurrentlyLoggedInUser()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\ActiveProcess", "ActiveUser", -1);
            }

            return -1;
        }

        public static string GetShortcutsPath()
        {
            return Path.Combine(GetUserDataPath(), GetCurrentlyLoggedInUser().ToString(), "config", "shortcuts.vdf");
        }
    }
}
