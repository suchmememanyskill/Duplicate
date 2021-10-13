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
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Media;
using LegendaryGUI.Services;
using MessageBox.Avalonia.BaseWindows.Base;

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

            if (lastCheckedPath != PathText && !string.IsNullOrEmpty(PathText))
            {
                if (Directory.Exists(PathText))
                {
                    PathTextColor = greenBrush;
                    lastCheckedPath = PathText;
                    legendary.DownloadManager.InstallPath = lastCheckedPath;
                    File.WriteAllText("./DlPath.txt", lastCheckedPath);
                }
                else
                    PathTextColor = redBrush;
            }
            else if (lastCheckedPath != PathText)
            {
                lastCheckedPath = "";
                legendary.DownloadManager.InstallPath = null;
                File.WriteAllText("./DlPath.txt", "");
            }
        }

        public GameViewModel(Legendary legendary)
        {
            this.legendary = legendary;
            Queued = new ObservableCollection<GameModel>();
            Installed = new ObservableCollection<GameModel>(legendary.InstalledGames.Select(x => new GameModel(x)));
            NotInstalled = new ObservableCollection<GameModel>(legendary.NotInstalledGames.Select(x => new GameModel(x)));

            if (File.Exists("./DlPath.txt"))
            {
                PathTextColor = greenBrush;
                lastCheckedPath = File.ReadAllText("./DlPath.txt");
                PathText = lastCheckedPath;
                if (!string.IsNullOrEmpty(PathText))
                    legendary.DownloadManager.InstallPath = PathText;
            }

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

        private IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Style = Style.DarkMode,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = true,
            });

        public void BtnRemove(string launchName)
        {
            LegendaryGame gam = legendary.InstalledGames.Find(x => x.AppName == launchName);

            var messageBoxOkToRemove = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = "Remove Game?",
                ContentMessage = $"Are you sure you want to remove {gam.AppTitle}?",
                Style = Style.DarkMode,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.Width,
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
        public void BtnStart(string launchName) => new GameLauncherSwitcher(legendary).Launch(launchName, true);
        public void BtnInstall(string launchName)
        {
            try
            {
                legendary.DownloadManager.QueueDownload(legendary.NotInstalledGames.Find(x => x.AppName == launchName));
            } catch (Exception e)
            {
                CreateMessageBox("Error during install", e.Message).Show();
            }
            
        }
        public void BtnUpdate(string launchName)
        {
            try
            {
                legendary.DownloadManager.QueueDownload(legendary.InstalledGames.Find(x => x.AppName == launchName));
            }
            catch (Exception e)
            {
                CreateMessageBox("Error during install", e.Message).Show();
            }
        }
        public void BtnStopDl(string launchName) => legendary.DownloadManager.ActiveDownloads.FirstOrDefault(x => x.Game.AppName == launchName)?.RemoveDownload(legendary.DownloadManager);
        public void BtnPauseDl(string launchName) => legendary.DownloadManager.ActiveDownloads.FirstOrDefault(x => x.Game.AppName == launchName)?.Stop();
        public void BtnStartDl(string launchName) => legendary.DownloadManager.ActiveDownloads.FirstOrDefault(x => x.Game.AppName == launchName)?.Start();

        public void BtnInfo(string launchName)
        {
            LegendaryGame gam = legendary.InstalledGames.Find(x => x.AppName == launchName);
            CreateMessageBox("Game info", $"Game: {gam.AppTitle}\nLaunch Name: {gam.AppName}\nInstalled version: {gam.InstalledVersion}\nPath: {gam.InstallPath}").Show();
        }

        public void BtnUpdateSteamGames()
        {
            SteamManager m = new SteamManager();
            if (!m.Read())
            {
                CreateMessageBox("Fatal error!", $"Failure reading {m.VdfPath}").Show();
                return;
            }
            Tuple<int, int> res = m.UpdateWithLegendaryGameList(legendary.InstalledGames);
            m.Write();

            CreateMessageBox("Steam games updated", $"Removed {res.Item1}, Added {res.Item2} on Steam with '(Epic)' in name\nPlease restart steam for changes to take effect").Show();
        }

        public void BtnRemoveSteamGames()
        {
            SteamManager m = new SteamManager();
            if (!m.Read())
            {
                CreateMessageBox("Fatal error!", $"Failure reading {m.VdfPath}").Show();
                return;
            }
            int count = m.RemoveAllGamesWithTag();

            if (count != 0)
                m.Write();

            CreateMessageBox("Steam games removed", $"Removed {count} on Steam with '(Epic)' in name\nPlease restart steam for changes to take effect").Show();
        }

        public ObservableCollection<GameModel> Queued { get; }
        public ObservableCollection<GameModel> Installed { get; }
        public ObservableCollection<GameModel> NotInstalled { get; }
        private string lastCheckedPath = "";
        public string PathText { get; private set; }
        private IBrush pathTextColor = new SolidColorBrush(Colors.Black);
        public IBrush PathTextColor
        {
            get { return pathTextColor; }
            set
            {
                this.RaiseAndSetIfChanged(ref pathTextColor, value);
            }
        }

        private IBrush greenBrush = new SolidColorBrush(Colors.Green);
        private IBrush redBrush = new SolidColorBrush(Colors.Red);
    }
}
