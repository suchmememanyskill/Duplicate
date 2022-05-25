using LegendaryMapperV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LegendaryMapperV2.Services;

namespace LegendaryMapperV2.Service
{
    public class LegendaryGame
    {
        public InstalledGame InstalledData { get; set; } = null;
        public GameMetadata Metadata { get; private set; }
        public LegendaryGameManager Parser { get; private set; }
        public List<LegendaryGame> Dlc { get; private set; } = new();
        public bool IsDlc { get; set; }
        public bool ConfigAlwaysOffline { get => GetConfigItem().AlwaysOffline; set { ConfigItem item = GetConfigItem(); item.AlwaysOffline = value; SetConfigItem(item); } }
        public bool ConfigAlwaysSkipUpdateCheck { get => GetConfigItem().AlwaysSkipUpdate; set { ConfigItem item = GetConfigItem(); item.AlwaysSkipUpdate = value; SetConfigItem(item); } }
        public string ConfigAdditionalGameArgs { get => GetConfigItem().AdditionalArgs; set { ConfigItem item = GetConfigItem(); item.AdditionalArgs = value; SetConfigItem(item); } }
        public bool ConfigSyncSave { get => GetConfigItem().SyncSave; set { ConfigItem item = GetConfigItem(); item.SyncSave = value; SetConfigItem(item); } }
        public bool ConfigUseProton { get => GetConfigItem().UseProton; set { ConfigItem item = GetConfigItem(); item.UseProton = value; SetConfigItem(item); } }
        public string ConfigProtonVersion { get => GetConfigItem().ProtonVersion; set { ConfigItem item = GetConfigItem(); item.ProtonVersion = value; SetConfigItem(item); } }
        public bool ConfigSeperateProtonPath { get => GetConfigItem().SeperateProtonPath; set { ConfigItem item = GetConfigItem(); item.SeperateProtonPath = value; SetConfigItem(item); } }

        public string AppName { get => Metadata.AppName; }
        public string AppTitle { get => Metadata.AppTitle; }
        public string InstalledVersion { get => InstalledData.Version; }
        public string AvailableVersion { get {
                if (Metadata.AssetInfo != null)
                    return Metadata.AssetInfo.BuildVersion;
                if (Metadata.AssetInfos != null)
                    return Metadata.AssetInfos["Windows"].BuildVersion;

                return null;
            }
        }
        public bool UpdateAvailable { get { if (AvailableVersion != null) return InstalledVersion != AvailableVersion; else return false; } }
        public long InstallSizeBytes { get => InstalledData.InstallSize; }
        private readonly string[] gameSizes = { "B", "KB", "MB", "GB" };
        public string InstallSizeReadable { get {
                int type = 0;
                double bytesLeft = InstallSizeBytes;
                while (bytesLeft >= 1024)
                {
                    type++;
                    bytesLeft /= 1024;
                }

                return $"{bytesLeft:0.00} {gameSizes[type]}";
            }
        }

        public string Developer { get { if (Metadata != null && Metadata.Metadata != null && Metadata.Metadata.Developer != null) return Metadata.Metadata.Developer; else return ""; } }
        
        public string InstallPath { get => InstalledData.InstallPath; }
        public bool IsInstalled { get => InstalledData != null; }
        public bool HasCloudSave { get { if (Metadata != null && Metadata.Metadata != null && Metadata.Metadata.CustomAttributes != null) return Metadata.Metadata.CustomAttributes.ContainsKey("CloudSaveFolder"); return false; } }

        public MetaImage GameBanner { get => GetGameImage("DieselGameBox"); }
        public MetaImage GameBannerTall { get => GetGameImage("DieselGameBoxTall"); }
        public MetaImage GameLogo { get => GetGameImage("DieselGameBoxLogo"); }

