using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LegendaryMapperV2.Service;
using System.Threading.Tasks;
using ReactiveUI;

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
        }

        private string gameCountText = "";
        public string GameCountText { get => gameCountText; set => this.RaiseAndSetIfChanged(ref gameCountText, value); }
    }
}
