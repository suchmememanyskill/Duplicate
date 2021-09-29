using LegendaryGUI.Models;
using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUI.Services
{
    public class FakeDB
    {
        public IEnumerable<GameModel> GetGames()
        {
            Legendary legendary = new Legendary();
            return legendary.InstalledGames.Select(x => new GameModel { AppName = x.AppName, AppTitle = x.AppTitle });
        }
    }
}
