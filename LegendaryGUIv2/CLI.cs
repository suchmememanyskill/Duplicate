using Avalonia;
using LegendaryGUIv2;
using LegendaryGUIv2.Models;
using LegendaryGUIv2.Services;
using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUIv2
{
    public class CLI
    {
        public static string[]? Args { get; private set; }
        private bool skipUpdate = false;
        private bool watchOnly = false;
        private string appName = "";
        public CLI(string[] incomingArgs)
        {
            Args = incomingArgs;
        }

        public void Handle()
        {
            if (Args!.Length == 0)
            {
                LaunchGUI();
                return;
            }

            else if (Args.Length == 1)
            {
                appName = Args[0];
            }
            else if (Args.Length == 2)
            {
                if (Args[0] == "watch")
                    watchOnly = true;
                else if (Args[0] == "skipupdate")
                    skipUpdate = true;
                else return;

                appName = Args[1];
            }
            else return;

            new LegendaryAuth().AttemptLogin(OnLogin, x => OnLoginError());
        }

        public void OnLogin(LegendaryAuth auth)
        {
            LegendaryGameManager manager = new(auth);
            manager.GetGames();

            if (watchOnly)
                WatchGame(manager);
            else
                LaunchGame(manager);
        }

        public void OnLoginError() => LaunchGUIWithLoginView();

        public void LaunchGame(LegendaryGameManager manager)
        {
            GameLaunchLog log = GameLaunchLog.Get();
            log.AppName = appName;
            log.Auth = manager.Auth;
            log.State = GameLaunchState.Launched;

            LegendaryGame? game = manager.InstalledGames.FirstOrDefault(x => x.AppName == appName);
            
            if (game == null)
            {
                log.State = GameLaunchState.NotInstalled;
                LaunchGUIWithArgLaunchView();
                return;
            }

            log.Game = game;

            if (game.UpdateAvailable && !skipUpdate)
            {
                log.State = GameLaunchState.UpdateAvailable;
                LaunchGUIWithArgLaunchView();
                return;
            }

            ProcessMonitor monitor = new(game);
            monitor.SetStartTime();

            game.LaunchCommand(false, skipUpdate).Then(x =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    monitor.Monitor();
                else
                {
                    monitor.SetEndTime();
                    monitor.Write();
                }
            }).OnError(x =>
            {
                log.StdErr = x.Terminal.StdErr;
                log.StdOut = x.Terminal.StdOut;
                log.State = GameLaunchState.LegendaryError;
                LaunchGUIWithArgLaunchView();
            }).Start();
        }

        public void WatchGame(LegendaryGameManager manager)
        {
            LegendaryGame? game = manager.InstalledGames.FirstOrDefault(x => x.AppName == appName);
            if (game != null)
                new ProcessMonitor(game).Monitor();
        }

        private void LaunchGUI() => Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(new string[] { CLIState.Passtrough.ToString() });
        public void LaunchGUIWithArgLaunchView() => Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(new string[] { CLIState.LaunchError.ToString() });
        public void LaunchGUIWithLoginView() => Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(new string[] { CLIState.NoLogin.ToString() });
    }
}
