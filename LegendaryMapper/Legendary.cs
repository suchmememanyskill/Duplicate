using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace LegendaryMapper
{
    public class Legendary
    {
        public List<LegendaryGame> InstalledGames { get; private set; }
        public List<LegendaryGame> AvailableGames { get; private set; }
        public List<LegendaryGame> NotInstalledGames { 
            get {
                return AvailableGames.Where(x => !InstalledGames.Any(y => x.AppTitle == y.AppTitle)).ToList();
            }
        }

        private Terminal t;

        public Legendary()
        {
            t = Terminal.GetInstance();
            BlockingReload();
        }

        private void ParseInstalled(Legendary w) => 
            InstalledGames = t.StdOut.Skip(t.StdOut.IndexOf("App name,App title,Installed version,Available version,Update available,Install size,Install path") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();

        private void ParseAvailable(Legendary w) =>
            AvailableGames = t.StdOut.Skip(t.StdOut.IndexOf("App name,App title,Version,Is DLC") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();

        public void BlockingReload()
        {
            if (new LegendaryActionBuilder(this, "legendary", "list-installed --csv").Block().Then(ParseInstalled).Start() != LegendaryState.Started)
                throw new Exception("Executable not found");

            LegendaryActionBuilder actionBuilder = new LegendaryActionBuilder(this, "legendary", "list-games --csv").Block().Then(ParseAvailable);

            if (actionBuilder.Start() != LegendaryState.Started)
                throw new Exception("Executable not found");

            if (actionBuilder.ExitCode != 0)
                throw new Exception("Probably not logged in");
        }

        public LegendaryActionBuilder InstallGame(LegendaryGame game)
        {
            LegendaryActionBuilder actionBuilder = new LegendaryActionBuilder(this, "legendary", $"-y install {game.AppName}", Terminal.PrintNewLineStdOut, Terminal.PrintNewLineStdErr);
            actionBuilder.Then(x => x.BlockingReload());

            if (InstalledGames.Any(x => x.AppName == game.AppName))
                throw new Exception("Appname is already present");

            return actionBuilder;
        }

        public LegendaryActionBuilder LaunchGame(LegendaryGame game)
        {
            if (!InstalledGames.Any(x => x.AppName == game.AppName))
                throw new Exception("App is not installed");

            return new LegendaryActionBuilder(this, "legendary", $"launch {game.AppName}", Terminal.PrintNewLineStdOut, Terminal.PrintNewLineStdErr);
        }
    }
}
