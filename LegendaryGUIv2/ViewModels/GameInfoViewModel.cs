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

namespace LegendaryGUIv2.ViewModels
{
    public class GameInfoViewModel : ViewModelBase
    {
        private LegendaryGame game;
        private MainViewModel mainView;

        public GameInfoViewModel(MainViewModel mainView, LegendaryGame game)
        {
            this.mainView = mainView;
            this.game = game;
            new Thread(() => DownloadImages()).Start();
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

        public void OpenLocation()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", "\"" + game.InstallPath.Replace("/", "\\") + "\"");
            }
            // TODO: add linux
        }

        public void Uninstall() => Utils.CreateMessageBox("Uninstall", $"Are you sure you want to uninstall {game.AppTitle}?        ", ButtonEnum.OkCancel).Show().ContinueWith(x =>
        {
            if (x.Result == ButtonResult.Ok)
            {
                game.Uninstall();
                Back();
            }
        });

        public LegendaryGame Game { get => game; }

        private Avalonia.Media.Imaging.Bitmap? background;
        public Avalonia.Media.Imaging.Bitmap? Background { get => background; set => this.RaiseAndSetIfChanged(ref background, value); }
        private Avalonia.Media.Imaging.Bitmap? icon;
        public Avalonia.Media.Imaging.Bitmap? Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
        private Avalonia.Media.Imaging.Bitmap? cover;
        public Avalonia.Media.Imaging.Bitmap? Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }
    }
}
