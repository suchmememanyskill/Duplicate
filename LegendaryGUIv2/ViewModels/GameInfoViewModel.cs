using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LegendaryGUIv2.Services;
using MessageBox.Avalonia.Enums;
using LegendaryMapperV2.Model;

namespace LegendaryGUIv2.ViewModels
{
    public class GameInfoViewModel : ViewModelBase
    {
        private LegendaryGame game;
        private GameViewModel gameView;
        private MainViewModel mainView;

        public GameInfoViewModel(MainViewModel mainView, GameViewModel gameView)
        {
            this.mainView = mainView;
            this.gameView = gameView;
            game = gameView.Game;
            new Thread(() => DownloadImages()).Start();

            if (!game.IsInstalled)
                game.Info(InfoCallback);
            else
                InstalledSize = game.InstallSizeReadable;
        }

        public void Back() => mainView.SetViewOnWindow(mainView);

        public void DownloadImages()
        {
            if (game.GameBannerTall != null)
            {
                Stream stream = new MemoryStream(game.GameBannerTall.GetImage());
                Cover = Avalonia.Media.Imaging.Bitmap.DecodeToHeight(stream, 500);
            }

            if (game.GameLogo != null)
            {
                Stream stream = new MemoryStream(game.GameLogo.GetImage());
                Icon = Avalonia.Media.Imaging.Bitmap.DecodeToWidth(stream, 400);
            }

            if (game.GameBanner != null)
            {
                Stream stream = new MemoryStream(game.GameBanner.GetImage());
                Background = new(stream);
            }
        }

        public void OpenLocation() => Utils.OpenFolder(game.InstallPath);
        public void OpenLocationOfFile(string file) => Utils.OpenFolderWithHighlightedFile(Path.Join(Path.GetTempPath(), "LegendaryImageCache", file));

        public void Uninstall() => Utils.CreateMessageBox("Uninstall", $"Are you sure you want to uninstall {game.AppTitle}?        ", ButtonEnum.OkCancel).Show().ContinueWith(x =>
        {
            if (x.Result == ButtonResult.Ok)
            {
                game.Uninstall();
                Back();
            }
        });

        public void Install()
        {
            gameView.Install();
            Back();
        }

        private void InfoCallback(LegendaryInfoResponse response)
        {
            DownloadSize = response.Manifest.DownloadSizeReadable;
            InstalledSize = response.Manifest.DiskSizeReadable;
        }

        public LegendaryGame Game { get => game; }
        public GameViewModel GameView { get => gameView; }

        private Avalonia.Media.Imaging.Bitmap? background;
        public Avalonia.Media.Imaging.Bitmap? Background { get => background; set => this.RaiseAndSetIfChanged(ref background, value); }
        private Avalonia.Media.Imaging.Bitmap? icon;
        public Avalonia.Media.Imaging.Bitmap? Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
        private Avalonia.Media.Imaging.Bitmap? cover;
        public Avalonia.Media.Imaging.Bitmap? Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }

        private string downloadSize = "--", installedSize = "--";
        public string DownloadSize { get => downloadSize; set => this.RaiseAndSetIfChanged(ref downloadSize, value); }
        public string InstalledSize { get => installedSize; set => this.RaiseAndSetIfChanged(ref installedSize, value); }

        public bool HasUpdate
        {
            get
            {
                if (game.IsInstalled)
                    return game.UpdateAvailable;
                return false;
            }
        }

        public string InstalledVersion
        {
            get
            {
                if (game.IsInstalled)
                    return game.InstalledVersion;
                return "";
            }
        }

        public string InstallPath
        {
            get
            {
                if (game.IsInstalled)
                    return game.InstallPath;
                return "";
            }
        }
    }
}