        public Image<Rgba32> GetGameBannerTallWithLogo()
        {
            byte[] rawBanner = GameBannerTall.GetImage();

            if (rawBanner == null)
                return null;
            
            Image<Rgba32> banner = Image.Load<Rgba32>(rawBanner);
            if (GameLogo == null)
                return banner;

            byte[] rawLogo = GameLogo.GetImage();

            if (rawLogo == null)
                return null;

            Image<Rgba32> logo = Image.Load<Rgba32>(rawLogo);
            Image<Rgba32> output = new Image<Rgba32>(banner.Width, banner.Height);

             // Steam's horizontal height is about 1.5x the vertical height
             float newWidth = banner.Height / 1.5f;
             float newHeight = (newWidth / logo.Width) * logo.Height;
             logo.Mutate(x => x.Resize(new Size((int)newWidth, (int)newHeight)));

            float centerX = banner.Width / 2;
            float centerY = banner.Height / 2;
            float logoPosX = centerX - logo.Width / 2;
            float logoPosY = centerY - logo.Height / 2;
            output.Mutate(x => x
                .DrawImage(banner, new Point(0, 0), 1f)
                .DrawImage(logo, new Point((int)logoPosX, (int)logoPosY), 1f)
            );

            banner.Dispose();
            logo.Dispose();
            return output;
        }

        public LegendaryGame(GameMetadata meta, LegendaryGameManager parser)
        {
            Metadata = meta;
            Parser = parser;
        }

        public void SetInstalledData(InstalledGame installed) => InstalledData = installed;

        public void Launch(bool offline = false, bool skipUpdate = false) => LaunchCommand(offline, skipUpdate).Start();
        public LegendaryCommand LaunchCommand(bool offline = false, bool skipUpdate = false)
        {
            if (InstalledData == null)
                throw new Exception("Game is not installed");

            List<string> args = new();

            if (Parser.Auth.OfflineLogin || ConfigAlwaysOffline)
                offline = true;

            if (offline)
                args.Add("--offline");

            if (!offline && (skipUpdate || ConfigAlwaysSkipUpdateCheck ))
                args.Add("--skip-version-check");

            ProtonManager protonManager = new();

            if (protonManager.CanUseProton && !ConfigUseProton)
            {
                try
                {
                    Process.Start("wine", "--version"); // Checks if wine is not installed
                }
                catch
                {
                    // Wine seems to not be installed. Switching over to proton instead
                    ConfigUseProton = true;
                    ConfigProtonVersion = protonManager.GetProtonPaths().First().Key;
                }
            }

            if (protonManager.CanUseProton && ConfigUseProton)
            {
                Dictionary<string, string> protonVersion = protonManager.GetProtonPaths();
                if (!protonVersion.ContainsKey(ConfigProtonVersion))
                    throw new Exception("Invalid proton configuration");

                args.Add($"--wrapper \"{protonVersion[ConfigProtonVersion].Replace(" ", "\\ ")}/proton run\"");
                args.Add("--no-wine");
            }

            LegendaryCommand cmd = new LegendaryCommand($"launch {InstalledData.AppName} {string.Join(" ", args)} {ConfigAdditionalGameArgs}");

            if (protonManager.CanUseProton && ConfigUseProton)
            {
                string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string protonPrefix = Path.Join(homeFolder, ".proton_duplicate", "default");

                if (ConfigSeperateProtonPath)
                    protonPrefix = Path.Join(homeFolder, ".proton_duplicate", AppName);

                if (!Directory.Exists(protonPrefix))
                    Directory.CreateDirectory(protonPrefix);

                // Move the old proton prefix folder
                string oldProtonPrefix = Path.Join(homeFolder, ".steam/steam/steamapps/compatdata");

                new List<string>()
                {
                    "config_info",
                    "pfx.lock",
                    "tracked_files",
                    "version",
                    "pfx"
                }.ForEach(x =>
                {
                    string path = Path.Join(oldProtonPrefix, x);

                    if (File.Exists(path))
                        File.Move(path, Path.Join(protonPrefix, x));
                    else if (Directory.Exists(path))
                        Directory.Move(path, Path.Join(protonPrefix, x));
                });

                cmd.Env("STEAM_COMPAT_DATA_PATH", protonPrefix);
                cmd.Env("STEAM_COMPAT_CLIENT_INSTALL_PATH", Path.Join(homeFolder, ".steam/steam"));
            }

            return cmd;
        }

