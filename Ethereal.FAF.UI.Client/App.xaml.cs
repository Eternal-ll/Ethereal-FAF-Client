using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models.Clans;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Mediator;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.Models.Configurations;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.UI.EtherealClient.Views.Windows;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            services.AddScoped<Server>(s => configuration.GetSection("Server").Get<Server>());

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

            services.AddSingleton<IDialogService, DialogService>(sp => sp.GetService<DialogService>());
            services.AddSingleton<ISnackbarService, SnackbarService>(sp => sp.GetService<SnackbarService>());

            services.AddScoped<ServerManager>();

            services.AddSingleton<UidGenerator>();
            services.AddScoped<PatchClient>();
            services.AddScoped<IceManager>();
            services.AddScoped<FafOAuthClient>(s =>
            {
                var server = s.GetService<Server>();
                return new FafOAuthClient(server.OAuth.ClientId, server.OAuth.Scope, server.OAuth.RedirectPorts, server.OAuth.BaseAddress,
                    s.GetService<IHttpClientFactory>(), s.GetService<ILogger<FafOAuthClient>>());
            });

            services.AddScoped<LobbyClient>(s =>
            {
                var logger = s.GetService<ILogger<LobbyClient>>();
                var ipv4 = DetermineIPAddress(configuration.GetValue<string>("Server:Lobby:Host"), logger);
                return new LobbyClient(ipv4, configuration.GetValue<int>("Server:Lobby:Port"), logger);
            });

            services.AddScoped<IrcClient>(s => new IrcClient(
                    host: configuration.GetValue<string>("Server:Irc:Host"),
                    port: configuration.GetValue<int>("Server:Irc:Port"),
                    s.GetService<ILogger<IrcClient>>()));

            services.AddScoped<IFafApi<ClanDto>, IFafClansService>(sp =>
            sp.GetRequiredService<IFafClansService>());

            services.AddRefitClient<IFafUserService>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.GetValue<Uri>("Server:API"))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.AddRefitClient<IFafClansService>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.GetValue<Uri>("Server:API"))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.AddRefitClient<IFafApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.GetValue<Uri>("Server:API"))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            services.AddRefitClient<IFafContentClient>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.GetValue<Uri>("Server:Content"))
                .AddHttpMessageHandler<AuthHeaderHandler>()
                .AddHttpMessageHandler<VerifyHeaderHandler>();

            services.AddTransient<IRequestHandler<GetDataCommand<ClanDto>, PaginationDto<ClanDto>>, GetDataCommandHandler<ClanDto>>();

            services.Configure<IceAdapterConfiguration>(configuration.GetSection("IceAdapter"));
            services.AddScoped<IRelationParser<ClanDto>, ClanDtoRelationParser>();
            services.AddScoped<IFileCacheService, FileCacheService>();

            services.AddScoped<PatchWatcher>();
            services.AddScoped<GameLauncher>();
            services.AddScoped<MapGenerator>();
            services.AddScoped<MapsService>();
            services.AddScoped<ServerViewModel>();

            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<VerifyHeaderHandler>();

            services.AddSingleton<TokenProvider>();
            services.AddSingleton<ITokenProvider, TokenProvider>(s => s.GetService<TokenProvider>());

            services.AddHttpClient();

            services.AddTransient<GamesView>();
            services.AddScoped<GamesViewModel>();
            services.AddTransient<MatchmakingViewModel>();
            services.AddTransient<PartyViewModel>();

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

            services.AddScoped<NavigationViewModel>();
            services.AddScoped<NavigationView>();

            services.AddScoped<LoaderViewModel>();
            services.AddScoped<LoaderView>();
            
            services.AddTransient<ProfileViewModel>();
            services.AddScoped<ProfileView>();

            services.AddScoped<DataView>();
            services.AddScoped<DataViewModel>();

            services.AddScoped<SelectGameLocationView>();
            services.AddScoped<SelectFaPatchLocationView>();
            services.AddScoped<SelectVaultLocationView>();

            services.AddTransient<ReportViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<ReportsView>();

            services.AddTransient<DownloadsView>();
            services.AddTransient<DownloadsViewModel>();

            services.AddTransient<AuthView>().AddTransient<AuthViewModel>();

            services
                .AddTransient<ClansView>()
                .AddTransient<ClansViewModel>();

            services
                .AddTransient<ChangeEmailView>()
                .AddTransient<ChangeEmailViewModel>();

            services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(App).Assembly));
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


        private IPAddress DetermineIPAddress(string host, ILogger logger)
        {
            logger.LogInformation("Determining IPv4 using [{host}]", host);
            if (IPAddress.TryParse(host, out var address))
            {
                logger.LogInformation("IP address [{host}] recognized", host);
                return address;
            }
            logger.LogInformation("Searching for IP address using Dns host [{host}]", host);
            var addresses = Dns.GetHostEntry(host).AddressList;
            logger.LogInformation("For [{host}] founded next addresses: [{address}]", host, string.Join(',', addresses.Select(a => a.ToString())));
            address = Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (address is null)
            {
                logger.LogError("IPv4 address not found");
                throw new ArgumentOutOfRangeException("host", host, "Can`t determine IP4v address for this host");
            }
            logger.LogInformation("Found IPv4 address [{ipv4}]", address.ToString());
            return address;
        }
    }
}
