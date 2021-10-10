using Force.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDFMapper;
using VDFMapper.ShortcutMap;
using VDFMapper.VDF;
using LegendaryMapper;

namespace LegendaryGUI.Services
{
    public class SteamManager
    {
        private string vdfPath = GetSteamShortcutPath.GetShortcutsPath();
        private string gridPath = GetSteamShortcutPath.GetGridPath();
        private VDFMap root;
        public ShortcutRoot ShortcutRoot { get; private set; }
        public string VdfPath { get => vdfPath; private set => vdfPath = value; }

        public SteamManager()
        {
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
            root.Write(writer, null);
            writer.Close();
            return true;
        }

        public int RemoveAllGamesWithTag()
        {
            int count = 0;

            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                if (ShortcutRoot.GetEntry(i).AppName.Contains("(Epic)"))
                {
                    ShortcutRoot.RemoveEntry(i);
                    i--;
                    count++;
                }
            }

            return count;
        }

        private void UpdateExe(ShortcutEntry entry, string appName)
        {

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                entry.Exe = Path.GetFullPath("./LegendaryGUI.exe");
            }
            else
            {
                entry.Exe = Path.GetFullPath("./LegendaryGUI");
            }
            entry.LaunchOptions = $"{appName}";
        }

        public Tuple<int, int> UpdateWithLegendaryGameList(List<LegendaryGame> legendaryGames)
        {
            List<LegendaryGame> copy = new(legendaryGames);
            List<int> unknownIndexes = new();

            int removedCount = 0;
            int addedCount = 0;

            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                ShortcutEntry entry = ShortcutRoot.GetEntry(i);
                if (entry.AppName.Contains("(Epic)"))
                {
                    string temp = entry.AppName.Substring(0, entry.AppName.Length - 7);
                    if (copy.Any(x => temp == x.AppTitle)) // Is game already registered?
                    {
                        LegendaryGame game = copy.Find(x => temp == x.AppTitle);
                        copy.Remove(game);
                        UpdateExe(entry, game.AppName);
                    }
                    else // Game that doesn't seem to be in the list. lets remove it
                    {
                        ShortcutRoot.RemoveEntry(i);
                        removedCount++;
                        i--;
                    }
                }
            }

            copy.ForEach(x =>
            {
                ShortcutEntry entry = ShortcutRoot.AddEntry();
                entry.AppName = $"{x.AppTitle} (Epic)";
                entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);
                UpdateExe(entry, x.AppName);
                entry.AddTag("EpicGames");

                LegendaryImage boxTall = x.ExtendedJson.Images.Find(y => y.Type == "DieselGameBoxTall");
                LegendaryImage box = x.ExtendedJson.Images.Find(y => y.Type == "DieselGameBox");
                
                if (boxTall != null)
                {
                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}.{boxTall.UrlExt}"))){
                        boxTall.SaveUrlAs(Path.Combine(gridPath, $"{entry.AppId}"));
                    }

                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}p.{boxTall.UrlExt}")))
                    {
                        File.Copy(Path.Combine(gridPath, $"{entry.AppId}.{boxTall.UrlExt}"), Path.Combine(gridPath, $"{entry.AppId}p.{boxTall.UrlExt}"));
                    }
                }
                    
                if (box != null)
                {
                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}_hero.{boxTall.UrlExt}")))
                        box.SaveUrlAs(Path.Combine(gridPath, $"{entry.AppId}_hero"));
                }

                addedCount++;
            });

            return new Tuple<int, int>(removedCount, addedCount);
        }
    }
}
