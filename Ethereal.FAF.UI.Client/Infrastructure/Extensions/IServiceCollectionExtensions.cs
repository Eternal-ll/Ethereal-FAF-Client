using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Api;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Dialogs;
using Ethereal.FAF.UI.Client.Views;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using Refit;
using System;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configure HTTP client by selected server
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureClient"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static IHttpClientBuilder ConfigureHttpClientBySelectedServer(this IHttpClientBuilder builder,
            Action<IServiceProvider, Server, System.Net.Http.HttpClient> configureClient)
        {
            if (configureClient == null)
            {
                throw new ArgumentNullException(nameof(configureClient));
            }
            return builder.ConfigureHttpClient((sp, x) =>
            {
                var server = sp.GetRequiredService<ClientManager>().GetServer() ?? throw new InvalidOperationException("Server not selected");
                configureClient(sp, server, x);
            });
        }
    }
    internal static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services) => services
            // Theme manipulation
            .AddSingleton<IThemeService, ThemeService>()
            // Taskbar manipulation
            .AddSingleton<ITaskBarService, TaskBarService>()
            // Tray icon
            //.AddSingleton<INotifyIconService, CustomNotifyIconService>()
            // Page resolver service
            .AddSingleton<IPageService, PageService>()
            // Dialog service
            .AddSingleton<IContentDialogService, ContentDialogService>()
            // Snackbar service
            .AddSingleton<ISnackbarService, SnackbarService>()
            // Page resolver service
            .AddSingleton<IWindowService, WindowService>()
            // Navigation service
            .AddSingleton<INavigationService, NavigationService>()
            // Main window container with root navigation
            .AddScoped<INavigationWindow, MainWindow>()
            
            
            .AddSingleton<ISettingsManager, SettingsManager>()
            .AddSingleton<IDownloadService, DownloadService>()

            .AddSingleton<IImageCacheService, FileImageCacheService>()

            .AddExternalServices();

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddSingleton<IGithubService, GithubService>();
            services.AddSingleton<IGitHubClient>(sp => new GitHubClient(new ProductHeaderValue("ethereal-faf-client", VersionHelper.GetCurrentVersionInText())));


            services
                .AddHttpClient("ClientConfig")
                .ConfigureHttpClient((sp, x) => x.BaseAddress = sp.GetService<IConfiguration>().GetValue<Uri>("Client:ConfigurationUrl"));
            return services;
        }

        public static IServiceCollection AddFafServices(this IServiceCollection services)
        {
            services
                .AddSingleton<IFafAuthService, FafAuthService>()
                .AddSingleton<StreamJsonRpcFafLobbyService>()
                .AddSingleton<FafLobbyService>()
                .AddSingleton<IFafLobbyService>(sp => sp.GetService<StreamJsonRpcFafLobbyService>())
                .AddSingleton<IFafLobbyEventsService>(sp => sp.GetService<StreamJsonRpcFafLobbyService>())
                .AddSingleton<IFafLobbyActionClient>(sp => sp.GetService<StreamJsonRpcFafLobbyService>())
                .AddSingleton<IFafGamesService, FafGamesService>()
                .AddSingleton<IFafGamesEventsService>(sp => (FafGamesService)sp.GetService<IFafGamesService>())
                .AddSingleton<IFafPlayersService, FafPlayersService>()
                .AddSingleton<IFafPlayersEventsService>(sp => (FafPlayersService)sp.GetService<IFafPlayersService>())
                .AddSingleton<IUserService, FafUserService>()

                .AddSingleton<IJavaRuntime, LocalJavaRuntime>()
                .AddSingleton<FafJavaIceAdapter>()
                .AddSingleton<IGameNetworkAdapter>(sp => sp.GetService<FafJavaIceAdapter>())
                .AddSingleton<IFafJavaIceAdapterClient>(sp => sp.GetService<FafJavaIceAdapter>())
                .AddSingleton<IFafJavaIceAdapterCallbacks>(sp => sp.GetService<StreamJsonRpcFafLobbyService>())

                .AddSingleton<INeroxisMapGenerator, MapGenerator>()

                .AddSingleton<IMapsService, FafMapsService>()
                .AddSingleton<IGameLauncher, FafGameLauncher>()
                .AddSingleton<IUIDService, UidGenerator>()
                
                .AddTransient<IPatchClient, PatchClient>();

            services
                .AddSingleton<StreamJsonRpcFafLobbyService>()
                .AddSingleton<IFafLobbyCallbacks>(sp => sp.GetService<StreamJsonRpcFafLobbyService>());

            services.AddSingleton<GameManager>(sp =>
            {
                var manager = (GameManager)System.Windows.Application.Current.FindResource("GameManager");
                if (!manager.Initialized)
                    manager.Initialize(
                        gameLauncher: sp.GetService<IGameLauncher>(),
                        contentDialogService: sp.GetService<IContentDialogService>());
                return manager;
            });

            services.AddSingleton<LobbyNotificationsService>();
            //services
                //.AddSingleton<GameMapPreviewCacheService>();

            services.AddTransient<WebSocketTransportClient>(sp =>
            {
                var server = sp.GetService<ClientManager>().GetServer() ?? throw new InvalidOperationException("Server not selected");
                return new(
                    url: server.Lobby.Url,
                    fafUserApi: sp.GetService<IFafUserApi>(),
                    logger: sp.GetService<ILogger<Websocket.Client.WebsocketClient>>());
            });

            services.AddTransient<WsTransportClient>(sp =>
            {
                var server = sp.GetService<ClientManager>().GetServer() ?? throw new InvalidOperationException("Server not selected");
                return new(sp.GetService<IFafUserApi>());
            });

            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<VerifyHeaderHandler>();

            // FAF OAuth2 service
            services
                .AddRefitClient<IAuthApi>()
                .ConfigureHttpClientBySelectedServer((sp, server, x) => x.BaseAddress = server.OAuth.BaseAddress);
            // FAF User service
            services
                .AddRefitClient<IFafUserApi>()
                .ConfigureHttpClientBySelectedServer((sp, server, x) => x.BaseAddress = server.UserApi)
                .AddHttpMessageHandler<AuthHeaderHandler>();
            // FAF clans service
            services.AddRefitClient<IFafClansService>()
                .ConfigureHttpClientBySelectedServer((sp, server, x) => x.BaseAddress = server.Api)
                .AddHttpMessageHandler<AuthHeaderHandler>();
            // FAF api service
            services.AddRefitClient<IFafApiClient>()
                .ConfigureHttpClientBySelectedServer((sp, server, x) =>
                x.BaseAddress = server.Api)
                .AddHttpMessageHandler<AuthHeaderHandler>();
            // FAF content service
            services.AddRefitClient<IFafContentClient>()
                .ConfigureHttpClientBySelectedServer((sp, server, x) => x.BaseAddress = server.Content)
                .AddHttpMessageHandler<AuthHeaderHandler>()
                .AddHttpMessageHandler<VerifyHeaderHandler>();

            services
                .AddHttpClient("FafContent")
                .ConfigureHttpClientBySelectedServer((sp, server, x) => x.BaseAddress = server.Content)
                .AddHttpMessageHandler<AuthHeaderHandler>()
                .AddHttpMessageHandler<VerifyHeaderHandler>();

            services.AddByAttributes();

            return services;
        }

        public static IServiceCollection AddByAttributes(this IServiceCollection services)
        {
            var exportedTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("Ethereal") == true)
                .SelectMany(a => a.GetExportedTypes())
                .ToArray();

            var transientTypes = exportedTypes
                .Select(t => new { t, attributes = t.GetCustomAttributes(typeof(TransientAttribute), false) })
                .Where(t1 =>  t1.attributes is { Length: > 0 } && !t1.t.Name.Contains("Mock", StringComparison.OrdinalIgnoreCase))
                .Select(t1 => new { Type = t1.t, Attribute = (TransientAttribute)t1.attributes[0] });

            foreach (var typePair in transientTypes)
            {
                if (typePair.Attribute.InterfaceType is null)
                {
                    services.AddTransient(typePair.Type);
                }
                else
                {
                    services.AddTransient(typePair.Attribute.InterfaceType, typePair.Type);
                }
            }

            var singletonTypes = exportedTypes
                .Select(t => new { t, attributes = t.GetCustomAttributes(typeof(SingletonAttribute), false) })
                .Where(
                    t1 =>
                        t1.attributes is { Length: > 0 }
                        && !t1.t.Name.Contains("Mock", StringComparison.OrdinalIgnoreCase)
                )
                .Select(
                    t1 => new { Type = t1.t, Attributes = t1.attributes.Cast<SingletonAttribute>().ToArray() }
                );

            foreach (var typePair in singletonTypes)
            {
                foreach (var attribute in typePair.Attributes)
                {
                    if (attribute.InterfaceType is null)
                    {
                        services.AddSingleton(typePair.Type);
                    }
                    else if (attribute.ImplType is not null)
                    {
                        services.AddSingleton(attribute.InterfaceType, attribute.ImplType);
                    }
                    else
                    {
                        services.AddSingleton(attribute.InterfaceType, typePair.Type);
                    }
                }
            }
            return services;
        }

        public static IServiceCollection AddViewsWithViewModels(this IServiceCollection services) => services
            .AddTransient<LobbyConnectionView>()
            .AddTransient<LobbyConnectionViewModel>()

            .AddTransient<SelectServerView>()
            .AddTransient<SelectServerViewModel>()

            //.AddTransient<SettingsView>()
            //.AddTransient<SettingsViewModel>()

            //.AddTransient<LinksView>()
            //.AddTransient<LinksViewModel>()

            .AddTransient<PlayersView>()
            .AddTransient<PlayersViewModel>()

            //.AddScoped<ChangelogView>()
            //.AddScoped<ChangelogViewModel>()

            //.AddScoped<ChatViewModel>()
            //.AddScoped<ChatView>()

            //.AddScoped<MapsViewModel>()
            //.AddScoped<MapsView>()

            //.AddScoped<ModsViewModel>()
            //.AddScoped<ModsView>()

            .AddScoped<NavigationViewModel>()
            .AddScoped<NavigationView>()

            //.AddScoped<LoaderView>()
            //.AddScoped<LoaderViewModel>()

            //.AddScoped<ProfileView>()
            //.AddTransient<ProfileViewModel>()

            //.AddScoped<DataView>()
            //.AddScoped<DataViewModel>()

            //.AddTransient<ClansView>()
            //.AddTransient<ClansViewModel>()

            //.AddTransient<ReportsView>()
            //.AddTransient<ReportsViewModel>()

            //.AddTransient<DownloadsView>()
            //.AddTransient<DownloadsViewModel>()

            //.AddTransient<HostGameView>()
            //.AddTransient<HostGameViewModel>()

            .AddSingleton<GamesView>()
            //.AddScoped<GamesViewModel>()

            .AddTransient<AuthView>()
            .AddTransient<AuthViewModel>()

            .AddSingleton<UpdateViewModel>()
            .AddTransient<UpdateClientView>()

            .AddSingleton<MainWindowViewModel>()

            .AddSingleton<CustomGamesViewModel>()
            .AddSingleton<LobbyGamesViewModel>()

            .AddTransient<PlayTabPage>()
            .AddTransient<PlayTabViewModel>()

            .AddTransient<PrepareClientView>()
            .AddTransient<PrepareClientViewModel>();
    }
}
