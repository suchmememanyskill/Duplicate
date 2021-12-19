using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        private string userPath = "", currentPath = "";
        private bool hadPathFailure = false;
        public string UserPath { get => userPath; set => this.RaiseAndSetIfChanged(ref userPath, value); }
        public string CurrentPath { get => currentPath; set => this.RaiseAndSetIfChanged(ref currentPath, value); }
        public bool HadPathFailure { get => hadPathFailure; set => this.RaiseAndSetIfChanged(ref hadPathFailure, value); }
    }
}
