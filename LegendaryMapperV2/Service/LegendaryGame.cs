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
        public bool UpdateAvailable { get => InstalledVersion != AvailableVersion; }
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

        public string Developer { get => Metadata.Metadata.Developer; }
        
        public string InstallPath { get => InstalledData.InstallPath; }
        public bool IsInstalled { get => InstalledData != null; }

        public MetaImage GameBanner { get => GetGameImage("DieselGameBox"); }
        public MetaImage GameBannerTall { get => GetGameImage("DieselGameBoxTall"); }
        public MetaImage GameLogo { get => GetGameImage("DieselGameBoxLogo"); }

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

            if (Parser.Auth.OfflineLogin || ConfigAlwaysOffline)
                offline = true;

            string args = "";
            if (offline)
                args += "--offline";

            if (!offline && (skipUpdate || ConfigAlwaysSkipUpdateCheck ))
                args += "--skip-version-check";

            return new LegendaryCommand($"launch {InstalledData.AppName} {args}");
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
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    string request = "{\"query\":\"{Catalog{catalogOffers( namespace:\\\"" + Metadata.Metadata.Namespace + "\\\"){elements {productSlug}}}}\"}";
                    Debug.WriteLine(request);
                    string response = client.UploadString("https://www.epicgames.com/graphql", request);
                    EpicProductSlugResponse parsedResponse = JsonConvert.DeserializeObject<EpicProductSlugResponse>(response);
                    Element slug = parsedResponse.Data.Catalog.CatalogOffers.Elements.FirstOrDefault(x => x.ProductSlug != null);
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