        public LegendaryDownload InstantiateDownload() => new LegendaryDownload(this);

        public void Uninstall()
        {
            if (InstalledData == null)
                throw new Exception("Game is not installed");

            new LegendaryCommand($"uninstall {InstalledData.AppName} -y").Then(x => Parser.GetGames()).Start();
        }

        public delegate void InfoCallback(LegendaryInfoResponse response);
        public void Info(InfoCallback callback)
        {
            if (Parser.Auth.OfflineLogin)
                throw new Exception("Info cannot be gathered while offline!");

            new LegendaryCommand($"info {AppName} --json").Then(x =>
            {
                LegendaryInfoResponse info = JsonConvert.DeserializeObject<LegendaryInfoResponse>(x.Terminal.StdOut.First());
                callback.Invoke(info);
            }).Start();
        }

        public string GetProductSlug()
        {
            if (Metadata == null || Metadata.Metadata == null || Metadata.Metadata.Namespace == null)
                return "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpContent content = new StringContent("{\"query\":\"{Catalog{catalogOffers( namespace:\\\"" +
                                                            Metadata.Metadata.Namespace +
                                                            "\\\"){elements {productSlug}}}}\"}", Encoding.Default, "application/json");
                    HttpResponseMessage response = client.PostAsync(new Uri("https://www.epicgames.com/graphql"), content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    string textResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    
                    EpicProductSlugResponse parsedResponse = JsonConvert.DeserializeObject<EpicProductSlugResponse>(textResponse);
                    Element slug = parsedResponse?.Data?.Catalog?.CatalogOffers?.Elements?.FirstOrDefault(x => x?.ProductSlug != null);
                    if (slug == null)
                        return "";
                    return slug.ProductSlug.Split("/").First();
                    
                }
            }
            catch
            {
                return "";
            }
        }

        // I don't see a nice way to do this, and i do not understand linux shenans enough to figure this out
        // Will silently fail if offline
        public LegendaryCommand ForceSyncSave()
        {
            if (Parser.Auth.OfflineLogin)
                return null;

            LegendaryCommand cmd = new($"sync-saves {AppName}");
            cmd.Terminal.Yes = true;
            return cmd;
        }

        public Process GetGameProcess()
        {
            string processName;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) // Now, you may think this is a mistake. It's not. Os' were a mistake
                processName = InstalledData.Executable.Replace(".exe", ""); // Needs .exe stripped to work
            else
                processName = InstalledData.Executable; // Needs .exe intact to work

            Process[] possibleProcesses = Process.GetProcessesByName(processName);
            Console.WriteLine($"Found {possibleProcesses.Length} processes with name {processName}\n\n{String.Join("\n", possibleProcesses.ToList().Select(x => x.ProcessName))}");
            return possibleProcesses.FirstOrDefault();
        }
        private MetaImage GetGameImage(string type)
        {
            if (Metadata == null || Metadata.Metadata == null || Metadata.Metadata.KeyImages == null)
                return null;

            if (!Metadata.Metadata.KeyImages.Any(x => x.Type == type))
                return null;
            else
                return Metadata.Metadata.KeyImages.Find(x => x.Type == type);
        }

        private ConfigItem GetConfigItem()
        {
            if (Parser.Config.GameConfigs.TryGetValue(AppName, out ConfigItem item))
                return item;

            return new();
        }

        private void SetConfigItem(ConfigItem item)
        {
            if (Parser.Config.GameConfigs.ContainsKey(AppName))
                Parser.Config.GameConfigs[AppName] = item;
            else
                Parser.Config.GameConfigs.Add(AppName, item);
        }
    }
}
