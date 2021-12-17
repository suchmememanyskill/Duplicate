using System;
using System.IO;
using VDFMapper.VDF;
using VDFMapper.ShortcutMap;
using VDFMapper;
using System.Linq;
using LegendaryMapperV2;
using LegendaryGUI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LegendaryMapperV2.Model;
using LegendaryMapperV2.Service;

namespace SteamShortcutsParser
{
    class Program
    { 
        private static LegendaryAuth auth;

        void OnUpdate(LegendaryDownload download)
        {
            Console.WriteLine($"Triggered onUpdate on {download.Game.AppTitle}: {download.Progress}");
        }

        void OnDownload(LegendaryDownload download)
        {
            Console.WriteLine($"Triggered onDownload on {download.Game.AppTitle}");
        }

        void OnRefresh(LegendaryGameManager man)
        {
            Console.WriteLine("Triggered refresh");
        }

        void Success()
        {
            Console.WriteLine("Success!");
            LegendaryGameManager parser = new(auth);
            parser.GetGames();
            //parser.InstalledGames[2].Launch();
            //parser.InstalledGames.Find(x => x.AppTitle == "Fez").Uninstall();
            LegendaryDownload download = parser.Games.Find(x => x.AppTitle == "Fez").InstantiateDownload();
            download.OnUpdate = OnUpdate;
            download.OnCompletionOrCancel = OnDownload;
            parser.OnGameRefresh = OnRefresh;
            download.Start();
        }
        void Failure()
        {
            Console.WriteLine("Failure!");
        }

        void Start()
        {
            auth = new LegendaryAuth();
            auth.AttemptLogin(Success, Failure);
        }

        static void Main(string[] args) => new Program().Start();
        //{
            //Directory.GetFiles("C:/Users/SuchMeme/.config/legendary/metadata").ToList().ForEach(x => JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(x)));
            /*
            LegendaryCommand command = new("legendary", "status --json");
            command.Block().Start();

            GameMetadata data = JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText("C:/Users/SuchMeme/.config/legendary/metadata/Cobra.json"));
            */

            //auth.GetStatus().Block().Start();

            

            /*
            Console.WriteLine("Hello World!" + GetSteamShortcutPath.GetShortcutsPath());

            SteamManager m = new SteamManager();
            m.Read();
            */
            /*
            Legendary l = new Legendary();
            SteamManager m = new SteamManager();
            m.Read();
            /*
            for (int i = 0; i < m.ShortcutRoot.GetSize(); i++)
                if (m.ShortcutRoot.GetEntry(i).AppName == "Fez")
                    Console.WriteLine(m.ShortcutRoot.GetEntry(i).AppId);

            return;
            //m.UpdateWithLegendaryGameList(l.InstalledGames, "EpicGames");
            m.Write();
            */

            //return;

            //Terminal terminal = Terminal.GetInstance();
            //terminal.Exec("legendary", "list-installed --csv");
            //Console.WriteLine("sup");
            /*
            Legendary wrapper = new Legendary();
            //wrapper.Reload();

            DownloadManager man = new DownloadManager(wrapper);
            int i = 0;



            

            if (wrapper.InstalledGames.Find(x => x.AppName == "cacao") != null)
                wrapper.RemoveGame(wrapper.InstalledGames.Find(x => x.AppName == "cacao")).Block().Start();

            if (wrapper.InstalledGames.Find(x => x.AppTitle == "Fez") != null)
                wrapper.RemoveGame(wrapper.InstalledGames.Find(x => x.AppTitle == "Fez")).Block().Start();

            man.QueueDownload(wrapper.NotInstalledGames.Find(x => x.AppName == "cacao"));
            man.QueueDownload(wrapper.NotInstalledGames.Find(x => x.AppTitle == "Fez"));

            //wrapper.InstallGame(wrapper.NotInstalledGames.Find(x => x.AppTitle == "Fez")).Block().Start();
            //wrapper.LaunchGame(wrapper.InstalledGames.Find(x => x.AppTitle == "Fez")).Block().Start();

            System.Threading.Thread.Sleep(5000);
            man.ActiveDownloads.Last().MoveUp();

            */
            /*
            Legendary legendary = new Legendary();
            legendary.DownloadManager.QueueDownload(legendary.NotInstalledGames.Find(x => x.AppTitle == "Fez"));
            legendary.DownloadManager.WaitUntilCompletion();
            legendary.LaunchGame(legendary.InstalledGames.Find(x => x.AppTitle == "Fez")).Start();

            return;
            */
            /*
            VDFStream stream = new VDFStream(GetSteamShortcutPath.GetShortcutsPath());
            //stream.ReadByte();
            //Console.WriteLine(stream.ReadString());
            
            VDFMap map = new VDFMap(stream);

            ShortcutRoot shortcutRoot = new ShortcutRoot(map);

            for (int i = 0; i < shortcutRoot.GetSize(); i++)
            {
                if (shortcutRoot.GetEntry(i).AppName.StartsWith("Fez"))
                {
                    Console.WriteLine(shortcutRoot.GetEntry(i).AppId);
                }
            }

            stream.Close();
            */
            /*
            ShortcutEntry entry = shortcutRoot.GetEntry(8);
            entry.AppName = "Celeste 2";
            entry.SetTag(0, "Humble");
            //ShortcutEntry entry = shortcutRoot.addEntry();

            Console.WriteLine(entry.GetTag(0));


            BinaryWriter writer = new BinaryWriter(new FileStream("./test.vdf", FileMode.OpenOrCreate));
            map.Write(writer, null);
            writer.Close();
            */
        //}
    }
}
