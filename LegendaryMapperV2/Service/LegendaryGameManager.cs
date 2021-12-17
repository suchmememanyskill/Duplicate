using LegendaryMapperV2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private string gameDirectory = "";
        public string GameDirectory
        {
            get => gameDirectory;
            set
            {
                if (!Directory.Exists(value))
                    throw new Exception("Invalid game directory");

                gameDirectory = value;
            }
        }
        public List<LegendaryDownload> Downloads { get; private set; } = new List<LegendaryDownload>();

        public LegendaryGameManager(LegendaryAuth auth, LegendaryGameManagerCallback onGameRefresh = null)
        {
            Auth = auth;
            OnGameRefresh = onGameRefresh;
        }
        public void GetGames()
        {
            string configDir = Auth.StatusResponse.ConfigDirectory;
            Games = Directory.GetFiles(Path.Combine(configDir, "metadata"))
                .ToList()
                .Select(x => new LegendaryGame(JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(x)), this)).ToList();

            InstalledGameList list = new();

            list.Games = JsonConvert.DeserializeObject<Dictionary<string, InstalledGame>>(File.ReadAllText(Path.Combine(configDir, "installed.json")));

            list.GetGamesAsList()
                .ForEach(x => Games.Find(y => y.Metadata.AppName == x.AppName)?.SetInstalledData(x));

            OnGameRefresh?.Invoke(this);
        }

        public void AttachDownload(LegendaryDownload download)
        {
            Downloads.Add(download);
            OnGameRefresh?.Invoke(this);
        }

        public void DetachDownload(LegendaryDownload download)
        {
            Downloads.Remove(download);
            GetGames();
        }
    }
}
