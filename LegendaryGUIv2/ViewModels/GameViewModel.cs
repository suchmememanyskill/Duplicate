using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using LegendaryMapperV2.Service;
using ReactiveUI;
using System.Threading;

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

        public void DownloadImages()
        {
            if (game.GameBannerTall != null)
            {
                using (WebClient client = new WebClient())
                {
                    Stream stream = new MemoryStream(client.DownloadData(game.GameBannerTall.Url));
                    Cover = Avalonia.Media.Imaging.Bitmap.DecodeToHeight(stream, 300);
                }
            }

            if (game.GameLogo != null)
            {
                using (WebClient client = new WebClient())
                {
                    Stream stream = new MemoryStream(client.DownloadData(game.GameLogo.Url));
                    Icon = Avalonia.Media.Imaging.Bitmap.DecodeToWidth(stream, 200);
                }
            }
        }

        public string GameName { get => game.AppTitle; }
        public static IBrush HalfTransparency { get; } = new SolidColorBrush(Avalonia.Media.Color.FromArgb(128,0,0,0));

        private Avalonia.Media.Imaging.Bitmap cover;
        public Avalonia.Media.Imaging.Bitmap Cover { get => cover; set => this.RaiseAndSetIfChanged(ref cover, value); }
        private Avalonia.Media.Imaging.Bitmap icon;
        public Avalonia.Media.Imaging.Bitmap Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }
    }
}
