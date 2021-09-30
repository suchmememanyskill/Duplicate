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

namespace LegendaryMapper
{
    public class SteamManager
    {
        private string vdfPath = GetSteamShortcutPath.GetShortcutsPath();
        private string gridPath = GetSteamShortcutPath.GetGridPath();
        private VDFMap root;
        public ShortcutRoot ShortcutRoot { get; private set; }

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

        public int RemoveAllGamesWithTag(string tag)
        {
            int count = 0;

            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                if (ShortcutRoot.GetEntry(i).GetTagIndex(tag) != -1)
                {
                    ShortcutRoot.RemoveEntry(i);
                    i--;
                    count++;
                }
            }

            return count;
        }

        // TODO: implement in lib
        private uint GenerateSteamGridAppId(string appName, string appTarget)
        {
            byte[] nameTargetBytes = Encoding.UTF8.GetBytes(appTarget + appName + "");
            uint crc = Crc32Algorithm.Compute(nameTargetBytes);
            uint gameId = crc | 0x80000000;

            return gameId;
        }

        public Tuple<int, int> UpdateWithLegendaryGameList(List<LegendaryGame> legendaryGames, string tag)
        {
            List<LegendaryGame> copy = new List<LegendaryGame>(legendaryGames);
            List<int> unknownIndexes = new List<int>();

            int removedCount = 0;
            int addedCount = 0;

            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                ShortcutEntry entry = ShortcutRoot.GetEntry(i);
                if (entry.GetTagIndex(tag) != -1)
                {
                    if (copy.Any(x => entry.AppName == x.AppTitle)) // Is game already registered?
                    {
                        copy.Remove(copy.Find(x => entry.AppName == x.AppTitle));
                        // TODO
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
                entry.AppName = x.AppTitle;
                entry.AppId = GenerateSteamGridAppId(entry.AppName, entry.Exe);
                entry.Exe = "legendary";
                entry.LaunchOptions = $"launch {x.AppName}";
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
