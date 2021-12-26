using LegendaryMapperV2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
