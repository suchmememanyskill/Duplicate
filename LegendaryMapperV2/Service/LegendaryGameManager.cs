using LegendaryMapperV2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Service
{
    public class LegendaryGameManager
    {
        public delegate void LegendaryGameManagerCallback(LegendaryGameManager manager);
        public LegendaryAuth Auth { get; private set; }
        public List<LegendaryGame> Games { get; private set; }
        public List<LegendaryGame> InstalledGames { get => Games.Where(x => x.InstalledData != null).ToList(); }
        public List<LegendaryGame> NotInstalledGames { get => Games.Where(x => x.InstalledData == null).ToList(); }
        public LegendaryGameManagerCallback OnGameRefresh { get; set; }
        public Config Config { get => Auth.Config; }
        private string gameDirectory = "";
        public string GameDirectory
        {
            get => gameDirectory;
            set
            {
                string val = value.Trim();
                if (val == "")
                {
                    if (File.Exists(DlLocFile))
                        File.Delete(DlLocFile);

                    gameDirectory = "";
                    return;
                }

                if (!Directory.Exists(val))
                    throw new Exception("Invalid game directory");

                File.WriteAllText(DlLocFile, val);

                gameDirectory = val;
            }
        }
        public List<LegendaryDownload> Downloads { get; private set; } = new List<LegendaryDownload>();

        public LegendaryGameManager(LegendaryAuth auth, LegendaryGameManagerCallback onGameRefresh = null)
        {
            Auth = auth;
            OnGameRefresh = onGameRefresh;
            if (File.Exists(DlLocFile))
            {
                string dir = File.ReadAllText(DlLocFile);
                if (Directory.Exists(dir))
                    gameDirectory = dir;
            }      
        }
        public void GetGames()
        {
            string configDir = Auth.StatusResponse.ConfigDirectory;
            Games = Directory.GetFiles(Path.Combine(configDir, "metadata"))
                .ToList()
                .Select(x => new LegendaryGame(JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(x)), this)).ToList();

            if (File.Exists(Path.Combine(configDir, "installed.json")))
            {
                InstalledGameList list = new();

                list.Games = JsonConvert.DeserializeObject<Dictionary<string, InstalledGame>>(File.ReadAllText(Path.Combine(configDir, "installed.json")));

                list.GetGamesAsList()
                    .ForEach(x => Games.Find(y => y.Metadata.AppName == x.AppName)?.SetInstalledData(x));
            }

            // Filter out dlc
            List<LegendaryGame> dlc = new();
            Games.ForEach(currentGame =>
            {
                if (currentGame.Metadata != null && currentGame.Metadata.Metadata != null && currentGame.Metadata.Metadata.DlcItemList != null)
                {
                    List<LegendaryGame> currentGameDlc = Games.Where(possibleDlc =>
                        currentGame.Metadata.Metadata.DlcItemList.Any(currentGameDlc =>
                        {
                            if (currentGameDlc.ReleaseDetails == null)
                                return false;

                            return currentGameDlc.ReleaseDetails.Any(currentGameDlcRelease =>
                                currentGameDlcRelease.AppId == possibleDlc.AppName
                            );
                        })
                    ).ToList();
                    currentGame.Dlc.AddRange(currentGameDlc);
                    currentGameDlc.ForEach(x => x.IsDlc = true);
                    dlc.AddRange(currentGameDlc);
                }
            });

            Games.RemoveAll(x => dlc.Contains(x));

            // Filter out UE stuff
            Games.RemoveAll(x =>
            {
                if (x.Metadata != null && x.Metadata.Metadata != null && x.Metadata.Metadata.Categories != null)
                    return !x.Metadata.Metadata.Categories.Any(y => y["path"] == "games");

                return false;
            });

            Games = Games.OrderBy(x => x.AppTitle).ToList();
            OnGameRefresh?.Invoke(this);
        }

        public void AttachDownload(LegendaryDownload download)
        {
            Downloads.Add(download);
            OnGameRefresh?.Invoke(this);
        }

        public void SaveConfig() => Auth.SaveConfig();

        public void DetachDownload(LegendaryDownload download)
        {
            Downloads.Remove(download);
            GetGames();
        }

        private static string DlLocFile { get => Path.Join(LegendaryAuth.ConfigDir, "DlLoc.txt"); }
    }
}
