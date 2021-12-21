using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegendaryGUIv2.ViewModels;
using LegendaryGUIv2.Views;
using System;
using System.Globalization;
using System.IO;

namespace LegendaryGUIv2
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Avalonia.Controls.Window MainWindow { get; private set; }
        public static IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }

        public override void OnFrameworkInitializationCompleted()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-EN", false);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(desktop.Args),
                };

                MainWindow = desktop.MainWindow;
                Desktop = desktop;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnUnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                File.WriteAllText("error.log", ex.ToString());
            }
            catch { }
        }
    }
}
