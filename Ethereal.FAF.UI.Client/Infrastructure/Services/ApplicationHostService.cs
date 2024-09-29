using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
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
using Wpf.Ui;

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
        private readonly ITaskBarService _taskBarService;
        //private readonly INotifyIconService _notifyIconService;
        private readonly IHostApplicationLifetime ApplicationLifetime;
        private readonly IConfiguration Configuration;
        private readonly IWindowService _windowService;
        private readonly ISettingsManager _settingsManager;

        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
            IPageService pageService, ITaskBarService taskBarService,
            IHostApplicationLifetime applicationLifetime, IConfiguration configuration, IWindowService windowService, ISettingsManager settingsManager,
            LobbyNotificationsService lobbyNotificationsService)
        {
            // If you want, you can do something with these services at the beginning of loading the application.
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _pageService = pageService;
            _taskBarService = taskBarService;
            ApplicationLifetime = applicationLifetime;
            Configuration = configuration;
            _windowService = windowService;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _serviceProvider.GetService<IGameNetworkAdapter>().Stop();
            //_notifyIconService.Unregister();
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
                _serviceProvider.GetRequiredService<IFafPlayersService>();
                _serviceProvider.GetRequiredService<IFafGamesService>();
                _serviceProvider.GetRequiredService<LobbyGamesViewModel>();
                //_serviceProvider.GetRequiredService<ChatViewModel>();
                _navigationWindow = _serviceProvider.GetService<INavigationWindow>();

                _serviceProvider.GetService<PartyViewModel>();
                
                //_serviceProvider.GetRequiredService<GameMapPreviewCacheService>();

                //var notifyIcon = _serviceProvider.GetService<INotifyIconService>();

                //if (!notifyIcon!.IsRegistered)
                //{
                //    notifyIcon!.SetParentWindow(_navigationWindow as Window);
                //    notifyIcon.Register();
                //}
                await _settingsManager.LoadAsync();
                _navigationWindow.ShowWindow();

                if (!_settingsManager.Settings.ClientInitialized)
                {
                    _navigationWindow.Navigate(typeof(PrepareClientView));
                }
                else if (_settingsManager.Settings.SelectedFaServer == null)
                {
                    _navigationWindow.Navigate(typeof(SelectServerView));
                }
                else
                {
                    var auth = _serviceProvider.GetService<IFafAuthService>();
                    if (auth.IsAuthorized() && false)
                    {
                        _navigationWindow.Navigate(typeof(LobbyConnectionView));
                    }
                    else
                    {
                        _navigationWindow.Navigate(typeof(AuthView));
                    }
                }
                //_navigationWindow.Navigate(typeof(NavigationView));
                //_navigationWindow.Navigate(typeof(PrepareClientView));
            }

            await Task.CompletedTask;
        }
    }
}
