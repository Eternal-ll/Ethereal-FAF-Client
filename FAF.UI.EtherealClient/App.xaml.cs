using FAF.UI.EtherealClient.Infrastructure.Services;
using FAF.UI.EtherealClient.Infrastructure.Services.Interfaces;
using FAF.UI.EtherealClient.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace FAF.UI.EtherealClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost Hosting;
        protected override async void OnStartup(StartupEventArgs e)
        {
            Hosting = Host.CreateDefaultBuilder(e.Args)
                .ConfigureLogging(logging =>
                {
                })
                .ConfigureAppConfiguration(cfg => cfg
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location))
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .ConfigureServices(ConfigureServices)
                .Build();
            await Hosting.StartAsync();
        }
        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // App Host
            services.AddHostedService<ApplicationHostService>();

            // Theme manipulation
            services.AddSingleton<IThemeService, ThemeService>();

            // Taskbar manipulation
            services.AddSingleton<ITaskBarService, TaskBarService>();

            // Tray icon
            services.AddSingleton<INotifyIconService, NotifyIconService>();

            // Page resolver service
            services.AddSingleton<IPageService, PageService>();

            // Page resolver service
            services.AddSingleton<ITestWindowService, TestWindowService>();

            // Service containing navigation, same as INavigationWindow... but without window
            services.AddSingleton<INavigationService, NavigationService>();

            // Main window container with navigation
            services.AddScoped<INavigationWindow, Views.Windows.MainnWindow>();
            services.AddScoped<ContainerViewModel>();
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
    }
}
