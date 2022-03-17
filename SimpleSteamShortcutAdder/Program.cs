using System;
using System.IO;

namespace SimpleSteamShortcutAdder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("[FATAL] Not enough or too many args!");
                return;
            }

            SteamManager manager = new SteamManager();
            if (!manager.InitialisePaths())
            {
                Console.WriteLine("[FATAL] Could not initialise paths. Does a shortcuts.vdf file exist? Try to add a non-steam game to steam");
                return;
            }

            if (!manager.Read())
            {
                Console.WriteLine("[FATAL] Failed to read shortcuts.vdf");
                return;
            }

            if (!manager.AddExe(args[0], Path.GetFileName(args[0])))
            {
                Console.WriteLine("[FATAL] Failed to add executable");
                return;
            }

            if (!manager.Write())
            {
                Console.WriteLine("[FATAL] Failed to write to the shortcuts.vdf file");
                return;
            }
        
            Console.WriteLine("Finished!");
        }
    }
}