using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Servers;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public static IHost Hosting;
        protected override async void OnStartup(StartupEventArgs e)
        {
            Hosting = Host.CreateDefaultBuilder(e.Args)
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.AddFile(hostingContext.Configuration.GetSection("Logging"));
                //loggingBuilder.AddConsole();
            })
            .ConfigureServices(ConfigureServices)
            .ConfigureHostConfiguration(c =>
            {
                c.AddJsonFile("appsettings.user.json", true, true);
            })
            .Build();
            await Hosting.StartAsync();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;
            // Background
            //services.AddHostedService<TokenReloadService>();
            //services.AddHostedService<ApiGameValidator>();
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

            services.AddTransient<ServerOauthTokenProvider>();
            services.AddScoped<ServersManagement>();
            services.AddTransient<ServerManager>();

            services.AddSingleton(s => new UidGenerator(
                logger: s.GetService<ILogger<UidGenerator>>(),
                uid: configuration.GetValue<string>("Paths:UidGenerator")));
            services.AddTransient<PatchClient>();
            services.AddTransient<IceManager>();
            services.AddTransient<FafOAuthClient>();
            services.AddScoped<PatchWatcher>();
            services.AddScoped<GameLauncher>();
            services.AddScoped<MapGenerator>();
            services.AddScoped<MapsService>();

            services.AddSingleton<ITokenProvider, TokenProvider>();

            services.AddHttpClient();

            services.AddTransient<GamesView>();
            services.AddScoped<GamesViewModel>();
            services.AddScoped<MatchmakingViewModel>();
            services.AddScoped<PartyViewModel>();

            services.AddSingleton<BackgroundViewModel>();

            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();

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

            services.AddScoped<ChangelogViewModel>();
            services.AddScoped<ChangelogView>();

            services.AddScoped<ChatViewModel>();
            services.AddScoped<ChatView>();

            services.AddScoped<MapsViewModel>();
            services.AddScoped<MapsView>();

            services.AddScoped<ModsViewModel>();
            services.AddScoped<ModsView>();

            services.AddTransient<SelectServerVM>();
            services.AddTransient<SelectServerView>();

            services.AddScoped<NavigationViewModel>();
            services.AddScoped<NavigationView>();

            services.AddScoped<LoaderViewModel>();
            services.AddScoped<LoaderView>();

            services.AddScoped<ProfileView>();

            services.AddScoped<DataView>();
            services.AddScoped<DataViewModel>();

            services.AddScoped<SelectGameLocationView>();
            services.AddScoped<SelectFaPatchLocationView>();
            services.AddScoped<SelectVaultLocationView>();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Hosting is null)
            {
                File.WriteAllText($"APPLICATION_UNHANDLED_EXCEPTION.log", e.Exception.ToString());
                return;
            }
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
