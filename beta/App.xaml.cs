using beta.Infrastructure.Extensions;
using beta.Models;
using beta.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using beta.Models.Debugger;

namespace beta
{
  
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        

        public static IServiceProvider Services => Hosting.Services;
        public static bool IsDesignMode { get; private set; } = true;
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

            string mapPreviews = GetPathToFolder(Folder.MapsSmallPreviews);
            if (Directory.Exists(mapPreviews))
                Directory.CreateDirectory(mapPreviews);

            string emojisCache = GetPathToFolder(Folder.Emoji);
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

            await host.StopAsync().ConfigureAwait(false);
            host.Dispose();
            _Hosting = null;
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
