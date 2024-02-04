using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using System;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LobbyConnectionViewModel: Base.ViewModel
    {
        private readonly IFafLobbyService _fafLobbyService;
        private readonly IFafAuthService _fafAuthService;
        private readonly ISnackbarService _snackbarService;
        private readonly INavigationWindow _navigationWindow;
        private readonly ILogger _logger;

        public LobbyConnectionViewModel(
            IFafLobbyService fafLobbyService,
            ILogger<LobbyConnectionViewModel> logger,
            ISnackbarService snackbarService,
            INavigationWindow navigationWindow,
            IFafAuthService fafAuthService)
        {
            _fafLobbyService = fafLobbyService;
            _logger = logger;
            _snackbarService = snackbarService;
            _navigationWindow = navigationWindow;
            _fafAuthService = fafAuthService;
        }
        public override async Task OnLoadedAsync() => await Task.Run(ConnectAsync);
        private async Task ConnectAsync()
        {
            if (_fafLobbyService.Connected)
            {
                _logger.LogWarning("Lobby is already connected");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _navigationWindow.Navigate(typeof(NavigationView));
                });
                return;
            }
            var delays = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 50, null, true);
            foreach (var delay in delays)
            {
                var unauthorized = false;
                await _fafLobbyService.ConnectAsync()
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                        {
                            _logger.LogError("Failed to connect to lobby");
                            _logger.LogError(x.Exception?.InnerException?.ToString());
                            if(x.Exception.InnerException is Refit.ApiException api)
                            {
                                unauthorized = true;
                            }
                        }
                    }, TaskScheduler.Default);
                if (unauthorized)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _snackbarService.Show("Lobby", $"Unauthorized");
                        _navigationWindow.Navigate(typeof(AuthView));
                    });
                    return;
                }
                if (_fafLobbyService.Connected)
                {
                    break;
                }
                await Task.Delay(delay);
            }
            if (_fafLobbyService.Connected)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _snackbarService.Show("Lobby", $"Connected to lobby");
                    _navigationWindow.Navigate(typeof(NavigationView));
                });
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                _snackbarService.Show("Lobby", $"Failed to connect to lobby");
                _navigationWindow.Navigate(typeof(AuthView));
            });
        }
    }
}
