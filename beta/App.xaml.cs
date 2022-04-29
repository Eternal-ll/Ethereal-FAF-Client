using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Debugger;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace beta
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static IServiceProvider Services => Hosting.Services;
        public static bool IsDesignMode { get; private set; } = false;
        private static IHost _Hosting;
        public static IHost Hosting => _Hosting ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

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

            IsDesignMode = false;

            //new Window()
            //{
            //    Content = new ProfileViewModel(new()
            //    {
            //        id = 165,
            //        ratings = new()
            //        {
            //            { "global", new() { rating = new double[] { 10, 1 } } },
            //            { "ladder_1v1", new() { rating = new double[] { 10, 1 } } },
            //            { "tmm_2v2", new() { rating = new double[] { 10, 1 } } },
            //            { "tmm_4v4_share_until_death", new() { rating = new double[] { 10, 1 } } },
            //        }
            //    })
            //}.Show();

            string mapPreviews = GetPathToFolder(Folder.MapsSmallPreviews);
            if (Directory.Exists(mapPreviews))
                Directory.CreateDirectory(mapPreviews);

            string emojisCache = GetPathToFolder(Folder.Emoji);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Common);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            //emojisCache = GetPathToFolder(Folder.Maps);
            //if (Directory.Exists(emojisCache))
            //    Directory.CreateDirectory(emojisCache);
            //emojisCache = GetPathToFolder(Folder.Mods);
            //if (Directory.Exists(emojisCache))
            //    Directory.CreateDirectory(emojisCache);

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
        public static string CurrentDirectory => IsDesignMode 
            ? Path.GetDirectoryName(GetSourceCodePath())
            : Environment.CurrentDirectory;

        private static string GetSourceCodePath([CallerFilePath] string Path = null) => Path;
    }
}
