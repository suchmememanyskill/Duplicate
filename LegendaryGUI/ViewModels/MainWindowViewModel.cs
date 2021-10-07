using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.ApplicationLifetimes;
using LegendaryMapper;
using ReactiveUI;

namespace LegendaryGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Legendary legendary;
        public MainWindowViewModel(Legendary legendary, IClassicDesktopStyleApplicationLifetime desktop)
        {
            this.legendary = legendary;
            GameLaunchViewModel launch = new(legendary, this, desktop.Args);
            if (launch.LaunchViewRequired)
            {
                View = _view = launch;
            }
            else
                EndGameLaunchView();
        }

        public void EndGameLaunchView()
        {
            WindowWidth = 1280;
            WindowHeight = 720;
            View = new GameViewModel(legendary);
        }

        private ViewModelBase _view;
        private int windowWidth = 300;
        private int windowHeight = 150;  
        public ViewModelBase View { get => _view; private set => this.RaiseAndSetIfChanged(ref _view, value); }
        public int WindowWidth { get => windowWidth; private set => this.RaiseAndSetIfChanged(ref windowWidth, value); }
        public int WindowHeight { get => windowHeight; private set => this.RaiseAndSetIfChanged(ref windowHeight, value); }
    }
}
