using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using LegendaryMapperV2.Service;

namespace LegendaryGUIv2.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private LegendaryGame game;
        public GameViewModel(LegendaryGame game)
        {
            this.game = game;
        }
        
        public void Select()
        {
            Debug.WriteLine($"Selected {GameName}");
        }

        public void Unselect()
        {
            Debug.WriteLine($"Unselected {GameName}");
        }

        public string GameName { get => game.AppTitle; }
        public static IBrush HalfTransparency { get; } = new SolidColorBrush(Color.FromArgb(128,0,0,0));
    }
}
