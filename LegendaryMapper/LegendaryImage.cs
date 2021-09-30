using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapper
{
    public class LegendaryImage
    {
        public JObject JSON { get; private set; }

        public LegendaryImage(JObject JSON)
        {
            this.JSON = JSON;
        }

        public int Height { get => (int)JSON["height"]; }
        public int Width { get => (int)JSON["width"]; }
        public int Size { get => (int)JSON["size"]; }
        public string Md5 { get => (string)JSON["md5"]; }
        public string Type { get => (string)JSON["type"]; }
        public string UploadDate { get => (string)JSON["uploadedDate"]; }
        public string Url { get => (string)JSON["url"]; }
        public string UrlExt { get {
                string a = Url.Split('.').Last();
                if (a.Length > 5)
                    return "jpg";

                return a;
            }}

        public void SaveUrlAs(string path)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(Url), $"{path}.{UrlExt}");
            }
        }
    }
}
