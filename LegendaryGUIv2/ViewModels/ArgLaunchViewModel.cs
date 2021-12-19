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

namespace LegendaryGUIv2.ViewModels
{
    public class ArgLaunchViewModel : ViewModelBase
    {
        private LegendaryAuth auth;
        private string[] args;
        private LegendaryGame? game;
        private string cliOut;
        public ArgLaunchViewModel(LegendaryAuth auth, string[] args)
        {
            this.auth = auth;
            this.args = args;
            AttemptLaunch();
        }

        public void AttemptLaunch()
        {
            string gameName = args[0];
            LegendaryGameManager manager = new(auth);
            manager.GetGames();
            game = manager.InstalledGames.FirstOrDefault(x => x.AppName == gameName);
            if (game == null)
            {
                Text = $"Game not found: {gameName}";
                return;
            }

            if (game.UpdateAvailable)
            {
                LaunchAvailable = true;
                Text = $"{game.AppTitle} has an available update";
                return;
            }

            game.LaunchCommand().Then(x => ExitApplication()).OnError(x =>
            {
                ConsoleAvailable = true;
                cliOut = $"Standard out:\n{string.Join('\n', x.Terminal.StdOut)}\n\nStandard error:\n{string.Join('\n', x.Terminal.StdErr)}    ";
                Text = $"{game.AppTitle} failed to launch. See console for errors";
            }).Start();
        }

        public void ExitApplication() => Environment.Exit(0);
        public void ForceLaunch() => game?.LaunchCommand(false, true).Then(x => ExitApplication()).Start();
        public void ViewConsole() => CreateMessageBox("Console", cliOut).Show();

        private IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message, ButtonEnum buttons = ButtonEnum.Ok) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title,
                ContentMessage = message,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
            });

        private string text = "Launching game...";
        private bool launchAvailable = false, consoleAvailable = false;
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
        public bool LaunchAvailable { get => launchAvailable; set => this.RaiseAndSetIfChanged(ref launchAvailable, value); }
        public bool ConsoleAvailable { get => consoleAvailable; set => this.RaiseAndSetIfChanged(ref consoleAvailable, value); }
    }
}
