using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LobbyConnectionViewModel: Base.ViewModel
    {
        private readonly IFafLobbyService _fafLobbyService;
        private readonly ISnackbarService _snackbarService;
        private readonly INavigationWindow _navigationWindow;
        private readonly ILogger _logger;

        public LobbyConnectionViewModel(
            IFafLobbyService fafLobbyService,
            ILogger<LobbyConnectionViewModel> logger,
            ISnackbarService snackbarService,
            INavigationWindow navigationWindow)
        {
            _fafLobbyService = fafLobbyService;
            _logger = logger;
            _snackbarService = snackbarService;
            _navigationWindow = navigationWindow;
        }
        public override async Task OnLoadedAsync() => await _fafLobbyService
            .ConnectAsync()
            .ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    _logger.LogError("Failed to connect to lobby");
                    _logger.LogError(x.Exception.InnerException.ToString());
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _snackbarService.Show("Lobby", $"Failed to connect: {x.Exception.InnerException.Message}");
                        _navigationWindow.Navigate(typeof(AuthView));
                    });
                    return;
                }
                Application.Current.Dispatcher.Invoke(() => _navigationWindow.Navigate(typeof(NavigationView)));
            }, TaskScheduler.Default);
    }
}
