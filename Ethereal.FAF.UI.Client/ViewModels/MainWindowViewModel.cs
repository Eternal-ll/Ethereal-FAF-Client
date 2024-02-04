using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels.Dialogs;
using Ethereal.FAF.UI.Client.Views;
using System.Windows;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class MainWindowViewModel : Base.ViewModel
    {
        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly IFafLobbyService _fafLobbyService;
        private INavigationWindow _navigationWindow;

        public UpdateViewModel UpdateViewModel { get; init; }

        public MainWindowViewModel(UpdateViewModel updateViewModel, IFafLobbyEventsService fafLobbyEventsService, IFafLobbyService fafLobbyService)
        {
            fafLobbyEventsService.OnConnection += FafLobbyEventsService_OnConnection;
            UpdateViewModel = updateViewModel;
            _fafLobbyEventsService = fafLobbyEventsService;
            _fafLobbyService = fafLobbyService;
        }

        private void FafLobbyEventsService_OnConnection(object sender, bool e)
        {
            if (!e)
            {
                Application.Current.Dispatcher.Invoke(() =>
                _navigationWindow.Navigate(typeof(LobbyConnectionView)));
            }
        }
        public void SetupNavigationWindow(INavigationWindow navigationWindow)
            => _navigationWindow = navigationWindow;
    }
}