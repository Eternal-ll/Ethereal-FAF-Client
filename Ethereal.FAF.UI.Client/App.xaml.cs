using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using static Ethereal.FAF.API.Client.BuilderExtensions;

namespace Ethereal.FAF.UI.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost Hosting;
        protected override async void OnStartup(StartupEventArgs e)
        {
            Hosting = Host.CreateDefaultBuilder(e.Args)
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.AddFile(hostingContext.Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
            })
            .ConfigureHostOptions(c =>
            {
                //c.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
            //.ConfigureAppConfiguration(cfg => cfg
            //    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)))
            .ConfigureServices(ConfigureServices)
            .Build();
            await Hosting.StartAsync();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;
            var paths = Client.Properties.Paths.Default;
            string version = "2.0.5";

            // Background
            services.AddHostedService<TokenReloadService>();
            // App Host
            services.AddHostedService<ApplicationHostService>();
            // Theme manipulation
            services.AddSingleton<IThemeService, ThemeService>();
            // Taskbar manipulation
            services.AddSingleton<ITaskBarService, TaskBarService>();
            // Tray icon
            services.AddSingleton<INotifyIconService, CustomNotifyIconService>();
            // Page resolver service
            services.AddSingleton<IPageService, PageService>();
            // Page resolver service
            services.AddSingleton<ITestWindowService, TestWindowService>();
            // Service containing navigation, same as INavigationWindow... but without window
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<NotificationService>();
            // Main window container with navigation
            services.AddScoped<INavigationWindow, MainWindow>();
            services.AddScoped<ContainerViewModel>();

            services.AddSingleton<SnackbarService>();
            services.AddSingleton<DialogService>();

            services.AddScoped(p => new FafOAuthClient(
                clientId: configuration.GetValue<string>("FAForever:OAuth:ClientId"),
                scope: configuration.GetValue<string>("FAForever:OAuth:Scope"),
                redirectPorts: configuration.GetSection("FAForever:OAuth:RedirectPorts").Get<int[]>(),
                baseAddress: configuration.GetValue<string>("FAForever:OAuth:BaseAddress"),
                httpClientFactory: p.GetRequiredService<IHttpClientFactory>(),
                logger: p.GetRequiredService<ILogger<FafOAuthClient>>()));

            services.AddSingleton(s => new UidGenerator(
                logger: s.GetService<ILogger<UidGenerator>>(),
                uid: configuration.GetValue<string>("Paths:UidGenerator")));

            services.AddScoped(p=> new LobbyClient(
                host: configuration.GetValue<string>("FAForever:Lobby:Host"),
                port: configuration.GetValue<int>("FAForever:Lobby:Port"),
                logger: p.GetRequiredService<ILogger<LobbyClient>>(),
                uidGenerator: p.GetService<UidGenerator>(),
                userAgent: configuration.GetValue<string>("FAForever:OAuth:ClientId"),
                userAgentVersion: version));

            services.AddSingleton<TokenProvider>();

            services.AddScoped(s => new PatchClient(
                logger: s.GetService<ILogger<PatchClient>>(),
                serviceProvider: s,
                patchFolder: paths.Patch,
                tokenProvider: s.GetService<ITokenProvider>()));
            
            services.AddScoped(s => new IceManager(
                logger: s.GetService<ILogger<IceManager>>(),
                lobbyClient: s.GetService<LobbyClient>(),
                javaRuntimeFile: configuration.GetValue<string>("Paths:Java:Executable"),
                iceClientJar: configuration.GetValue<string>("Paths:IceAdapter:Executable"),
                iceClientLogging: configuration.GetValue<string>("Paths:IceAdapter:Logs"),
                notificationService: s.GetService<NotificationService>()));

            services.AddScoped<GameLauncher>();

            var vault = Environment.ExpandEnvironmentVariables(paths.Vault);
            if (FaPaths.TryGetCustomVaultPath(out var customVaultPath))
            {
                vault = customVaultPath;
            }
            FaPaths.Path = vault;

            services.AddTransient(s => new MapGenerator(
                javaRuntime: configuration.GetValue<string>("Paths:Java:Executable"),
                logging: configuration.GetValue<string>("Paths:MapGenerator:Logs"),
                previewPath: configuration.GetValue<string>("Paths:MapGenerator:PreviewPath"),
                mapGeneratorsFolder: configuration.GetValue<string>("Paths:MapGenerator:Versions"),
                generatedMapsFolder: FaPaths.Maps,
                httpClientFactory: s.GetService<IHttpClientFactory>(),
                logger: s.GetService<ILogger<MapGenerator>>()));

            services.AddScoped(s => new MapsService(
                mapsFolder: FaPaths.Maps,
                baseAddress: configuration.GetValue<string>("FAForever:Content"),
                httpClientFactory: s.GetService<IHttpClientFactory>(),
                logger: s.GetService<ILogger<MapsService>>()));

            services.AddScoped<ITokenProvider, TokenProvider>();

            services.AddFafApi();

            services.AddHttpClient();

            services.AddTransient<GamesView>();
            services.AddScoped<GamesViewModel>();
            services.AddScoped<MatchmakingViewModel>();
            services.AddScoped<PartyViewModel>();

            services.AddTransient<LinksView>();
            services.AddTransient<LinksViewModel>();

            services.AddScoped<PlayersView>();
            services.AddScoped<PlayersViewModel>();

            services.AddTransient<HostGameView>();
            services.AddTransient<HostGameViewModel>();
            services.AddTransient<GenerateMapView>();
            services.AddTransient<SelectLocalMapView>();
            services.AddTransient<SelectCoopView>();

            services.AddTransient<GenerateMapsVM>();
            services.AddTransient<LocalMapsVM>();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText("Log.log", e.Exception.ToString());
            if (Hosting is null) return;
            var logger = Hosting.Services.GetService<ILogger<App>>();
            logger.LogError(e.Exception.ToString());
            var snackbar = Hosting.Services.GetService<SnackbarService>();
            try
            {
                snackbar.Show("App exception", e.Exception.ToString(), Wpf.Ui.Common.SymbolRegular.ErrorCircle24);
            }
            catch { }
        }
    }
}
