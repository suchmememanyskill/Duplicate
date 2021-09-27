using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

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

        public DownloadManager DownloadManager { get; private set; }

        public Legendary()
        {
            BlockingReload();
            DownloadManager = new DownloadManager(this);
        }

        private void ParseInstalled(LegendaryActionBuilder action) => 
            InstalledGames = action.Terminal.StdOut.Skip(action.Terminal.StdOut.IndexOf("App name,App title,Installed version,Available version,Update available,Install size,Install path") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();

        private void ParseAvailable(LegendaryActionBuilder action) =>
            AvailableGames = action.Terminal.StdOut.Skip(action.Terminal.StdOut.IndexOf("App name,App title,Version,Is DLC") + 1)
                .Select(x => new LegendaryGame(CSVParser.Parse(x, ','))).ToList();

        public void BlockingReload()
        {
            LegendaryActionBuilder actionGetInstalled = new LegendaryActionBuilder(this, "legendary", "list-installed --csv").Then(ParseInstalled);
            if (actionGetInstalled.Start() != LegendaryState.Started)
                throw new Exception("Executable not found");

            LegendaryActionBuilder actionGetAvailable = new LegendaryActionBuilder(this, "legendary", "list-games --csv").Then(ParseAvailable);

            if (actionGetAvailable.Start() != LegendaryState.Started)
                throw new Exception("Executable not found");

            if (actionGetAvailable.ExitCode != 0)
                throw new Exception("Probably not logged in");

            actionGetInstalled.WaitUntilCompletion();
            actionGetAvailable.WaitUntilCompletion();
        }

        public LegendaryActionBuilder RemoveGame(LegendaryGame game)
        {
            if (!InstalledGames.Any(x => x.AppName == game.AppName))
                throw new Exception("Game is not installed");

            return new LegendaryActionBuilder(this, "legendary", $"-y uninstall {game.AppName}").Then(x => x.Legendary.BlockingReload());
        }

        public LegendaryActionBuilder LaunchGame(LegendaryGame game)
        {
            if (!InstalledGames.Any(x => x.AppName == game.AppName))
                throw new Exception("App is not installed");

            return new LegendaryActionBuilder(this, "legendary", $"launch {game.AppName}").OnNewLine(LegendaryActionBuilder.PrintNewLineStdOut).OnErrLine(LegendaryActionBuilder.PrintNewLineStdErr);
        }
    }
}
