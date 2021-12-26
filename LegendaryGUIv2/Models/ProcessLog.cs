using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegendaryGUIv2.Models
{
    public class ProcessLog
    {
        [JsonProperty("appName")]
        public string? AppName { get; set; }
        [JsonProperty("appTitle")]
        public string? AppTitle { get; set; }
        [JsonProperty("sessions")]
        public List<ProcessSessionLog> Sessions { get; set; } = new();
    }

    public class ProcessSessionLog
    {
        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }
        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }
        [JsonProperty("timeSpent")]
        public TimeSpan TimeSpent { get; set; }
    }
}
