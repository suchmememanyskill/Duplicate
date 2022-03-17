
using System;
using System.IO;
using System.Linq;
using VDFMapper;
using VDFMapper.ShortcutMap;
using VDFMapper.VDF;

namespace SimpleSteamShortcutAdder
{
    public class SteamManager
    {
        private string vdfPath = "";
        private string gridPath = "";
        private VDFMap? root;
        public ShortcutRoot? ShortcutRoot { get; private set; }
        public string VdfPath { get => vdfPath; private set => vdfPath = value; }


        public bool InitialisePaths()
        {
            if (GetSteamShortcutPath.GetUserDataPath() == "")
                return false;

            if (GetSteamShortcutPath.GetCurrentlyLoggedInUser() <= 0)
                return false;

            vdfPath = GetSteamShortcutPath.GetShortcutsPath();
            gridPath = GetSteamShortcutPath.GetGridPath();
            return true;
        }

        public bool Read()
        {
            if (!File.Exists(vdfPath))
                return false;

            VDFStream stream = new VDFStream(vdfPath);
            root = new VDFMap(stream);
            ShortcutRoot = new ShortcutRoot(root);
            stream.Close();
            return true;
        }

        public bool Write()
        {
            File.WriteAllText(vdfPath, "");
            BinaryWriter writer = new BinaryWriter(new FileStream(vdfPath, FileMode.OpenOrCreate));
            root!.Write(writer, null);
            writer.Close();
            return true;
        }

        public bool AddExe(string path, string name)
        {
            if (!File.Exists(path))
                return false;

            name = name.Split(".").First();
            
            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                ShortcutEntry existingEntry = ShortcutRoot!.GetEntry(i);
                if (existingEntry.AppName == name)
                {
                    Console.WriteLine($"Updating entry {name}");
                    existingEntry.Exe = path;
                    if (existingEntry.Exe.Contains(" "))
                        existingEntry.Exe = $"\"{existingEntry.Exe}\"";

                    return true;
                }
            }
            
            Console.WriteLine($"Adding {name}");
            ShortcutEntry entry = ShortcutRoot!.AddEntry();
            entry.AppName = name;
            entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);
            entry.Exe = path;
            
            if (entry.Exe.Contains(" "))
                entry.Exe = $"\"{entry.Exe}\"";

            return true;
        }
    }
}
