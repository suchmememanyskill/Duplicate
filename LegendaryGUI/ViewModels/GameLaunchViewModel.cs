using LegendaryGUI.Services;
using LegendaryMapper;
using ReactiveUI;
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
        private LegendaryGame game;

        public bool LaunchViewRequired { get; private set; } = false;

        public GameLaunchViewModel(Legendary legendary, MainWindowViewModel model, string[] args)
        {
            this.legendary = legendary;
            this.model = model;

            if (args.Length != 1)
                return;
            
            game = legendary.InstalledGames.Find(x => x.AppName == args[0]);

            if (game == null)
                return;

            GameLauncherSwitcher gameLaunch = new GameLauncherSwitcher(legendary);
            GameState state = gameLaunch.Launch(game);
            if (state == GameState.Launched)
                ExitApplication();
            else if (state == GameState.UpdateRequired) {
                LaunchViewRequired = true;
                WindowText = $"Launched game {game.AppTitle}\nhas an update!\n\nLaunch the GUI to update";
            }
            else
                return;
        }

        public void ExitApplication() => Environment.Exit(0);
        public void ForceLaunch()
        {
            GameLauncherSwitcher gameLaunch = new GameLauncherSwitcher(legendary);
            gameLaunch.Launch(game, true);
            ExitApplication();
        }

        private string windowText = "";
        public string WindowText { get => windowText; private set => this.RaiseAndSetIfChanged(ref windowText, value); }
    }
}
