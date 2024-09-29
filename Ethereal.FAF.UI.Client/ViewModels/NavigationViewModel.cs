using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class NavigationViewModel : Base.ViewModel
    {
        private readonly IFafLobbyService _fafLobbyService;
        private readonly IGameLauncher _gameLauncher;
        private readonly IContentDialogService _contentDialogService;
        private readonly INavigationWindow _navigationWindow;
        private readonly ISnackbarService _snackbarService;
        public NavigationViewModel(IFafLobbyService fafLobbyService, IGameLauncher gameLauncher, IContentDialogService contentDialogService, INavigationWindow navigationWindow, ISnackbarService snackbarService)
        {
            LoadMenuItems();

            Infrastructure.Helper.EventManager.Instance.UpdateAvailable += (_, info) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateNavigationViewItem.Visibility = Visibility.Visible;
                    UpdateNavigationViewItem.ToolTip = $"Update avaiable! New version: {info.Channel}-v{info.Version}";
                    _snackbarService.Show("Client", "Update available!");
                });
            };
            _fafLobbyService = fafLobbyService;
            _gameLauncher = gameLauncher;
            _contentDialogService = contentDialogService;
            _navigationWindow = navigationWindow;
            _snackbarService = snackbarService;
        }
        private NavigationViewItem UpdateNavigationViewItem;

        private void LoadMenuItems()
        {

            MenuItems = new()
            {
                new NavigationViewItem("Play", SymbolRegular.XboxController24, typeof(PlayTabPage))
                {
                    MenuItems = new NavigationViewItem[]
                    {
                        new("Custom", SymbolRegular.XboxController24, typeof(GamesView))
                        {
                            NavigationCacheMode = NavigationCacheMode.Enabled
                        },
                    }
                },
                new NavigationViewItem("Players", SymbolRegular.PeopleList24, typeof(PlayersView))
                {
                    NavigationCacheMode = NavigationCacheMode.Disabled
                },
                new NavigationViewItem("Data", SymbolRegular.DataArea24, typeof(DataView))
                {

                }
            };
            UpdateNavigationViewItem = new()
            {
                Command = UpdateClientCommand,
                Foreground = Brushes.LimeGreen,
                Icon = new SymbolIcon(SymbolRegular.ArrowDownload16)
                {
                    Foreground = Brushes.LimeGreen
                },
                Visibility = Visibility.Collapsed,
            };
            FooterMenuItems = new()
            {
                UpdateNavigationViewItem
            };
        }

        [ObservableProperty]
        private ObservableCollection<object> _MenuItems;
        [ObservableProperty]
        private ObservableCollection<object> _FooterMenuItems;
        [RelayCommand]
        private async Task UpdateClient()
        {
            if (_gameLauncher.State == GameLauncherState.Running)
            {
                await _contentDialogService.ShowAlertAsync("Update", "You must close the game to update client", "Close");
                return;
            }
            if (_fafLobbyService.Connected)
            {
                await _fafLobbyService.DisconnectAsync();
            }
            _navigationWindow.Navigate(typeof(UpdateClientView));
        }
    }
}
