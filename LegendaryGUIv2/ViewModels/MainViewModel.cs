using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LegendaryMapperV2.Service;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading;

namespace LegendaryGUIv2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private LegendaryAuth auth;
        private LegendaryGameManager manager;
        private Thread? imageDownloadThread;
        private bool stopImageDownloadThread = false;
        public MainViewModel(LegendaryAuth auth)
        {
            this.auth = auth;
            manager = new(auth, x => OnLibraryRefresh());
            manager.GetGames();
        }

        public void RefreshLibrary() => manager.GetGames();

        public void OnLibraryRefresh()
        {
            if (imageDownloadThread != null && imageDownloadThread.IsAlive)
            {
                stopImageDownloadThread = true;
                imageDownloadThread.Join();
            }

            GameCountText = $"Found {manager.Games.Count} games, {manager.InstalledGames.Count} installed";
            Installed = new(manager.InstalledGames.Select(x => new GameViewModel(x)));
            NotInstalled = new(manager.NotInstalledGames.Select(x => new GameViewModel(x)));

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

        private string gameCountText = "";
        public string GameCountText { get => gameCountText; set => this.RaiseAndSetIfChanged(ref gameCountText, value); }
        private ObservableCollection<GameViewModel> installed = new();
        public ObservableCollection<GameViewModel> Installed { get => installed; set => this.RaiseAndSetIfChanged(ref installed, value); }
        private ObservableCollection<GameViewModel> notInstalled = new();
        public ObservableCollection<GameViewModel> NotInstalled { get => notInstalled; set => this.RaiseAndSetIfChanged(ref notInstalled, value); }
        private GameViewModel? selectedGameInstalled, selectedGameNotInstalled;
        public GameViewModel? SelectedGameInstalled { 
            get => selectedGameInstalled; 
            set {
                selectedGameInstalled?.Unselect();
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
                this.RaiseAndSetIfChanged(ref selectedGameNotInstalled, value);
                selectedGameNotInstalled?.Select();
            }
        }
    }
}
