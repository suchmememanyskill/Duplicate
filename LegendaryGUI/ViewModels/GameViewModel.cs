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
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;

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
                .ForEach(x => { Queued.Remove(x); Debug.WriteLine($"Removing from queued: {x.LaunchName}"); });

            legendary.DownloadManager.ActiveDownloads
                .Where(x => !Queued.Any(y => y.LaunchName == x.Game.AppName))
                .ToList()
                .ForEach(x => { Queued.Add(new GameModel(x)); Debug.WriteLine($"Adding to queued: {x.Game.AppName}"); });

            Installed
                .Where(x => !legendary.InstalledGames.Any(y => y.AppName == x.LaunchName) || legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => { Installed.Remove(x); Debug.WriteLine($"Removing from installed: {x.LaunchName}"); });

            legendary.InstalledGames
                .Where(y => !Installed.Any(x => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName))
                .ToList()
                .ForEach(x => { Installed.Add(new GameModel(x)); Debug.WriteLine($"Adding to installed: {x.AppName}"); });

            NotInstalled
                .Where(x => !legendary.NotInstalledGames.Any(y => y.AppName == x.LaunchName) || legendary.DownloadManager.ActiveDownloads.Any(y => y.Game.AppName == x.LaunchName))
                .ToList()
                .ForEach(x => { NotInstalled.Remove(x); Debug.WriteLine($"Removing from notInstalled: {x.LaunchName}"); });

            legendary.NotInstalledGames
                .Where(y => !NotInstalled.Any(x => y.AppName == x.LaunchName) && !legendary.DownloadManager.ActiveDownloads.Any(x => x.Game.AppName == y.AppName))
                .ToList()
                .ForEach(x => { NotInstalled.Add(new GameModel(x)); Debug.WriteLine($"Adding to notInstalled: {x.AppName}"); });
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

        public void BtnRemove(string launchName)
        {
            LegendaryGame gam = legendary.InstalledGames.Find(x => x.AppName == launchName);

            var messageBoxOkToRemove = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = "Remove Game?",
                ContentMessage = $"Are you sure you want to remove {gam.AppTitle}?",
                Style = Style.DarkMode,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                SizeToContent = Avalonia.Controls.SizeToContent.Width,
                Height = 125,
                CanResize = true,
            });

            Task<ButtonResult> btnTask = messageBoxOkToRemove.Show();
            btnTask.ContinueWith(x =>
            {
                if (x.Result == ButtonResult.Ok)
                    legendary.RemoveGame(gam).Start(); // I cannot block this for some reason
            });
        }
        public void BtnStart(string launchName) => legendary.LaunchGame(legendary.InstalledGames.Find(x => x.AppName == launchName)).Block().Start();
        public void BtnInstall(string launchName) => legendary.DownloadManager.QueueDownload(legendary.NotInstalledGames.Find(x => x.AppName == launchName));
        public void BtnStopDl(string launchName) => legendary.DownloadManager.RemoveDownload(legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName));
        public void BtnPauseDl(string launchName) => legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName).Stop();
        public void BtnStartDl(string launchName) => legendary.DownloadManager.ActiveDownloads.First(x => x.Game.AppName == launchName).Start();
        
        public void BtnInfo(string launchName)
        {
            LegendaryGame gam = legendary.InstalledGames.Find(x => x.AppName == launchName);

            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Game info",
                ContentMessage = $"Game: {gam.AppTitle}\nLaunch Name: {gam.AppName}\nInstalled version: {gam.InstalledVersion}\nPath: {gam.InstallPath}",
                Style = Style.DarkMode,
                SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = true,
            });

            messageBox.Show();
        }

        public void BtnUpdateSteamGames()
        {
            SteamManager m = new SteamManager();
            m.Read();
            Tuple<int, int> res = m.UpdateWithLegendaryGameList(legendary.InstalledGames, "EpicGames");

            if (res.Item1 != 0 || res.Item2 != 0)
                m.Write();

            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Steam games updated",
                ContentMessage = $"Removed {res.Item1}, Added {res.Item2} on Steam with tag 'EpicGames'\nPlease restart steam for changes to take effect",
                Style = Style.DarkMode,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = true,
            });

            messageBox.Show();
        }

        public void BtnRemoveSteamGames()
        {
            SteamManager m = new SteamManager();
            m.Read();
            int count = m.RemoveAllGamesWithTag("EpicGames");

            if (count != 0)
                m.Write();

            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Steam games removed",
                ContentMessage = $"Removed {count} on Steam with tag 'EpicGames'\nPlease restart steam for changes to take effect",
                Style = Style.DarkMode,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = true,
            });

            messageBox.Show();
        }

        public ObservableCollection<GameModel> Queued { get; }
        public ObservableCollection<GameModel> Installed { get; }
        public ObservableCollection<GameModel> NotInstalled { get; }
    }
}
