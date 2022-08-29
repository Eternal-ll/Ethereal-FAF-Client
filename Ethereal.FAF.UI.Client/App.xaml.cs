using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using System.Configuration;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using System;
using Ethereal.FAF.API.Client;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e) => await Host.CreateDefaultBuilder(e.Args)
            .ConfigureLogging(c =>
            {

            })
            .ConfigureHostOptions(c =>
            {
                c.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
            .ConfigureAppConfiguration(cfg => cfg
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
            .ConfigureServices(ConfigureServices)
            .Build()
            .StartAsync();

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;
            // App Host
            services.AddHostedService<ApplicationHostService>();
            services.AddHostedService<TokenReloadService>();
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
            // Main window container with navigation
            services.AddScoped<INavigationWindow, MainWindow>();
            services.AddScoped<ContainerViewModel>();

            services.AddScoped(p => new FafOAuthClient(
                clientId: configuration.GetValue<string>("FAForever:OAuth:ClientId"),
                scope: configuration.GetValue<string>("FAForever:OAuth:Scope"),
                redirectPorts: configuration.GetSection("FAForever:OAuth:RedirectPorts").Get<int[]>(),
                baseAddress: configuration.GetValue<string>("FAForever:OAuth:BaseAddress"),
                httpClientFactory: p.GetRequiredService<IHttpClientFactory>(),
                logger: p.GetRequiredService<ILogger<FafOAuthClient>>()));

            services.AddScoped(p=> new LobbyClient(
                host: configuration.GetValue<string>("FAForever:Lobby:Host"),
                port: configuration.GetValue<int>("FAForever:Lobby:Port"),
                logger: p.GetRequiredService<ILogger<LobbyClient>>()));

            services.AddSingleton<TokenProvider>();

            services.AddTransient<GamesView>();
            services.AddScoped<GamesViewModel>();

            services.AddScoped<PatchClient>();
            services.AddScoped<IceManager>();
            services.AddScoped<GameLauncher>();

            services.AddScoped<SnackbarService>();
            services.AddScoped<DialogService>();

            services.AddFafApi();

            services.AddHttpClient();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            new Window()
            {
                Content = new TextBox()
                {
                    Text = e.Exception.Message + '\n' + e.Exception.StackTrace
                }
            }.Show();
        }
    }
}
