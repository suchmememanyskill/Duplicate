using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryGUIv2.ViewModels
{
    public class LoginFailureViewModel : ViewModelBase
    {
        private MainWindowViewModel mainWindow;

        public LoginFailureViewModel(MainWindowViewModel mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        private void OpenUrl(string url)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else throw new Exception("No url 4 u");
        }

        public void UrlOpen() => OpenUrl("https://www.epicgames.com/id/login?redirectUrl=https://www.epicgames.com/id/api/redirect");

        public void OnSubmit()
        {
            if (string.IsNullOrWhiteSpace(SidField))
                return;

            mainWindow.Login(SidField);
            Submitted = true;
        }

        public void OnFailedSubmit()
        {
            LoginFailure = true;
            Submitted = false;
        }

        public string SidField { get; set; }
        private bool loginFailure = false;
        public bool LoginFailure { get => loginFailure; set => this.RaiseAndSetIfChanged(ref loginFailure, value); }
        private bool submitted = false;
        public bool Submitted { get => submitted; set => this.RaiseAndSetIfChanged(ref submitted, value); }
    }
}
