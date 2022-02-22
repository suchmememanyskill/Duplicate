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
using LegendaryGUIv2.Models;
using Newtonsoft.Json;
using Avalonia.Media;
using LegendaryMapperV2.Services;

namespace LegendaryGUIv2.ViewModels
{
    public class GameInfoViewModel : ViewModelBase
    {
        private LegendaryGame game;
        private GameViewModel gameView;
        private MainViewModel mainView;
        public ProtonManager ProtonManager { get; } = new();
        private Dictionary<string, string> availableProtonVersions = new();

        public GameInfoViewModel(MainViewModel mainView, GameViewModel gameView)
        {
            this.mainView = mainView;
            this.gameView = gameView;
            game = gameView.Game;
            alwaysOffline = game.ConfigAlwaysOffline;
            alwaysSkipUpdate = game.ConfigAlwaysSkipUpdateCheck;
            additionalArgs = game.ConfigAdditionalGameArgs;
            syncSaves = game.ConfigSyncSave;
            new Thread(() => DownloadImages()).Start();
            new Thread(() => GameSlug = game.GetProductSlug()).Start();

            if (!game.IsInstalled)
                game.Info(InfoCallback);
            else
                InstalledSize = game.InstallSizeReadable;

            string playtimePath = Path.Join(LegendaryAuth.ConfigDir, $"{game.AppName}.json");
            if (File.Exists(playtimePath))
            {
                ProcessLog? log = JsonConvert.DeserializeObject<ProcessLog>(File.ReadAllText(playtimePath));
                TimeSpan total = TimeSpan.FromSeconds(log!.Sessions.Sum(x => x.TimeSpent.TotalSeconds));
                Playtime = $"{total:hh\\h\\ mm\\m}";
            }

            if (ProtonManager.CanUseProton)
            {
                availableProtonVersions = ProtonManager.GetProtonPaths();
                ProtonItems = availableProtonVersions.Keys.ToList();
                useWithProton = game.ConfigUseProton;
                protonConfigIndex = ProtonItems.FindIndex(x => x == game.ConfigProtonVersion);
                if (protonConfigIndex < 0)
                {
                    protonConfigIndex = 0;
                    game.ConfigProtonVersion = protonItems.First();
                }
                    
            }
        }

        public void Back()
        {
            if (configChanged)
                game.Parser.SaveConfig();
            mainView.SetViewOnWindow(mainView);
        }

        public void DownloadImages()
        {
            if (game.GameBannerTall != null)
            {
                Stream stream = new MemoryStream(game.GameBannerTall.GetImage());
                Cover = Avalonia.Media.Imaging.Bitmap.DecodeToHeight(stream, 450, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            }

            if (game.GameLogo != null)
            {
                Stream stream = new MemoryStream(game.GameLogo.GetImage());
                Icon = Avalonia.Media.Imaging.Bitmap.DecodeToWidth(stream, 360);
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

        public void OpenEpicGames() => Utils.OpenUrl($"https://www.epicgames.com/store/en-US/p/{GameSlug}");

        private void InfoCallback(LegendaryInfoResponse response)
        {
            DownloadSize = response.Manifest.DownloadSizeReadable;
            InstalledSize = response.Manifest.DiskSizeReadable;
        }

        public void Play()
        {
            if (configChanged)
                game.Parser.SaveConfig();

            GameView.Play();
        }

        public void SyncNow() => mainView.ConsoleView.ExecuteCommand($"Manually syncing {game.AppName}", game.ForceSyncSave(), this);

        public LegendaryGame Game { get => game; }
        public GameViewModel GameView { get => gameView; }
        private Avalonia.Media.Imaging.Bitmap? background;
        public Avalonia.Media.Imaging.Bitmap? Background { get => background; set => this.RaiseAndSetIfChanged(ref background, value); }
        private Avalonia.Media.Imaging.Bitmap? icon;
        public Avalonia.Media.Imaging.Bitmap? Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
        private Avalonia.Media.Imaging.Bitmap? cover;
        public Avalonia.Media.Imaging.Bitmap? Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }
        private bool configChanged = false, alwaysOffline, alwaysSkipUpdate, syncSaves;
        private string additionalArgs = "";
        public bool AlwaysOffline { get => alwaysOffline; set { this.RaiseAndSetIfChanged(ref alwaysOffline, value); game.ConfigAlwaysOffline = value; configChanged = true; } }
        public bool AlwaysSkipUpdate { get => alwaysSkipUpdate; set { this.RaiseAndSetIfChanged(ref alwaysSkipUpdate, value); game.ConfigAlwaysSkipUpdateCheck = value; configChanged = true; } }
        public string AdditionalArgs { get => additionalArgs; set { this.RaiseAndSetIfChanged(ref additionalArgs, value); game.ConfigAdditionalGameArgs = value; configChanged = true; } }
        public bool SyncSaves { get => syncSaves; set { this.RaiseAndSetIfChanged(ref syncSaves, value); game.ConfigSyncSave = value; configChanged = true; } }

        private string downloadSize = "--", installedSize = "--", gameSlug = "";
        public string DownloadSize { get => downloadSize; set => this.RaiseAndSetIfChanged(ref downloadSize, value); }
        public string InstalledSize { get => installedSize; set => this.RaiseAndSetIfChanged(ref installedSize, value); }
        public string GameSlug { get => gameSlug; set => this.RaiseAndSetIfChanged(ref gameSlug, value); }
        public string Playtime { get; } = "";
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

        private List<string> protonItems = new();
        public List<string> ProtonItems { get => protonItems; set => this.RaiseAndSetIfChanged(ref protonItems, value); }
        private int protonConfigIndex = -1;
        public int ProtonConfigIndex { get => protonConfigIndex; set 
            { 
                this.RaiseAndSetIfChanged(ref protonConfigIndex, value);
                game.ConfigProtonVersion = protonItems[value];
                configChanged = true;
            } 
        }
        private bool useWithProton = false;
        public bool UseWithProton { get => useWithProton; set { this.RaiseAndSetIfChanged(ref useWithProton, value); game.ConfigUseProton = value; configChanged = true; } }
    }
}
