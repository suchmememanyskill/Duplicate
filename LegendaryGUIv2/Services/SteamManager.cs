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
using LegendaryMapperV2.Service;
using LegendaryMapperV2.Model;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Runtime.InteropServices;

namespace LegendaryGUIv2.Services
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

        public int RemoveAllGamesWithTag()
        {
            int count = 0;

            for (int i = 0; i < ShortcutRoot!.GetSize(); i++)
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

        private void UpdateExe(ShortcutEntry entry, LegendaryGame game)
        {
            entry.Exe = Utils.GetExecutablePath();
            if (entry.Exe.Contains(" "))
                entry.Exe = $"\"{entry.Exe}\"";
            entry.LaunchOptions = $"{game.AppName}";

            string iconPath = Path.Combine(game.InstalledData.InstallPath, game.InstalledData.Executable);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // Steam only supports reading an exe's icon on windows, seemingly
                entry.Icon = iconPath.Replace("/", "\\");
        }

        public Tuple<int, int> UpdateWithLegendaryGameList(List<LegendaryGame> legendaryGames)
        {
            List<LegendaryGame> copy = new(legendaryGames);
            List<int> unknownIndexes = new();

            int removedCount = 0;
            int addedCount = 0;

            for (int i = 0; i < ShortcutRoot!.GetSize(); i++)
            {
                ShortcutEntry entry = ShortcutRoot.GetEntry(i);
                if (entry.AppName.Contains("(Epic)"))
                {
                    string temp = entry.AppName[0..^7];
                    if (copy.Any(x => temp == x.AppTitle)) // Is game already registered?
                    {
                        LegendaryGame? game = copy.Find(x => temp == x.AppTitle);
                        if (game != null)
                        {
                            copy.Remove(game);
                            UpdateExe(entry, game);
                        }
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
                UpdateExe(entry, x);
                entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);
                entry.AddTag("EpicGames");

                MetaImage boxTall = x.GameBannerTall;
                MetaImage box = x.GameBanner;
                
                if (boxTall != null)
                {
                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}.jpg"))){
                        Stream outStream = new FileStream(Path.Combine(gridPath, $"{entry.AppId}.jpg"), FileMode.OpenOrCreate);
                        Image<Rgba32> image = x.GetGameBannerTallWithLogo();
                        image.Save(outStream, new JpegEncoder());
                        image.Dispose();
                        outStream.Dispose();
                    }

                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}p.jpg")))
                        File.Copy(Path.Combine(gridPath, $"{entry.AppId}.jpg"), Path.Combine(gridPath, $"{entry.AppId}p.jpg"));
                }
                    
                if (box != null)
                {
                    if (!File.Exists(Path.Combine(gridPath, $"{entry.AppId}_hero.{box.UrlExt}")))
                        File.WriteAllBytes(Path.Combine(gridPath, $"{entry.AppId}_hero.{box.UrlExt}"), box.GetImage());
                }

                addedCount++;
            });

            return new Tuple<int, int>(removedCount, addedCount);
        }
    }
}
