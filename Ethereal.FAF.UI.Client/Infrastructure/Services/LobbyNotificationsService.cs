using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class LobbyNotificationsService
    {
        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly ISnackbarService _snackbarService;

        public LobbyNotificationsService(IFafLobbyEventsService fafLobbyEventsService, ISnackbarService snackbarService)
        {
            fafLobbyEventsService.NotificationReceived += FafLobbyEventsService_NotificationReceived;
            _fafLobbyEventsService = fafLobbyEventsService;
            _snackbarService = snackbarService;
        }

        private async void FafLobbyEventsService_NotificationReceived(object sender, global::FAF.Domain.LobbyServer.Notification e)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            _snackbarService.Show("FAF lobby", e.Text));
        }
    }
}
