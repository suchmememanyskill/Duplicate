using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LegendaryMapper
{
    public class LegendaryDownload
    {
        private DownloadManager downloadMan;
        public double Progress { get; private set; }
        public long SecondsElapsed { get; private set; }
        public long SecondsETA { get; private set; }
        public LegendaryActionBuilder Action { get; private set; }
        public LegendaryGame Game { get; private set; }
        public string GameSize { get; private set; } = "0 MiB";
        public string DownloadSize { get; private set; } = "0 MiB";
        public bool IsDownloading { get { return (Action.Terminal == null) ? false : Action.Terminal.IsActive; } }
        public int DownloadIndex { get => downloadMan.ActiveDownloads.IndexOf(this); }

        public void DownloadTracker(LegendaryActionBuilder action)
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
                Progress = double.Parse(temp[0], System.Globalization.NumberStyles.Any);
                temp = last.Split(',');
                DateTime dt = DateTime.ParseExact(temp[1].Substring(13), "hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                SecondsElapsed = (int)dt.TimeOfDay.TotalSeconds;
                dt = DateTime.ParseExact(temp[2].Substring(6), "hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                SecondsETA = (int)dt.TimeOfDay.TotalSeconds;
                //Console.WriteLine($"Dumped: E:{SecondsElapsed} A:{SecondsETA} P:{Progress}% S:{GameSize:0.00}");
            }


            
        }

        public void DownloadCompletion(LegendaryActionBuilder action)
        {
            downloadMan.NotifyCompletion(this);
        }

        public LegendaryDownload(LegendaryActionBuilder action, DownloadManager downloadMan, LegendaryGame game)
        {
            Action = action.OnErrLine(DownloadTracker).Then(DownloadCompletion);
            this.downloadMan = downloadMan;
            Game = game;
        }

        public void Start() {
            if (!IsDownloading)
                Action.Start();
        } 
        public void Stop() => Action.Stop();

        public void MoveUp() => downloadMan.MoveGameUp(Game);
        public void MoveDown() => downloadMan.MoveGameDown(Game);
    }
}
