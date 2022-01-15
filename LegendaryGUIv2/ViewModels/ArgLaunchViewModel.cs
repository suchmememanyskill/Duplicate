using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using Avalonia.Controls;
using MessageBox.Avalonia.Enums;
using LegendaryGUIv2.Services;
using System.Threading;
using System.Diagnostics;
using Avalonia.Threading;
using LegendaryGUIv2.Models;

namespace LegendaryGUIv2.ViewModels
{
    public class ArgLaunchViewModel : ViewModelBase
    {
        private string cliOut = "";
        private GameLaunchLog log = GameLaunchLog.Get();

        public ArgLaunchViewModel()
        {
            DisplayError();
        }

        public void DisplayError()
        {
            switch (log.State)
            {
                case GameLaunchState.NotInstalled:;
                    Text = $"Game not found: {log.AppName}";
                    break;
                case GameLaunchState.UpdateAvailable:
                    LaunchAvailable = true;
                    Text = $"{log.Game!.AppTitle} has an available update";
                    break;
                case GameLaunchState.LegendaryError:
                    ConsoleAvailable = true;
                    cliOut = $"Standard out:\n{log.MergeStdOut()}\n\nStandard error:\n{log.MergeStdErr()}    ";
                    Text = $"{log.Game!.AppTitle} failed to launch. See console for errors";
                    break;
            }
        }

        public void ExitApplication() => Dispatcher.UIThread.Post(() => App.MainWindow?.Close());
        public void ForceLaunch()
        {
            new ProcessMonitor(log.Game!).SpawnNewAppSkipUpdate();
            ExitApplication();
        }
        public void ViewConsole() => Utils.CreateMessageBox("Console", cliOut).Show();

        private string text = "Launching game...";
        private bool launchAvailable = false, consoleAvailable = false;
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
        public bool LaunchAvailable { get => launchAvailable; set => this.RaiseAndSetIfChanged(ref launchAvailable, value); }
        public bool ConsoleAvailable { get => consoleAvailable; set => this.RaiseAndSetIfChanged(ref consoleAvailable, value); }
    }
}
