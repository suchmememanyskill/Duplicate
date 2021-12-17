using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Service
{
    public class LegendaryDownload
    {
        public delegate void DownloadCallback(LegendaryDownload download);
        public double Progress { get; private set; }
        public long SecondsElapsed { get; private set; }
        public long SecondsETA { get; private set; }
        public LegendaryCommand Action { get; private set; }
        public string GameSize { get; private set; } = "0 MiB";
        public string DownloadSize { get; private set; } = "0 MiB";
        public bool IsDownloading { get { return (Action.Terminal == null) ? false : Action.Terminal.IsActive; } }
        public LegendaryGame Game { get; private set; }
        public DownloadCallback OnUpdate { get; set; } = null;
        public DownloadCallback OnCompletionOrCancel { get; set; } = null;
        private bool attached = false;

        public void DownloadTracker(LegendaryCommand action)
        {
            string last = action.Terminal.StdErr.Last();

            if (last.StartsWith("[cli] INFO: Install size: "))
                GameSize = last.Substring(26);

            else if (last.StartsWith("[cli] INFO: Download size: "))
                DownloadSize = last.Substring(27).Split('(')[0].Trim();

            else if (last.StartsWith("[DLManager] INFO: = Progress: "))
            {
                last = last.Substring(30);
                string[] temp = last.Split('%');
                double a = 0;
                double.TryParse(temp[0].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out a);
                Progress = a;
                temp = last.Split(',');
                DateTime dt = DateTime.ParseExact(temp[1].Substring(13), "hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                SecondsElapsed = (int)dt.TimeOfDay.TotalSeconds;
                dt = DateTime.ParseExact(temp[2].Substring(6), "hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                SecondsETA = (int)dt.TimeOfDay.TotalSeconds;
                //Console.WriteLine($"Dumped: E:{SecondsElapsed} A:{SecondsETA} P:{Progress}% S:{GameSize:0.00}");
            }
            else return;

            OnUpdate?.Invoke(this);
        }

        public LegendaryDownload(LegendaryGame game)
        {
            Game = game;

            string args = "";
            if (game.Parser.GameDirectory != "")
                args += $"--game-folder \"{game.Parser.GameDirectory}/{game.AppTitle}\"";

            Action = new LegendaryCommand($"-y install {game.AppName} {args}").Then(x => NotifyCompletion()).OnErrLine(DownloadTracker);
        }

        public void Start()
        {
            if (Game.IsInstalled && !Game.UpdateAvailable)
                throw new Exception("Game is already installed and up to date"); 

            if (!attached)
            {
                attached = true;

                if (Game.Parser.Downloads.Any(x => x.Game.AppName == Game.AppName))
                    throw new Exception("Game is already downloading");

                Game.Parser.AttachDownload(this);
            }

            if (!IsDownloading)
                Action.Start();
        }

        public void NotifyCompletion()
        {
            Game.Parser.DetachDownload(this);
            OnCompletionOrCancel?.Invoke(this);
        }

        public void Pause() => Action.Stop();

        public void Stop()
        {
            Pause();
            NotifyCompletion();
        }
    }
}
