using LegendaryMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUI.Models
{
    public class GameModel : INotifyPropertyChanged
    {
        private string appName;
        private string gameSize;
        private string infoText;
        private string launchName;
        private string progressBarText;
        private double progressBarPercent;
        private string dlIndex;

        private bool isInstalled = false;
        private bool isDownloading = false;
        private bool isQueued = false;
        private bool hasUpdate = false;
        private bool isNotInstalled = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void FillWithGameModel(LegendaryGame game)
        {
            AppName = game.AppTitle;
            LaunchName = game.AppName;
            IsQueued = false;
            IsDownloading = false;
            IsInstalled = false;
            IsNotInstalled = false;
            hasUpdate = false;

            if (game.ExtendedInfo)
            {
                IsInstalled = true;
                GameSize = $"{game.InstallSize:0.00} GiB";
                InfoText = "";
                HasUpdate = game.UpdateAvailable;
            }
            else
                IsNotInstalled = true;
        }

        public GameModel(LegendaryGame game)
        {
            FillWithGameModel(game);
        }

        public GameModel(LegendaryDownload download)
            : this(download.Game)
        {
            UpdateDownload(download);
        }

        public void UpdateDownload(LegendaryDownload download)
        {
            DlIndex = $"Download index: {download.DownloadIndex}";
            IsQueued = false;
            IsDownloading = false;
            IsInstalled = false;
            IsNotInstalled = false;
            if (download.IsDownloading)
            {
                TimeSpan t = TimeSpan.FromSeconds(download.SecondsETA);
                ProgressBarText = $"Unpacked: {download.GameSize}, Dl: {download.DownloadSize}, R: {t:hh\\:mm\\:ss}";
                ProgressBarPercent = download.Progress;
                IsDownloading = true;
            }
            else
            {
                IsQueued = true;
            }
        }

        public void OnPropChanged(string value)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(value));
            }
        }

        public string AppName { get => appName; set { appName = value; OnPropChanged("AppName"); }}
        public string GameSize { get => gameSize; set { gameSize = value; OnPropChanged("GameSize"); } }
        public string InfoText { get => infoText; set { infoText = value; OnPropChanged("InfoText"); } }
        public string LaunchName { get => launchName; set { launchName = value; OnPropChanged("LaunchName"); } }
        public string ProgressBarText { get => progressBarText; set { progressBarText = value; OnPropChanged("ProgressBarText"); } }
        public double ProgressBarPercent { get => progressBarPercent; set { progressBarPercent = value; OnPropChanged("ProgressBarPercent"); } }
        public string DlIndex { get => dlIndex; set { dlIndex = value; OnPropChanged("DlIndex"); } }
        public bool IsInstalled { get => isInstalled; set { isInstalled = value; OnPropChanged("IsInstalled"); } }
        public bool IsDownloading { get => isDownloading; set { isDownloading = value; OnPropChanged("IsDownloading"); } }
        public bool HasUpdate { get => hasUpdate; set { hasUpdate = value; OnPropChanged("HasUpdate"); } }
        public bool IsQueued { get => isQueued; set { isQueued = value; OnPropChanged("IsQueued"); } }
        public bool IsNotInstalled { get => isNotInstalled; set { isNotInstalled = value; OnPropChanged("IsNotInstalled"); }
}
    }
}
