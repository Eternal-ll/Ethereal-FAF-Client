using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.UI.EtherealClient.Views.Windows;
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

        private readonly IceManager IceManager;
        private readonly LobbyClient LobbyClient;

        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
            IPageService pageService, IThemeService themeService,
            ITaskBarService taskBarService, INotifyIconService notifyIconService, IceManager iceManager,
            IHostApplicationLifetime applicationLifetime, LobbyClient lobbyClient)
        {
            // If you want, you can do something with these services at the beginning of loading the application.
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _pageService = pageService;
            _themeService = themeService;
            _taskBarService = taskBarService;
            _notifyIconService = notifyIconService;
            IceManager = iceManager;
            ApplicationLifetime = applicationLifetime;
            LobbyClient = lobbyClient;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            PrepareNavigation();
            return HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            LobbyClient.DisconnectAsync(false);
            LobbyClient.Dispose();
            IceManager.IceServer?.Kill();
            _notifyIconService.Unregister();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            await Task.CompletedTask;

            //LobbyClient.ConnectAsync();
            _serviceProvider.GetService<GamesViewModel>();
            _serviceProvider.GetService<PlayersViewModel>();
            _serviceProvider.GetService<PartyViewModel>();
            _serviceProvider.GetService<GameLauncher>();
            _serviceProvider.GetService<ChatViewModel>();

            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _navigationWindow = _serviceProvider.GetService<INavigationWindow>();
                _navigationWindow!.ShowWindow();

                var snackbarService = _serviceProvider.GetService<SnackbarService>();
                snackbarService.SetSnackbarControl(((MainWindow)_navigationWindow).RootSnackbar);

                var dialogService = _serviceProvider.GetService<DialogService>();
                dialogService.SetDialogControl(((MainWindow)_navigationWindow).RootDialog);
                // NOTICE: You can set this service directly in the window 
                // _navigationWindow.SetPageService(_pageService);

                // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()
                // _navigationWindow.Navigate(typeof(Views.Pages.Dashboard));
            }

            var notifyIcon = _serviceProvider.GetService<INotifyIconService>();

            if (!notifyIcon!.IsRegistered)
            {
                notifyIcon!.SetParentWindow(_navigationWindow as Window);
                notifyIcon.Register();
            }

            await Task.CompletedTask;
        }

        private void PrepareNavigation()
        {
            _navigationService.SetPageService(_pageService);
        }
    }
}
