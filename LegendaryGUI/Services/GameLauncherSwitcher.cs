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

        public GameLauncherSwitcher(Legendary legendary)
        {
            this.legendary = legendary;
        }

        public GameState Launch(LegendaryGame game, bool skipUpdateCheck = false)
        {
            LegendaryGame gam = legendary.InstalledGames.FirstOrDefault(x => x.AppName == game.AppName);
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
            if (action.ExitCode != 0)
                return GameState.UnknownError;

            return GameState.Launched;
        }
    }
}
