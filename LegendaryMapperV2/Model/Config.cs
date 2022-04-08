using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Model
{
    public class Config
    {
        [JsonProperty("GameConfigs")]
        public Dictionary<string, ConfigItem> GameConfigs { get; set; } = new();
    }

    public class ConfigItem
    {
        [JsonProperty("AlwaysOffline")]
        public bool AlwaysOffline { get; set; } = false;
        [JsonProperty("AlwaysSkipUpdate")]
        public bool AlwaysSkipUpdate { get; set; } = false;
        [JsonProperty("AdditionalArgs")]
        public string AdditionalArgs { get; set; } = "";
        [JsonProperty("SyncSave")]
        public bool SyncSave { get; set; } = false;

        [JsonProperty("UseProton")]
        public bool UseProton { get; set; } = false;

        [JsonProperty("ProtonVersion")]
        public string ProtonVersion { get; set; } = "-";

        [JsonProperty("SeperateProtonPath")]
        public bool SeperateProtonPath { get; set; } = false;
    }
}
