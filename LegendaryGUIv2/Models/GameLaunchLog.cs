using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUIv2.Models
{
    public class GameLaunchLog
    {
        public List<string> StdOut { get; set; } = new List<string>();
        public List<string> StdErr { get; set; } = new List<string>();
        public string AppName { get; set; } = "";
        public LegendaryGame? Game { get; set; }
        public GameLaunchState State { get; set; }
        public LegendaryAuth? Auth { get; set; }

        public string MergeStdOut() => string.Join("\n", StdOut);
        public string MergeStdErr() => string.Join("\n", StdErr);

        private static GameLaunchLog? instance;
        private GameLaunchLog() { }
        public static GameLaunchLog Get()
        {
            if (instance == null)
                instance = new();

            return instance;
        }
    }
}
