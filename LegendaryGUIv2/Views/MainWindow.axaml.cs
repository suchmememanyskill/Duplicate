using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LegendaryGUIv2.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;

namespace LegendaryGUIv2.Views
{
    public partial class MainWindow : Window
    {
        public static List<GameViewModel>? gameViewModels;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
            Closing += Deinit;
#endif
        }

        public void Deinit(object? sender, CancelEventArgs? e)
        {
            gameViewModels?.ForEach(x =>
            {
                x.Icon?.Dispose();
                x.Icon = null;
                x.Cover?.Dispose();
                x.Cover = null;
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
