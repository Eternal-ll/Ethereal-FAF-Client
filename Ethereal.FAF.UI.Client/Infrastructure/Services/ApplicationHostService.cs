using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
	/// <summary>
	/// Managed host of the application.
	/// </summary>
	internal class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly IPageService _pageService;
        private readonly IThemeService _themeService;
        private readonly ITaskBarService _taskBarService;
        private readonly INotifyIconService _notifyIconService;
        private readonly IHostApplicationLifetime ApplicationLifetime;
        private readonly IConfiguration Configuration;

        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
            IPageService pageService, IThemeService themeService,
            ITaskBarService taskBarService, INotifyIconService notifyIconService,
            IHostApplicationLifetime applicationLifetime, IConfiguration configuration)
        {
            // If you want, you can do something with these services at the beginning of loading the application.
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _pageService = pageService;
            _themeService = themeService;
            _taskBarService = taskBarService;
            _notifyIconService = notifyIconService;
            ApplicationLifetime = applicationLifetime;
            Configuration = configuration;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //using var client = new HttpClient();
            //var update = client.GetFromJsonAsync<Ethereal.FAF.Client.Updater.Update>(Configuration.GetUpdateUrl()).Result;
            //var version = Configuration.GetVersion();
            //if (version != update.Version)
            //{
            //    if (update.ForceUpdate)
            //    {
            //        Process.Start(new ProcessStartInfo()
            //        {
            //            FileName = "Ethereal.FAF.Client.Updater.exe",
            //            UseShellExecute = false,
            //        });
            //        Environment.Exit(0);
            //    }
            //}
            PrepareNavigation();
            return HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _notifyIconService.Unregister();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            await Task.CompletedTask;
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _serviceProvider.GetRequiredService<PlayersViewModel>();
                _serviceProvider.GetRequiredService<GamesViewModel>();
                _serviceProvider.GetRequiredService<ChatViewModel>();
                _navigationWindow = _serviceProvider.GetService<INavigationWindow>();

                var notifyIcon = _serviceProvider.GetService<INotifyIconService>();

                if (!notifyIcon!.IsRegistered)
                {
                    notifyIcon!.SetParentWindow(_navigationWindow as Window);
                    notifyIcon.Register();
                }

                _navigationWindow!.ShowWindow();

                var snackbarService = _serviceProvider.GetService<SnackbarService>();
                snackbarService.SetSnackbarControl(((MainWindow)_navigationWindow).RootSnackbar);

                var dialogService = _serviceProvider.GetService<DialogService>();
                dialogService.SetDialogControl(((MainWindow)_navigationWindow).RootDialog);
                // NOTICE: You can set this service directly in the window 
                _navigationWindow.SetPageService(_pageService);
                _navigationWindow.Navigate(typeof(LoaderView));

                var loaderVM = _serviceProvider.GetService<LoaderViewModel>();
                await loaderVM.TryPassChecksAndLetsSelectServer();
                _serviceProvider.GetService<ServerViewModel>();

                if (!JavaHelper.HasJavaRuntime())
                {
                    await JavaHelper.PrepareJavaRuntime();
                }
            }

            await Task.CompletedTask;
        }

        private void PrepareNavigation()
        {
            _navigationService.SetPageService(_pageService);
        }
    }
}
