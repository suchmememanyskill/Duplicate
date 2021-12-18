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
        public MainViewModel(LegendaryAuth auth)
        {
            this.auth = auth;
            manager = new(auth, x => OnLibraryRefresh());
            manager.GetGames();
        }

        public void OnLibraryRefresh()
        {
            GameCountText = $"Found {manager.Games.Count} games, {manager.InstalledGames.Count} installed";
            Installed = new(manager.Games.Select(x => new GameViewModel(x)));
            new Thread(DownloadAllImages).Start();
        }

        private void DownloadAllImages()
        {
            foreach (GameViewModel model in Installed)
                model.DownloadImages();
        }

        private string gameCountText = "";
        public string GameCountText { get => gameCountText; set => this.RaiseAndSetIfChanged(ref gameCountText, value); }
        private ObservableCollection<GameViewModel> installed = new();
        public ObservableCollection<GameViewModel> Installed { get => installed; set => this.RaiseAndSetIfChanged(ref installed, value); }
        private GameViewModel selectedGame;
        public GameViewModel SelectedGame { get => selectedGame; 
            set {
                selectedGame?.Unselect();
                this.RaiseAndSetIfChanged(ref selectedGame, value);
                selectedGame?.Select();
            }
        }
    }
}
