using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LegendaryMapperV2.Service;
using ReactiveUI;


namespace LegendaryGUIv2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LegendaryAuth auth;
        private string[] args;

        public MainWindowViewModel(string[] args)
        {
            this.args = args;
            auth = new LegendaryAuth();
            try
            {
                auth.AttemptLogin(OnLogin, OnLoginFailure);
                loginFailure = new(this);
                Content = new LoginView();
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
                auth.AttemptLogin(OnLogin, loginFailure!.OnFailedSubmit);
            }).Start();
        }
        public void SetViewModel(ViewModelBase view) => Content = view;

        private void OnLogin(LegendaryAuth a)
        {
            if (args.Length < 1)
                SetMainViewModel();
            else
                Content = new ArgLaunchViewModel(auth, args);
        }

        public void SetMainViewModel() => Content = mainView = new MainViewModel(auth, this);

        private void OnLoginFailure(LegendaryAuth a) => Content = loginFailure;

        private ViewModelBase? content = null;
        private MainViewModel? mainView;
        private LoginFailureViewModel? loginFailure;
        public ViewModelBase? Content { get => content; set => content = this.RaiseAndSetIfChanged(ref content, value); }
    }
}
