using LegendaryGUI.Models;
using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Timers;
using System.Diagnostics;

namespace LegendaryGUI.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private Legendary legendary;
        private Timer timer;

        public void Update()
        {
            List<string> a = new List<string>();

            Queued
                .Where(x => !legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => Queued.Remove(x));

            legendary.DownloadManager.ActiveDownloads
                .Where(x => !Queued.Any(y => y.LaunchName == x.Game.AppName))
                .ToList()
                .ForEach(x => Queued.Add(new GameModel(x)));

            Installed
                .Where(x => !legendary.InstalledGames.Any(y => y.AppName == x.LaunchName) || legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => Installed.Remove(x));

            legendary.InstalledGames
                .Where(y => !Installed.Any(x => y.AppName == x.LaunchName))
                .Where(y => !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName))
                .ToList()
                .ForEach(x => Installed.Add(new GameModel(x)));

            NotInstalled
                .Where(x => !legendary.NotInstalledGames.Any(y => y.AppName == x.LaunchName) || legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => NotInstalled.Remove(x));

            legendary.NotInstalledGames
                .Where(y => !NotInstalled.Any(x => y.AppName == x.LaunchName))
                .Where(y => !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName))
                .ToList()
                .ForEach(x => NotInstalled.Add(new GameModel(x)));
        }

        public GameViewModel()
        {
            legendary = new Legendary();
            Queued = new ObservableCollection<GameModel>();
            Installed = new ObservableCollection<GameModel>(legendary.InstalledGames.Select(x => new GameModel(x)));
            NotInstalled = new ObservableCollection<GameModel>(legendary.NotInstalledGames.Select(x => new GameModel(x)));

            timer = new Timer(1500);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1500;
            timer.Enabled = true;
        }

        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            Update();
            
            foreach (LegendaryDownload dl in legendary.DownloadManager.ActiveDownloads)
            {
                GameModel q = Queued.First(x => x.LaunchName == dl.Game.AppName);
                q.UpdateDownload(dl);
            }

            timer.Enabled = true;
        }

        public void BtnRemove(string launchName) => legendary.RemoveGame(legendary.InstalledGames.Find(x => x.AppName == launchName)).Block().Start();
        public void BtnStart(string launchName) => legendary.LaunchGame(legendary.InstalledGames.Find(x => x.AppName == launchName)).Block().Start();
        public void BtnInstall(string launchName) => legendary.DownloadManager.QueueDownload(legendary.NotInstalledGames.Find(x => x.AppName == launchName));
        public void BtnStopDl(string launchName) => legendary.DownloadManager.RemoveDownload(legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName));
        public void BtnPauseDl(string launchName) => legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName).Stop();
        public void BtnStartDl(string launchName) => legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName).Start();
        


        public ObservableCollection<GameModel> Queued { get; }
        public ObservableCollection<GameModel> Installed { get; }
        public ObservableCollection<GameModel> NotInstalled { get; }
    }
}
