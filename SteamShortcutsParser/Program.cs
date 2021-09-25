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

            //Terminal terminal = Terminal.GetInstance();
            //terminal.Exec("legendary", "list-installed --csv");
            //Console.WriteLine("sup");

            Legendary wrapper = new Legendary();
            //wrapper.Reload();

            //wrapper.InstallGame(wrapper.NotInstalledGames.Find(x => x.AppTitle == "Fez")).Block().Start();
            wrapper.LaunchGame(wrapper.InstalledGames.Find(x => x.AppTitle == "Fez")).Block().Start();

            return;

            VDFStream stream = new VDFStream(GetSteamShortcutPath.GetShortcutsPath());
            //stream.ReadByte();
            //Console.WriteLine(stream.ReadString());

            VDFMap map = new VDFMap(stream);
            ShortcutRoot shortcutRoot = new ShortcutRoot(map);
            ShortcutEntry entry = shortcutRoot.GetEntry(8);
            entry.AppName = "Celeste 2";
            entry.SetTag(0, "Humble");
            //ShortcutEntry entry = shortcutRoot.addEntry();

            Console.WriteLine(entry.GetTag(0));


            BinaryWriter writer = new BinaryWriter(new FileStream("./test.vdf", FileMode.OpenOrCreate));
            map.Write(writer, null);
            writer.Close();
        }
    }
}
