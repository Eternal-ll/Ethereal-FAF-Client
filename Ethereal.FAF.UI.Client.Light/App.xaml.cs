using Ethereal.FAF.UI.Client.Light.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Light.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Light.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Light.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Light.ViewModels;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.Light
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
            .ConfigureAppConfiguration(cfg => cfg
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
            .ConfigureServices(ConfigureServices)
            .Build()
            .StartAsync();

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
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
            // Main window container with navigation
            services.AddScoped<INavigationWindow, MainWindow>();
            services.AddScoped<ContainerViewModel>();
            services.AddScoped<FafOAuthClient>();
            services.AddHttpClient();

            services.AddSingleton<LobbyClient>();
        }
    }
}
