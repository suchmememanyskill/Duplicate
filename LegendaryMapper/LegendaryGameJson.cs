using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegendaryMapper
{
    public class LegendaryGameJson
    {
        public JObject JSON { get; private set; }

        public List<LegendaryImage> Images { get => GenerateImageList(); }

        private List<LegendaryImage> GenerateImageList()
        {
            JArray array = (JArray)JSON["metadata"]["keyImages"];
            return array.Select(x => new LegendaryImage((JObject)x)).ToList();
        }

        public LegendaryGameJson(LegendaryGame game)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "legendary", "metadata", $"{game.AppName}.json");
            if (File.Exists(path))
            {
                JSON = JObject.Parse(File.ReadAllText(path));
            }
        }
    }
}
