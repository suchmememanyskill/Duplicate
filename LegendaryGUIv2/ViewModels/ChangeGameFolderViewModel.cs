using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace LegendaryGUIv2.ViewModels
{
    public class ChangeGameFolderViewModel : ViewModelBase
    {
        private MainViewModel mainView;
        public ChangeGameFolderViewModel(MainViewModel mainView)
        {
            this.mainView = mainView;
            currentPath = mainView.DownloadLocation;
            UserPath = mainView.Manager.GameDirectory;
        }

        public void OnBack() => mainView.SetViewOnWindow(mainView);
        public void OnSubmit()
        {
            try
            {
                mainView.Manager.GameDirectory = UserPath;
            }
            catch
            {
                HadPathFailure = true;
                return;
            }

            mainView.SetDownloadLocationText();
            OnBack();
        }

        public async Task OnBrowse()
        {
            OpenFolderDialog dialog = new();
            string result = await dialog.ShowAsync(App.mainWindow);
            if (result != null && result != "")
                UserPath = result;
        }

        private string userPath = "", currentPath = "";
        private bool hadPathFailure = false;
        public string UserPath { get => userPath; set => this.RaiseAndSetIfChanged(ref userPath, value); }
        public string CurrentPath { get => currentPath; set => this.RaiseAndSetIfChanged(ref currentPath, value); }
        public bool HadPathFailure { get => hadPathFailure; set => this.RaiseAndSetIfChanged(ref hadPathFailure, value); }
    }
}
