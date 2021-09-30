using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;

namespace LegendaryMapper
{
    public class DownloadManager
    {
        public bool IsDownloading { get { return activeDownloads.Any(x => x.IsDownloading); } }
        public string InstallPath { get; set; } = null;

        private Legendary legendary;
        private List<LegendaryDownload> activeDownloads = new List<LegendaryDownload>();
        public ReadOnlyCollection<LegendaryDownload> ActiveDownloads { get { return activeDownloads.AsReadOnly(); } }

        public DownloadManager(Legendary legendary)
        {
            this.legendary = legendary;
        }

        public void WaitUntilCompletion()
        {
            while (activeDownloads.Count > 0) ;
        }

        public void NotifyCompletion(LegendaryDownload download)
        {
            activeDownloads.Remove(download);
            activeDownloads.FirstOrDefault()?.Start();
        }

        public void StartDownloads()
        {
            if (!IsDownloading)
            {
                activeDownloads.First().Start();
            }
        }

        public void RemoveDownload(LegendaryDownload game)
        {
            game.Stop();
            activeDownloads.Remove(game);
        }

        private void MoveGame(LegendaryGame game, int offset)
        {
            if (game == null)
                throw new Exception("Game is null");

            if (!activeDownloads.Any(x => x.Game.AppName == game.AppName))
                throw new Exception("Game is not being downloaded");

            int idx = activeDownloads.FindIndex(x => x.Game.AppName == game.AppName);

            if (idx + offset < 0 || idx + offset > activeDownloads.Count)
                throw new Exception("Index out of range");

            LegendaryDownload swapFrom = activeDownloads[idx];
            LegendaryDownload swapTo = activeDownloads[idx + offset];

            activeDownloads[idx + offset] = swapFrom;
            activeDownloads[idx] = swapTo;

            if (offset < 0 && swapTo.IsDownloading)
            {
                swapTo.Stop();
                swapFrom.Start();
            }
            else if (offset > 0 && swapFrom.IsDownloading)
            {
                swapFrom.Stop();
                swapTo.Start();
            }
        }

        public void MoveGameUp(LegendaryGame game) => MoveGame(game, -1);

        public void MoveGameDown(LegendaryGame game) => MoveGame(game, 1);

        public void StopDownloads() => activeDownloads.Where(x => x.IsDownloading).ToList().ForEach(x => x.Stop());

        public void QueueDownload(LegendaryGame game)
        {
            if (game == null)
                throw new Exception("Input was null");

            if (activeDownloads.Any(x => x.Game.AppName == game.AppName))
                throw new Exception("Game is already downloading");

            string extra = "";
            if (InstallPath != null)
            {
                if (!Directory.Exists(InstallPath))
                    throw new Exception("Install location is not valid");

                extra += $"--game-folder {InstallPath}";
            }

            LegendaryActionBuilder actionBuilder = new LegendaryActionBuilder(legendary, "legendary", $"-y install {game.AppName} {extra}").OnNewLine(LegendaryActionBuilder.PrintNewLineStdOut).OnErrLine(LegendaryActionBuilder.PrintNewLineStdErr);
            actionBuilder.Then(x => x.Legendary.BlockingReload());

            if (legendary.InstalledGames.Any(x => x.AppName == game.AppName))
                throw new Exception("Appname is already present");

            activeDownloads.Add(new LegendaryDownload(actionBuilder, this, game));

            if (!IsDownloading)
            {
                activeDownloads.First().Start();
            }
        }

    }
}
