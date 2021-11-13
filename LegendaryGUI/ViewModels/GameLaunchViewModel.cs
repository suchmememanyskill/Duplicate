using Avalonia.Controls;
using LegendaryGUI.Services;
using LegendaryMapper;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace LegendaryGUI.ViewModels
{
    public class GameLaunchViewModel : ViewModelBase
    {
        private Legendary legendary;
        private MainWindowViewModel model;
        private bool updateAvailable = false;
        private bool launchAvailable = false;
        private bool consoleAvailable = false;
        private bool openAvailable = false;
        private string gameName;
        public bool LaunchViewRequired { get; private set; } = false;
        private Timer timer;

        public GameLaunchViewModel(Legendary legendary, MainWindowViewModel model, string[] args)
        {
            this.legendary = legendary;
            this.model = model;

            if (legendary.InstalledGames == null)
            {
                LaunchViewRequired = true;
                WindowText = $"[Fatal] Unable to retrieve installed games";
                return;
            }
            else if (legendary.AvailableGames == null)
            {
                LaunchViewRequired = true;
                openAvailable = true;
                WindowText = $"Unable to retrieve available games.\nAre you offline?";
                return;
            }

            consoleAvailable = true;
            openAvailable = true;

            if (args.Length != 1)
                return;

            gameName = args[0];

            GameLauncherSwitcher gameLaunch = new GameLauncherSwitcher(legendary);
            GameState state = gameLaunch.Launch(gameName);

            LaunchViewRequired = true;

            Console = $"Standard out:\n{gameLaunch.stdOutput}\n\nStandard err:{gameLaunch.stdError}";

            if (state == GameState.Launched)
            {
                WindowText = $"Launched game {gameLaunch.LaunchedGame.AppTitle}\nWindow will close in 5 seconds";

                timer = new Timer(5500);
                timer.Elapsed += new ElapsedEventHandler(ExitApplicationFancy);
                timer.Enabled = true;
            }
            else if (state == GameState.UpdateRequired) {
                
                WindowText = $"Launched game {gameLaunch.LaunchedGame.AppTitle}\nhas an update!\n\nLaunch the GUI to update";
                updateAvailable = true;
                LaunchAvailable = true;
            }
            else
            {
                if (state == GameState.NotInstalled)
                    WindowText = "A fatal error occured:\nThe game is not installed";
                else
                    WindowText = "A fatal error occured:\nSee the console for errors";
            }
        }

        public void ExitApplicationFancy(object sender, ElapsedEventArgs e) => ExitApplication();
        public void StopTimer()
        {
            if (timer != null)
                timer.Enabled = false;
        }
        public void ExitApplication() => Environment.Exit(0);

        public void ForceLaunch()
        {
            GameLauncherSwitcher gameLaunch = new GameLauncherSwitcher(legendary);
            gameLaunch.Launch(gameName, true);
            ExitApplication();
        }

        public void ViewConsole()
        {
            StopTimer();
            CreateMessageBox("Console", Console).Show();
        }

        private IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Style = Style.DarkMode,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = true,
            });

        private string windowText = "";
        public string WindowText { get => windowText; private set => this.RaiseAndSetIfChanged(ref windowText, value); }
        public bool UpdateAvailable { get => updateAvailable; private set => this.RaiseAndSetIfChanged(ref updateAvailable, value); }
        public bool LaunchAvailable { get => launchAvailable; private set => this.RaiseAndSetIfChanged(ref launchAvailable, value); }
        public string Console { get; private set; }
        public bool ConsoleAvailable { get => consoleAvailable; private set => this.RaiseAndSetIfChanged(ref consoleAvailable, value); }
        public bool OpenAvailable { get => openAvailable; private set => this.RaiseAndSetIfChanged(ref openAvailable, value); }
    }
}
