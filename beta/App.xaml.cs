using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Debugger;
using beta.Models.Enums;
using beta.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace beta
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static IServiceProvider Services => Hosting.Services;
        
        private static IHost _Hosting;
        public static IHost Hosting => _Hosting ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static void Restart()
        {
            System.Diagnostics.Process.Start(ResourceAssembly.Location[..^3] + "exe");
            Current.Shutdown();
        }

        public static NewWindow Window;

        public static string GetPathToFolder(Folder folder)
        {
            return folder switch
            {
                Folder.Game => beta.Properties.Settings.Default.PathToGame,

                // Mods & Maps
                Folder.Maps => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMaps),
                Folder.Mods => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMods),

                // CACHE
                Folder.MapsSmallPreviews => CurrentDirectory + "\\cache\\previews\\small\\",
                Folder.MapsLargePreviews => CurrentDirectory + "\\cache\\previews\\large\\",
                Folder.PlayerAvatars => CurrentDirectory + "\\cache\\players\\avatars\\",

                Folder.Emoji => CurrentDirectory + "\\Resources\\Images\\Emoji",
                
                // PATCH
                Folder.ProgramData => "C:\\ProgramData\\FAForever\\",

                Folder.Common => CurrentDirectory + "\\cache\\common\\",

                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppDebugger.Init();
            Window = new NewWindow();
            Window.Show();

            string mapPreviews = GetPathToFolder(Folder.MapsSmallPreviews);
            if (Directory.Exists(mapPreviews))
                Directory.CreateDirectory(mapPreviews);
            string emojisCache = GetPathToFolder(Folder.Emoji);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Common);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Maps);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Mods);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);

            var host = Hosting;

            base.OnStartup(e);
            await host.StartAsync().ConfigureAwait(false);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            var host = Hosting;

            var gameSession = Services.GetService<IGameSessionService>();
            gameSession.Close();

            await host.StopAsync().ConfigureAwait(false);
            host.Dispose();
            _Hosting = null;

            Environment.Exit(-1);
        }

        public static void ConfigureServices(HostBuilderContext host, IServiceCollection services) => services
            .RegisterServices();

        // TODO: maybe will be useful for future
        public static string CurrentDirectory => Environment.CurrentDirectory;

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var win = new Window()
            {
                Content = new TextBox()
                {
                    Text = e.Exception.ToString(),
                    IsReadOnly = true,
                    Background = new SolidColorBrush(Colors.Transparent)
                },
                Background = new SolidColorBrush(Colors.DarkGray)
                {
                    Opacity = .1
                },
                Resources = App.Current.Resources
            };
            //win.Closed += Win_Closed;
            win.Show();
            // Prevent default unhandled exception processing
            //Task.Run(() =>
            //{
            //    Thread.Sleep(10000);
            //    Environment.Exit(-1);
            //});
            e.Handled = true;
        }
    }
}
