using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace VDFMapper
{
    public static class GetSteamShortcutPath
    {
        public static string GetUserDataPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string path = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null);
                if (path == null)
                    return "";
                return Path.Combine(path, "userdata");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam","steam","userdata");
            }
        }

        public static int GetCurrentlyLoggedInUser()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\ActiveProcess", "ActiveUser", -1);
            }
            else
            {
                int a = -1;
                return int.Parse(new DirectoryInfo(GetUserDataPath()).GetDirectories().ToList().Where(x => int.TryParse(x.Name, out a)).First().Name);
            }
        }

        public static string GetShortcutsPath()
        {
            return Path.Combine(GetUserDataPath(), GetCurrentlyLoggedInUser().ToString(), "config", "shortcuts.vdf");
        }

        public static string GetGridPath()
        {
            string path = Path.Combine(GetUserDataPath(), GetCurrentlyLoggedInUser().ToString(), "config", "grid");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
