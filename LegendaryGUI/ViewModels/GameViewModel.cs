﻿using LegendaryGUI.Models;
using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Timers;

namespace LegendaryGUI.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private Legendary legendary;
        private Timer timer;

        public void Update()
        {
            Queued
                .Where(x => !legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => Queued.Remove(x));

            legendary.DownloadManager.ActiveDownloads
                .Where(x => !Queued.Any(y => y.LaunchName == x.Game.AppName))
                .ToList()
                .ForEach(x => Queued.Add(new GameModel(x)));

            Installed
                .Where(x => !(legendary.InstalledGames.Any(y => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName)))
                .ToList()
                .ForEach(x => Installed.Remove(x));

            legendary.InstalledGames
                .Where(y => !(Installed.Any(x => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName)))
                .ToList()
                .ForEach(x => Installed.Add(new GameModel(x)));

            NotInstalled
                .Where(x => !(legendary.NotInstalledGames.Any(y => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName)))
                .ToList()
                .ForEach(x => NotInstalled.Remove(x));

            legendary.NotInstalledGames
                .Where(y => !(NotInstalled.Any(x => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName)))
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
            /*
            foreach (LegendaryDownload dl in legendary.DownloadManager.ActiveDownloads)
            {
                if (!Queued.Any(x => x.LaunchName == dl.Game.AppName))
                {
                    if (Installed.Any(x => x.LaunchName == dl.Game.AppName))
                    {
                        GameModel m = Installed.First(x => x.LaunchName == dl.Game.AppName);
                        Installed.Remove(m);
                        Queued.Add(m);
                    }
                    else
                    {
                        GameModel m = NotInstalled.First(x => x.LaunchName == dl.Game.AppName);
                        NotInstalled.Remove(m);
                        Queued.Add(m);
                    }
                }


            }
            */

            timer.Enabled = true;
        }

        public ObservableCollection<GameModel> Queued { get; }
        public ObservableCollection<GameModel> Installed { get; }
        public ObservableCollection<GameModel> NotInstalled { get; }
    }
}
