using Avalonia.Media;
using LegendaryMapperV2.Service;
using ReactiveUI;


namespace LegendaryGUIv2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LegendaryAuth auth;

        public MainWindowViewModel()
        {
            auth = new LegendaryAuth();
            auth.AttemptLogin(OnLogin, OnLoginFailure);
            loginFailure = new(this);
            Content = new LoginView();
        }

        public void Login(string sid)
        {
            auth.GetAuth(sid).Then(x =>
            {
                auth.AttemptLogin(OnLogin, loginFailure.OnFailedSubmit);
            }).Start();
        }

        private void OnLogin() => Content = new MainViewModel(auth);

        private void OnLoginFailure() => Content = loginFailure;

        private ViewModelBase? content = null;
        private LoginFailureViewModel loginFailure;
        public ViewModelBase? Content { get => content; set => content = this.RaiseAndSetIfChanged(ref content, value); }
    }
}
