using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace LegendaryGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            List = new GameViewModel();
        }

        public GameViewModel List { get; }
    }
}
