using LegendaryGUIv2.Services;
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

        public void UrlOpen() => Utils.OpenUrl("https://www.epicgames.com/id/login?redirectUrl=https://www.epicgames.com/id/api/redirect");

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

        public string SidField { get; set; } = "";
        private bool loginFailure = false;
        public bool LoginFailure { get => loginFailure; set => this.RaiseAndSetIfChanged(ref loginFailure, value); }
        private bool submitted = false;
        public bool Submitted { get => submitted; set => this.RaiseAndSetIfChanged(ref submitted, value); }
    }
}
