using LegendaryMapperV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Service
{
    public class LegendaryGame
    {
        public InstalledGame InstalledData { get; set; } = null;
        public GameMetadata Metadata { get; private set; }
        public LegendaryGameManager Parser { get; private set; }
        public List<LegendaryGame> Dlc { get; private set; } = new();
        public bool IsDlc { get; set; }

        public string AppName { get => Metadata.AppName; }
        public string AppTitle { get => Metadata.AppTitle; }
        public string InstalledVersion { get => InstalledData.Version; }
        public string AvailableVersion { get => Metadata.AssetInfos["Windows"].BuildVersion; }
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

        public void Launch(bool offline=false, bool skipUpdate=false)
        {
            if (InstalledData == null)
                throw new Exception("Game is not installed");

            string args = "";
            if (offline)
                args += "--offline";

            if (!offline && skipUpdate)
                args += "--skip-version-check";

            new LegendaryCommand($"launch {InstalledData.AppName} {args}").Start();
        }

        public LegendaryDownload InstantiateDownload() => new LegendaryDownload(this);

        public void Uninstall()
        {
            if (InstalledData == null)
                throw new Exception("Game is not installed");

            new LegendaryCommand($"uninstall {InstalledData.AppName} -y").Then(x => Parser.GetGames()).Start();
        }

        private MetaImage GetGameImage(string type)
        {
            if (!Metadata.Metadata.KeyImages.Any(x => x.Type == type))
                return null;
            else
                return Metadata.Metadata.KeyImages.Find(x => x.Type == type);
        }
    }
}
