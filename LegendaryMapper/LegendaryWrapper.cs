using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace LegendaryMapper
{
    public class LegendaryWrapper
    {
        public bool Installed { get; private set; }
        public bool LoggedIn { get; private set; }
        public List<LegendaryGame> InstalledGames { get; private set; }
        public List<LegendaryGame> AvailableGames { get; private set; }
        public List<LegendaryGame> NotInstalledGames { 
            get {
                return AvailableGames.Where(x => !InstalledGames.Any(y => x.AppTitle == y.AppTitle)).ToList();
            }
        }

        private Terminal t;

        public LegendaryWrapper()
        {
            t = Terminal.GetInstance();
            BlockingReload();
        }

        private void ParseInstalled(Terminal t) => 
            InstalledGames = t.StdOut.Skip(t.StdOut.IndexOf("App name,App title,Installed version,Available version,Update available,Install size,Install path") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();

        private void ParseAvailable(Terminal t)
        {
            if (t.ExitCode != 0)
            {
                LoggedIn = false;
                return;
            }

            LoggedIn = true;

            AvailableGames = t.StdOut.Skip(t.StdOut.IndexOf("App name,App title,Version,Is DLC") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();
        }

        public void BlockingReload()
        {
            Terminal t = Terminal.GetInstance();
            Installed = (t.Exec("legendary", "list-installed --csv") >= 0);
            if (!Installed)
                return;

            while (t.IsActive) ;
            ParseInstalled(t);
            Installed = (t.Exec("legendary", "list-games --csv") >= 0);
            while (t.IsActive) ;
            ParseAvailable(t);
        }

        public delegate void LegendaryCallback(LegendaryWrapper legendary);

        private LegendaryCallback installGameCallback = null;
        private void InstallGameCallback(Terminal t)
        {
            BlockingReload();
            installGameCallback?.Invoke(this);
        }
        // Maybe add syntax like .InstallGame().Callback() or .Block()
        public LegendaryState InstallGame(LegendaryGame game, LegendaryCallback callback = null)
        {
            Terminal t = Terminal.GetInstance();
            installGameCallback = callback;

            if (t.IsActive)
                return LegendaryState.Active;

            if (!InstalledGames.Any(x => x.AppName == game.AppName))
            {
                t.Exec("legendary", $"-y install {game.AppName}", InstallGameCallback, Terminal.PrintNewLineStdOut, Terminal.PrintNewLineStdErr);
                return LegendaryState.Started;
            }
            else
                return LegendaryState.InvalidInput;
        }

        public LegendaryState LaunchGame(LegendaryGame game)
        {
            Terminal t = Terminal.GetInstance();

            if (t.IsActive)
                return LegendaryState.Active;

            if (InstalledGames.Any(x => x.AppName == game.AppName))
            {
                t.Exec("legendary", $"launch {game.AppName}", null, Terminal.PrintNewLineStdOut, Terminal.PrintNewLineStdErr);
                return LegendaryState.Started;
            }
            else
                return LegendaryState.InvalidInput;
        }
    }
}
