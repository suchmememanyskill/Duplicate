using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using LegendaryGUIv2.Services;
using LegendaryMapperV2.Service;
using System;
using System.Diagnostics;
using System.Threading;

namespace LegendaryGUIv2
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => new CLI(args).Handle();
        public static void OnLogin(LegendaryAuth auth, string[] args)
        {
            LegendaryGameManager manager = new(auth);
            manager.GetGames();
            LegendaryGame? game = manager.InstalledGames.Find(x => x.AppName == args[1]);
            if (game != null)
            {
                ProcessMonitor ps = new(game);
                ps.Monitor();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
