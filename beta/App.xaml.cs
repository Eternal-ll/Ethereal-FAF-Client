using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services;
using beta.Models.Enums;
using beta.ViewModels;
using beta.Views;
using beta.Views.Windows;
using Ethereal.FAF.LobbyServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using System.Windows.Interop;
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
        public static IHost Hosting => _Hosting ??= Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
            .ConfigureLogging(logging =>
            {
            })
            .ConfigureAppConfiguration(cfg => cfg
                  .SetBasePath(Environment.CurrentDirectory)
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
            .ConfigureServices(ConfigureServices)
            .Build();

        public static string GetPathToFolder(Folder folder)
        {
            return folder switch
            {
                Folder.Game => beta.Properties.Settings.Default.PathToGame,

                // Mods & Maps
                Folder.Maps => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMaps),
                Folder.Mods => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMods),

                // CACHE
                Folder.MapsSmallPreviews => Environment.CurrentDirectory + "\\cache\\previews\\small\\",
                Folder.MapsLargePreviews => Environment.CurrentDirectory + "\\cache\\previews\\large\\",
                Folder.PlayerAvatars => Environment.CurrentDirectory + "\\cache\\players\\avatars\\",

                Folder.Emoji => Environment.CurrentDirectory + "\\Resources\\Images\\Emoji",
                
                // PATCH
                Folder.ProgramData => "C:\\ProgramData\\FAForever\\",

                Folder.Common => Environment.CurrentDirectory + "\\cache\\common\\",

                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var host = Hosting;
            base.OnStartup(e);
            await host.StartAsync();
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var host = Hosting;
            await host.StopAsync();
            host.Dispose();
            _Hosting = null;
            Environment.Exit(-1);
        }

        public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services
           .AddHostedService<ApplicationHostService>()
           .RegisterServices()
           .AddSingleton<NavigationService>()
           .RegisterViewModel() 
           .AddScoped<MainWindow>()
           .RegisterViews()
           .AddLobbyServer();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
    }
}
