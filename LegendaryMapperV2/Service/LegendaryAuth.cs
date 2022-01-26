using LegendaryMapperV2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryMapperV2.Service
{
    public class LegendaryAuth
    {
        public static bool HasNetwork()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public LegendaryStatusResponse StatusResponse { get; private set; }
        public bool OfflineLogin { get; private set; }
        public LegendaryCommand GetStatus(bool offline = false)
        {
            if (!HasNetwork())
                offline = true;

            string addons = "";
            OfflineLogin = offline;
            if (offline)
                addons += "--offline";

            return new LegendaryCommand($"status --json {addons}").Then(ParseResponse);
        }

        private void ParseResponse(LegendaryCommand command) => StatusResponse = JsonConvert.DeserializeObject<LegendaryStatusResponse>(command.Terminal.StdOut[0]);

        public LegendaryCommand GetAuth(string sid)
        {
            return new LegendaryCommand($"auth --sid {sid}");
        }

        public delegate void LegendaryAuthCallback(LegendaryAuth auth);

        public void AttemptLogin(LegendaryAuthCallback onLogin = null, LegendaryAuthCallback onFailure = null)
        {
            if (File.Exists(Path.Join(ConfigDir, "config.json")))
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Join(ConfigDir, "config.json")));

            GetStatus().Then(x =>
            {
                if (StatusResponse.IsLoggedIn())
                    onLogin?.Invoke(this);
                else
                    onFailure?.Invoke(this);
            }).OnError(x =>
            {
                GetStatus(true).Then(y =>
                {
                    if (StatusResponse.IsLoggedIn())
                        onLogin?.Invoke(this);
                    else
                        onFailure?.Invoke(this);
                }).OnError(y =>
                {
                    onFailure?.Invoke(this);
                }).Start();
            }).Start();
        }
        public Config Config { get; private set; } = new();
        public static string ConfigDir
        {
            get
            {
                string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Duplicate");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }
        public void SaveConfig() => File.WriteAllText(Path.Join(LegendaryAuth.ConfigDir, "config.json"), JsonConvert.SerializeObject(Config));
    }
}
