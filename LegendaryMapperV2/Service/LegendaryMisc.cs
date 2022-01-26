using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Service
{
    public class LegendaryMisc
    {
        private LegendaryAuth auth;

        public LegendaryMisc(LegendaryAuth auth)
        {
            this.auth = auth;
        }

        public LegendaryCommand EosOverlayInstall(string path = "")
        {
            if (auth.OfflineLogin)
                throw new Exception("Can't download overlay while offline");

            string args = "";
            if (path != "")
                args += $"--path \"{Path.Join(path, "Overlay")}\"";

            return new LegendaryCommand($"eos-overlay {args} install -y");
        }

        public LegendaryCommand EosOverlayRemove() => new LegendaryCommand($"eos-overlay remove -y");
        public LegendaryCommand EosOverlayInfo() => new LegendaryCommand("eos-overlay info");

        public LegendaryCommand EglSyncOneShot() => new LegendaryCommand("egl-sync --one-shot -y");
        public LegendaryCommand EglSyncMigrate() => new LegendaryCommand("egl-sync --migrate -y");
    }
}
