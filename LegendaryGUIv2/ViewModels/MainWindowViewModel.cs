using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LegendaryGUIv2.Models;
using LegendaryGUIv2.Services;
using LegendaryMapperV2.Service;
using ReactiveUI;
using System;

namespace LegendaryGUIv2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LegendaryAuth auth;
        public CLIState CLIState { get; private set; }

        public MainWindowViewModel(CLIState state)
        {
            CLIState = state;
            auth = new LegendaryAuth();
            try
            {
                if (CLIState == CLIState.LaunchError)
                {
                    auth = GameLaunchLog.Get().Auth!;
                    Content = new ArgLaunchViewModel();
                }
                else
                {
                    auth.AttemptLogin(OnLogin, OnLoginFailure);
                    loginFailure = new(this);
                    Content = new LoginView();
                }
            }
            catch
            {
                Content = new NotInstalledViewModel();
            }
        }

        public void Login(string sid)
        {
            auth.GetAuth(sid).Then(x =>
            {
                auth.AttemptLogin(y =>
                {
                    if (CLIState == CLIState.Passtrough)
                        OnLogin(y);
                    else
                    {
                        ProcessMonitor.SpawnApp(string.Join(" ", CLI.Args!));
                        Dispatcher.UIThread.Post(() => App.MainWindow?.Close());
                    }
                }, loginFailure!.OnFailedSubmit);
            }).Start();
        }
        public void SetViewModel(ViewModelBase view) => Content = view;

        private void OnLogin(LegendaryAuth a) => SetMainViewModel();

        public void SetMainViewModel() => Content = mainView = new MainViewModel(auth, this);

        private void OnLoginFailure(LegendaryAuth a) => Content = loginFailure;

        private ViewModelBase? content = null;
        private MainViewModel? mainView;
        private LoginFailureViewModel? loginFailure;
        public ViewModelBase? Content { get => content; set => content = this.RaiseAndSetIfChanged(ref content, value); }
    }
}
