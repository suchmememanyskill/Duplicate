using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace LegendaryGUIv2.Views
{
    public partial class ConsoleView : UserControl
    {
        public TextBox CLI { get; set; }

        public ConsoleView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            CLI = this.FindControl<TextBox>("CLI");
        }

        public void SetCaretIndex() => Dispatcher.UIThread.Post(() => CLI.CaretIndex = int.MaxValue);
    }
}
