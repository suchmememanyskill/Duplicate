using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LegendaryGUI.Views
{
    public partial class GameItem : UserControl
    {
        public GameItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
