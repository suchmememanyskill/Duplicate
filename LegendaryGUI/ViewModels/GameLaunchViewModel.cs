using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUI.ViewModels
{
    public class GameLaunchViewModel : ViewModelBase
    {
        private Legendary legendary;
        private MainWindowViewModel model;

        public GameLaunchViewModel(Legendary legendary, MainWindowViewModel model)
        {
            this.legendary = legendary;
            this.model = model;
        }
    }
}
