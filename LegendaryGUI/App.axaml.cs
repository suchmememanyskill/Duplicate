using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegendaryGUI.ViewModels;
using LegendaryGUI.Views;
using LegendaryMapper;
using System.Diagnostics;
using System;
using ReactiveUI;
using System.Reactive;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Avalonia.Controls;

namespace LegendaryGUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Legendary legendary = null;

            try
            {
                legendary = new Legendary();
            }
            catch
            {

            }
                
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(legendary, desktop),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
