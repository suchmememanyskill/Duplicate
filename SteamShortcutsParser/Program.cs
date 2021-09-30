using System;
using System.IO;
using VDFMapper.VDF;
using VDFMapper.ShortcutMap;
using VDFMapper;
using System.Linq;
using LegendaryMapper;

namespace SteamShortcutsParser
{
    class Program
    {
        static void Output(Terminal t)
        {
            /*
            Console.WriteLine($"Exitcode: {t.ExitCode}");
            foreach (string s in t.StdOut)
            {
                Console.WriteLine(s);
            }

            foreach (string s in t.StdErr)
            {
                Console.WriteLine(s);
            }
            */
        }

        static void OutputLine(Terminal t)
        {
            Console.WriteLine(t.StdOut.Last());
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!" + GetSteamShortcutPath.GetShortcutsPath());

            Legendary l = new Legendary();
            SteamManager m = new SteamManager();
            m.Read();
            /*
            for (int i = 0; i < m.ShortcutRoot.GetSize(); i++)
                if (m.ShortcutRoot.GetEntry(i).AppName == "Fez")
                    Console.WriteLine(m.ShortcutRoot.GetEntry(i).AppId);

            return;
            */
            m.UpdateWithLegendaryGameList(l.InstalledGames, "EpicGames");
            m.Write();
            return;

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
        }
    }
}
