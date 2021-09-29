using LegendaryGUI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace LegendaryGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(FakeDB db)
        {
            List = new GameViewModel();
        }

        public GameViewModel List { get; }

        public void Test(string test) => List.Test(test);
    }
}
