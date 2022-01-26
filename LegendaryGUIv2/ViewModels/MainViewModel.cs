using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LegendaryMapperV2.Service;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading;
using LegendaryGUIv2.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Threading;
using System.Reactive;
using System.Reactive.Linq;
using LegendaryGUIv2.Views;
using System.IO;

namespace LegendaryGUIv2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private LegendaryAuth auth;
        private LegendaryGameManager manager;
        private MainWindowViewModel window;
        private ConsoleViewModel consoleView;
        private LegendaryMisc misc;
        public LegendaryGameManager Manager { get => manager; }
        public LegendaryAuth Auth { get => auth; }
        private Thread? imageDownloadThread;
        private bool stopImageDownloadThread = false;

        public MainViewModel(LegendaryAuth auth, MainWindowViewModel window)
        {
            this.auth = auth;
            this.window = window;
            manager = new(auth, x => OnLibraryRefresh());
            manager.GetGames();
            SetDownloadLocationText();
            consoleView = new(window);
            misc = new(auth);

            this.WhenAnyValue(x => x.SearchBoxText)
                //.Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(HandleSearchEvent!);
        }

        public void OnPathChange() => SetViewOnWindow(new ChangeGameFolderViewModel(this));
        public void SetViewOnWindow(ViewModelBase view) => window.SetViewModel(view);

        public void RefreshLibrary() => new LegendaryCommand("list-games").Then(x => manager.GetGames()).Start();

        public void OnLibraryRefresh()
        {

            if (imageDownloadThread != null && imageDownloadThread.IsAlive)
            {
                stopImageDownloadThread = true;
                imageDownloadThread.Join();
            }

            GameCountText = $"Found {manager.Games.Count} games, {manager.InstalledGames.Count} installed";
            Installed = new(manager.InstalledGames.Select(x => new GameViewModel(x, this)));
            NotInstalled = new(manager.NotInstalledGames.Select(x => new GameViewModel(x, this)));

            List<GameViewModel> transferList = new();
            manager.Downloads.ForEach(x =>
            {
                if (Installed.Any(y => x.Game.AppName == y.Game.AppName))
                    Installed.First(y => x.Game.AppName == y.Game.AppName).ApplyDownload(x);
                else if (NotInstalled.Any(y => x.Game.AppName == y.Game.AppName))
                {
                    GameViewModel model = NotInstalled.First(y => x.Game.AppName == y.Game.AppName);
                    transferList.Add(model);
                    model.ApplyDownload(x);
                }
            });

            int i = 0;
            transferList.ForEach(x =>
            {
                Installed.Insert(i++, x);
                NotInstalled.Remove(x);
            });

            stopImageDownloadThread = false;
            imageDownloadThread = new(DownloadAllImages);
            imageDownloadThread.IsBackground = true;
            imageDownloadThread.Start();

            MainWindow.gameViewModels = Installed.Concat(notInstalled).ToList();
        }

        public void BtnUpdateSteamGames()
        {
            SteamManager m = new SteamManager();

            if (!m.InitialisePaths())
            {
                Utils.CreateMessageBox("Fatal error!", "Failure getting steam paths. Is steam running?").Show();
                return;
            }

            if (!m.Read())
            {
                Utils.CreateMessageBox("Fatal error!", $"Failure reading {m.VdfPath}.\nDo you have at least 1 non-steam game shortcut in steam?    ").Show();
                return;
            }
            Tuple<int, int> res = m.UpdateWithLegendaryGameList(manager.InstalledGames);
            m.Write();

            Utils.CreateMessageBox("Steam games updated", $"Removed {res.Item1}, Added {res.Item2} on Steam with '(Epic)' in name\nPlease restart steam for changes to take effect").Show();
        }

        public void BtnRemoveSteamGames()
        {
            SteamManager m = new SteamManager();
            if (!m.InitialisePaths())
            {
                Utils.CreateMessageBox("Fatal error!", "Failure getting steam paths. Is steam running?").Show();
                return;
            }
            if (!m.Read())
            {
                Utils.CreateMessageBox("Fatal error!", $"Failure reading {m.VdfPath}").Show();
                return;
            }
            int count = m.RemoveAllGamesWithTag();

            if (count != 0)
                m.Write();

            Utils.CreateMessageBox("Steam games removed", $"Removed {count} on Steam with '(Epic)' in name\nPlease restart steam for changes to take effect").Show();
        }

        public void HandleSearchEvent(string search)
        {
            if (string.IsNullOrWhiteSpace(SearchBoxText))
                Installed.Concat(NotInstalled).ToList().ForEach(x => x.Visible = true);
            else
                Installed.Concat(NotInstalled).ToList().ForEach(x => x.Visible = x.Game.AppTitle.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase));
        }

        private void DownloadAllImages()
        {
            foreach (GameViewModel model in Installed.Concat(NotInstalled))
            {
                if (stopImageDownloadThread)
                    return;

                try
                {
                    model.DownloadImages();
                }
                catch { }
            }
        }

        public void SetDownloadLocationText()
        {
            if (manager.GameDirectory != "")
                DownloadLocation = $"Download location: {manager.GameDirectory}";
            else
                DownloadLocation = $"Download location: ~/Legendary";
        }

        public void OpenFreeGame() => Utils.OpenUrl("https://www.epicgames.com/store/en-US/free-games");
        public void OpenSource() => Utils.OpenUrl("https://github.com/suchmememanyskill/Duplicate");
        public void OpenLegendaryConfig() => Utils.OpenFolder(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "legendary", "config.ini"));
        public void OpenLegendaryConfigDir() => Utils.OpenFolder(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "legendary"));
        public void OpenDuplicateConfigDir() => Utils.OpenFolder(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Duplicate"));
        public void EosOverlayInstall() => consoleView.ExecuteCommand("Installing/Updating EOS Overlay...", misc.EosOverlayInstall(manager.GameDirectory), this);
        public void EosOverlayRemove() => consoleView.ExecuteCommand("Removing EOS Overlay...", misc.EosOverlayRemove(), this);
        public void EosOverlayInfo() => consoleView.ExecuteCommand("EOS Overlay info", misc.EosOverlayInfo(), this);
        public void Exit() => App.MainWindow?.Close();

        private string gameCountText = "";
        public string GameCountText { get => gameCountText; set => this.RaiseAndSetIfChanged(ref gameCountText, value); }

        private string downloadLocation = "";
        public string DownloadLocation { get => downloadLocation; set => this.RaiseAndSetIfChanged(ref downloadLocation, value); }

        private ObservableCollection<GameViewModel> installed = new();
        public ObservableCollection<GameViewModel> Installed { get => installed; set => this.RaiseAndSetIfChanged(ref installed, value); }
        private ObservableCollection<GameViewModel> notInstalled = new();
        public ObservableCollection<GameViewModel> NotInstalled { get => notInstalled; set => this.RaiseAndSetIfChanged(ref notInstalled, value); }
        private GameViewModel? selectedGameInstalled, selectedGameNotInstalled;
        public GameViewModel? SelectedGameInstalled { 
            get => selectedGameInstalled; 
            set {
                selectedGameInstalled?.Unselect();
                if (value != null)
                    SelectedGameNotInstalled = null;
                this.RaiseAndSetIfChanged(ref selectedGameInstalled, value);
                selectedGameInstalled?.Select();
            }
        }
        public GameViewModel? SelectedGameNotInstalled
        {
            get => selectedGameNotInstalled;
            set
            {
                selectedGameNotInstalled?.Unselect();
                if (value != null)
                    SelectedGameInstalled = null;
                this.RaiseAndSetIfChanged(ref selectedGameNotInstalled, value);
                selectedGameNotInstalled?.Select();
            }
        }

        private string searchBoxText = "";
        public string SearchBoxText { get => searchBoxText; set => this.RaiseAndSetIfChanged(ref searchBoxText, value); }
    }
}
