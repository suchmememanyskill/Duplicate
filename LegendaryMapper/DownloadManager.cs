using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace LegendaryMapper
{
    public class DownloadManager
    {
        public bool IsDownloading { get { return activeDownloads.Any(x => x.IsDownloading); } }
        public string InstallPath { get; set; } = null;

        private Legendary legendary;
        private List<LegendaryDownload> activeDownloads = new List<LegendaryDownload>();

        public DownloadManager(Legendary legendary)
        {
            this.legendary = legendary;
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
