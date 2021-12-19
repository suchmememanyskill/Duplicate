using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegendaryGUIv2.ViewModels;
using LegendaryGUIv2.Views;
using System.Globalization;

namespace LegendaryGUIv2
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-EN", false);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(desktop.Args),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
