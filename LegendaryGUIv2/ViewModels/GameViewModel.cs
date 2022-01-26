using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using LegendaryMapperV2.Service;
using ReactiveUI;
using System.Threading;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Avalonia.Controls;
using LegendaryGUIv2.Services;
using Avalonia.Threading;
using System.Runtime.InteropServices;

namespace LegendaryGUIv2.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private LegendaryGame game;
        private LegendaryDownload? download;
        private MainViewModel mainView;
        public GameViewModel(LegendaryGame game, MainViewModel mainView)
        {
            this.mainView = mainView;
            this.game = game;
            if (game.IsInstalled)
            {
                Installed = true;
                if (game.UpdateAvailable)
                    updateAvailable = true;
                GameSize = game.InstallSizeReadable;
            }

            else
                NotInstalled = true;
        }

        public void Select()
        {
            Selected = true;
            Debug.WriteLine($"Selected {GameName}");
        }

        public void Unselect()
        {
            Selected = false;
            Debug.WriteLine($"Unselected {GameName}");
        }

        public void ApplyDownload(LegendaryDownload download)
        {
            this.download = download;
            Installed = false;
            UpdateAvailable = false;
            NotInstalled = false;
            Downloading = true;
            DownloadPaused = true;
            DownloadNotPaused = false;
            download.OnUpdate = x => UpdateDownloadData();
        }

        public void DownloadImages()
        {
            if (game.GameBannerTall != null)
            {
                Stream stream = new MemoryStream(game.GameBannerTall.GetImage());
                Cover = Avalonia.Media.Imaging.Bitmap.DecodeToHeight(stream, 300);
            }

            if (game.GameLogo != null)
            {
                Stream stream = new MemoryStream(game.GameLogo.GetImage());
                Icon = Avalonia.Media.Imaging.Bitmap.DecodeToWidth(stream, 200);
            }
        }
        public void Play()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // This os specific code is because linux keeps the legendary command active. This means if you close the GUI, the app itself won't close
            {
                game.LaunchCommand().OnError(x =>
                    Dispatcher.UIThread.Post(() =>
                    Utils.CreateMessageBox("An error occured!", $"Something went wrong while launching {game.AppTitle}!\nCommand: legendary {x.Arguments}\n\nStandard out:\n{string.Join('\n', x.Terminal.StdOut)}\n\nStandard error:\n{string.Join('\n', x.Terminal.StdErr)}    ").Show())
                ).Start();
                new ProcessMonitor(game).SpawnNewWatchApp();
            }
            else new ProcessMonitor(game).SpawnNewAppSkipUpdate();
        }

        public void Info() => mainView.SetViewOnWindow(new GameInfoViewModel(mainView, this));
        public void Install()
        {
            LegendaryDownload dl = game.InstantiateDownload();
            dl.Action.OnError(x =>
                Dispatcher.UIThread.Post(() =>
                    Utils.CreateMessageBox($"An error occured during downloading {game.AppTitle}!", $"Command: legendary {x.Arguments}\n\nStandard out:\n{string.Join('\n', x.Terminal.StdOut)}\n\nStandard error:\n{string.Join('\n', x.Terminal.StdErr)}    ").Show()
                )
            );
            dl.Start();
        }

        public void Pause()
        {
            download?.Pause();
            DownloadPaused = true;
            DownloadNotPaused = false;
        }

        public void Continue()
        {
            download?.Start();
            DownloadPaused = false;
            DownloadNotPaused = true;
        }

        public void Stop() => download?.Stop();

        private void UpdateDownloadData()
        {
            if (download!.IsDownloading)
            {
                DownloadPaused = false;
                DownloadNotPaused = true;
                DownloadProgress = download!.Progress;
                DownloadSize = $"Download: {download!.DownloadSize}";
                TimeSpan t = TimeSpan.FromSeconds(download!.SecondsETA);
                DownloadRemainingTime = $"Remaining: {t:hh\\:mm\\:ss}";
                DownloadUnpackedSize = download!.GameSize;
            }
            else
            {
                DownloadPaused = true;
                DownloadNotPaused = false;
            }
        }

        public LegendaryGame Game { get => game; }
        public string GameName { get => game.AppTitle; }
        public static IBrush HalfTransparency { get; } = new SolidColorBrush(Avalonia.Media.Color.FromArgb(128,0,0,0));
        public static IBrush ThreeFourthsTransparency { get; } = new SolidColorBrush(Avalonia.Media.Color.FromArgb(196, 0, 0, 0));

        private Avalonia.Media.Imaging.Bitmap? cover;
        public Avalonia.Media.Imaging.Bitmap? Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }
        private Avalonia.Media.Imaging.Bitmap? icon;
        public Avalonia.Media.Imaging.Bitmap? Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
        private bool selected = false, installed = false, notInstalled = false, downloading = false, downloadPaused = false, downloadNotPaused = false, updateAvailable = false, visible = true;
        public bool Selected { get => selected; set => this.RaiseAndSetIfChanged(ref selected, value); }
        public bool Installed { get => installed; set => this.RaiseAndSetIfChanged(ref installed, value); }
        public bool NotInstalled { get => notInstalled; set => this.RaiseAndSetIfChanged(ref notInstalled, value); }
        public bool Downloading { get => downloading; set => this.RaiseAndSetIfChanged(ref downloading, value); }
        public bool DownloadPaused { get => downloadPaused; set => this.RaiseAndSetIfChanged(ref downloadPaused, value); }
        public bool DownloadNotPaused { get => downloadNotPaused; set => this.RaiseAndSetIfChanged(ref downloadNotPaused, value); }
        public bool UpdateAvailable { get => updateAvailable; set => this.RaiseAndSetIfChanged(ref updateAvailable, value); }
        public string GameSize { get; } = "";
        private double downloadProgress = 0.00;
        private string downloadSize = "", downloadRemainingTime = "", downloadUnpackedSize = "";
        public double DownloadProgress { get => downloadProgress; set => this.RaiseAndSetIfChanged(ref downloadProgress, value); }
        public string DownloadSize { get => downloadSize; set => this.RaiseAndSetIfChanged(ref downloadSize, value); }
        public string DownloadUnpackedSize { get => downloadUnpackedSize; set => this.RaiseAndSetIfChanged(ref downloadUnpackedSize, value); }
        public string DownloadRemainingTime { get => downloadRemainingTime; set => this.RaiseAndSetIfChanged(ref downloadRemainingTime, value); }
        public bool Visible { get => visible; set => this.RaiseAndSetIfChanged(ref visible, value); }
    }
}
