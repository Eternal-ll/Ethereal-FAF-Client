using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models.Clans;
using Ethereal.FAF.UI.Client.Infrastructure.Background;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Mediator;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Updater;
using Ethereal.FAF.UI.Client.Models.Configurations;
using Ethereal.FAF.UI.Client.Models.Settings;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Hosting;
using Ethereal.FAF.UI.Client.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucs.JsonSettings.Autosave;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace Ethereal.FAF.UI.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public static IHost Hosting;
        //public static LaunchWindow Window;
        protected override async void OnStartup(StartupEventArgs e)
        {
#if DEBUG
#else
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
#endif
            Hosting = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                })
            .ConfigureServices(ConfigureServices)
            //.ConfigureHostConfiguration(c =>
            //{
            //    c.AddJsonFile("appsettings.user.json", true, true);
            //})
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.AddFile(hostingContext.Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
            })
            .Build();
            await Hosting.StartAsync();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            services.AddSingleton<Settings>(sp =>
            {
                var settings = new Settings()
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.user.json")
                };
                settings.Load();
                settings.EnableAutosave();
                return settings;
            });
            // App Host
            services.AddHostedService<ApplicationHostService>();


            services
                // background image cache worker
                .AddHostedService<ImageCachingBackgroundService>()
                // background image cache queue
                .AddSingleton<BackgroundImageCachingQueue>()
                // background image cache publisher
                .AddSingleton<IBackgroundImageCacheService>(sp => sp.GetService<BackgroundImageCachingQueue>());

            services
                .AddHostedService<LobbyBackgroundQueueService>()
                .AddSingleton<LobbyBackgroundQueue>();
            
            services.AddAppServices();
            services.AddFafServices();
            services.AddViewsWithViewModels();

            services.AddMemoryCache();
            // Background
            //services.AddHostedService<TokenReloadService>();
            //services.AddHostedService<ApiGameValidator>();
            services.AddTransient<WebViewWindow>();
            services.AddSingleton<NotificationService>();;
            services.AddScoped<ContainerViewModel>();

            services.AddSingleton<ClientManager>();

            services.AddScoped<ServerManager>();

            services.AddScoped<IrcClient>(s => new IrcClient(
                    host: configuration.GetValue<string>("Server:Irc:Host"),
                    port: configuration.GetValue<int>("Server:Irc:Port"),
                    s.GetService<ILogger<IrcClient>>()));

            services.AddScoped<IFafApi<ClanDto>, IFafClansService>(sp => sp.GetRequiredService<IFafClansService>());

            //services.AddTransient<IRequestHandler<GetDataCommand<ClanDto>, PaginationDto<ClanDto>>, GetDataCommandHandler<ClanDto>>();

            services.Configure<IceAdapterConfiguration>(configuration.GetSection("IceAdapter"));
            services.AddScoped<IRelationParser<ClanDto>, ClanDtoRelationParser>();

            services.AddSingleton<PatchWatcher>();
            services.AddScoped<MapGenerator>();
            services.AddScoped<MapsService>();
            //services.AddScoped<ServerViewModel>();

            services.AddSingleton<TokenProvider>();
            services.AddSingleton<ITokenProvider, TokenProvider>(s => s.GetRequiredService<TokenProvider>());

            services.AddHttpClient();

            services.AddTransient<MatchmakingViewModel>();
            services.AddTransient<PartyViewModel>();

            services.AddSingleton<BackgroundViewModel>();

            services.AddTransient<GenerateMapView>();
            services.AddTransient<SelectLocalMapView>();
            services.AddTransient<SelectCoopView>();

            services.AddTransient<GenerateMapsVM>();
            services.AddTransient<LocalMapsVM>();

            services.AddScoped<SelectGameLocationView>();
            services.AddScoped<SelectFaPatchLocationView>();
            services.AddScoped<SelectVaultLocationView>();

            services.AddTransient<ReportViewModel>();


            services.AddSingleton<IUpdateHelper, UpdateHelper>();
            services.AddHttpClient("UpdateClient");
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = e.Exception.ToString();
            e.Handled = true;
            if (Hosting is null)
            {
                return;
            }
            var logger = Hosting.Services.GetService<ILogger<App>>();
            logger.LogError(e.Exception.ToString());
            try
            {
                //var dialog = Hosting.Services.GetService<IDialogService>().GetDialogControl();
                //dialog.Hide();
                //dialog.Title = "Exception occuried";
                //dialog.ButtonLeftName = string.Empty;
                //dialog.ButtonRightName = "Close";
                //dialog.Content = null;
                //dialog.Message = e.Exception.Message;
                //dialog.Footer = null;
                //dialog.Show();
            }
            catch
            {
                //throw e.Exception;
            }
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
