using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUI.Services
{
    public enum GameState
    {
        Launched,
        UpdateRequired,
        NotInstalled,
        UnknownError
    }

    public class GameLauncherSwitcher
    {
        private Legendary legendary;
        public string stdOutput { get; private set; }
        public string stdError { get; private set; }
        public LegendaryGame LaunchedGame { get; private set; }
        

        public GameLauncherSwitcher(Legendary legendary)
        {
            this.legendary = legendary;
        }

        public GameState Launch(string appName, bool skipUpdateCheck = false)
        {
            LegendaryGame gam = legendary.InstalledGames.FirstOrDefault(x => x.AppName == appName);
            LaunchedGame = gam;

            if (gam == null)
                return GameState.NotInstalled;

            bool amOffline = !CheckIfOnline.CheckForInternetConnection();

            string args = "";

            if (amOffline)
            {
                args += "--offline";
            }
            else
            {
                if (skipUpdateCheck)
                    args += "--skip-version-check";
                else
                {
                    if (gam.UpdateAvailable)
                        return GameState.UpdateRequired;
                }
            }

            LegendaryActionBuilder action = legendary.LaunchGame(gam, args).Block();
            action.Start();

            stdOutput = string.Join("\n", action.Terminal.StdOut);
            stdError = string.Join("\n", action.Terminal.StdErr);

            if (action.ExitCode != 0)
                return GameState.UnknownError;

            return GameState.Launched;
        }
    }
}
