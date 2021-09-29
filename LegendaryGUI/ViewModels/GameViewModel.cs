using LegendaryGUI.Models;
using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace LegendaryGUI.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private Legendary legendary;

        public GameViewModel()
        {
            legendary = new Legendary();
            Items = new ObservableCollection<GameModel>(legendary.InstalledGames.Select(x => new GameModel { AppName = x.AppName, AppTitle = x.AppTitle }));
        }

        public void Test(string game)
        {
            legendary.LaunchGame(legendary.InstalledGames.Find(x => x.AppName == game)).Block().Start();
        }

        public void Incr()
        {
            Items[0].AppTitle = "yeet";
            Items.Add(new GameModel { AppName = legendary.InstalledGames[0].AppName, AppTitle = legendary.InstalledGames[0].AppTitle });
            //this.RaisePropertyChanged();
        }



        public ObservableCollection<GameModel> Items { get; }
    }
}
