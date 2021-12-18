﻿using System;
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

namespace LegendaryGUIv2.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private LegendaryGame game;
        public GameViewModel(LegendaryGame game)
        {
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

        public void DownloadImages()
        {
            if (game.GameBannerTall != null)
            {
                using (WebClient client = new WebClient())
                {
                    Stream stream = new MemoryStream(client.DownloadData(game.GameBannerTall.Url));
                    Cover = Avalonia.Media.Imaging.Bitmap.DecodeToHeight(stream, 300);
                }
            }

            if (game.GameLogo != null)
            {
                using (WebClient client = new WebClient())
                {
                    Stream stream = new MemoryStream(client.DownloadData(game.GameLogo.Url));
                    Icon = Avalonia.Media.Imaging.Bitmap.DecodeToWidth(stream, 200);
                }
            }
        }

        public void Play() => game.Launch();
        public void Info() => CreateMessageBox("Game information", $"Game: {game.AppTitle}\nGame ID: {game.AppName}\nInstalled version: {game.InstalledVersion}\nAvalilable version: {game.AvailableVersion}\nInstalled path: {game.InstallPath}").Show();
        public void Uninstall() => CreateMessageBox("Uninstall", $"Are you sure you want to uninstall {game.AppTitle}?        ", ButtonEnum.OkCancel).Show().ContinueWith(x =>
        {
            if (x.Result == ButtonResult.Ok)
                game.Uninstall();
        });

        private IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message, ButtonEnum buttons = ButtonEnum.Ok) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title,
                ContentMessage = message,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
            });

        public string GameName { get => game.AppTitle; }
        public static IBrush HalfTransparency { get; } = new SolidColorBrush(Avalonia.Media.Color.FromArgb(128,0,0,0));
        public static IBrush ThreeFourthsTransparency { get; } = new SolidColorBrush(Avalonia.Media.Color.FromArgb(196, 0, 0, 0));

        private Avalonia.Media.Imaging.Bitmap cover;
        public Avalonia.Media.Imaging.Bitmap Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }
        private Avalonia.Media.Imaging.Bitmap icon;
        public Avalonia.Media.Imaging.Bitmap Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
        private bool selected = false, installed = false, notInstalled = false, downloading = false, updateAvailable = false;
        public bool Selected { get => selected; set => this.RaiseAndSetIfChanged(ref selected, value); }
        public bool Installed { get => installed; set => this.RaiseAndSetIfChanged(ref installed, value); }
        public bool NotInstalled { get => notInstalled; set => this.RaiseAndSetIfChanged(ref notInstalled, value); }
        public bool Downloading { get => downloading; set => this.RaiseAndSetIfChanged(ref downloading, value); }
        public bool UpdateAvailable { get => updateAvailable; set => this.RaiseAndSetIfChanged(ref updateAvailable, value); }
        public string GameSize { get; }
    }
}
