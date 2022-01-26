using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LegendaryGUIv2.Models;
using System.IO;

namespace LegendaryGUIv2.Services
{
    public class ProcessMonitor
    {
        private LegendaryGame game;
        private ProcessSessionLog session = new();
        public ProcessMonitor(LegendaryGame game)
        {
            this.game = game;
        }

        public void SpawnNewApp() => SpawnApp($"{game.AppName}");
        public void SpawnNewWatchApp() => SpawnApp($"watch {game.AppName}");
        public void SpawnNewAppSkipUpdate() => SpawnApp($"skipupdate {game.AppName}");
        public static void SpawnApp(string args)
        {
            string path = Utils.GetExecutablePath();
            Process.Start(path, args);
        }

        public void SetStartTime() => session.StartTime = DateTime.Now;

        public void SetEndTime()
        {
            session.EndTime = DateTime.Now;
            session.TimeSpent = session.EndTime - session.StartTime;
        }

        public void Write()
        {
            string path = Path.Join(LegendaryAuth.ConfigDir, $"{game.AppName}.json");
            ProcessLog? log = null;
            if (!File.Exists(path))
            {
                log = new();
                log.AppName = game.AppName;
                log.AppTitle = game.AppTitle;
            }
            else
                log = JsonConvert.DeserializeObject<ProcessLog>(File.ReadAllText(path));

            log!.Sessions.Add(session);
            File.WriteAllText(path, JsonConvert.SerializeObject(log));
        }

        public void Monitor()
        {
            Process? p = null;
            int count = 0;
            do
            {
                Thread.Sleep(1000);
                p = game.GetGameProcess();
                count++;
                if (count > 30)
                    return; // I give up
            } while (p == null);


            SetStartTime();
            p.WaitForExit();
            SetEndTime();

            Write();
        }
    }
}
