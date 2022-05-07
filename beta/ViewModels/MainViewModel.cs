using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace beta.ViewModels
{
    /// <summary>
    /// Main view model that controls overall work and stability of application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private readonly IOAuthService OAuthService;
        private readonly ISessionService SessionService;
        private readonly INotificationService NotificationService;

        public MainViewModel()
        {
            OAuthService = App.Services.GetService<IOAuthService>();
            SessionService = App.Services.GetService<ISessionService>();
            NotificationService = App.Services.GetService<INotificationService>();

            SessionService.Authorized += OnSessionAuthorizationCompleted;
            SessionService.NotificationReceived += SessionService_NotificationReceived;

            ChildViewModel = new AuthorizationViewModel();
            //ChildViewModel = new NavigationViewModel();

            //Task.Run(() =>
            //{
            //    //App.Current.Dispatcher.Invoke(() => Services.GetService<INotificationService>().ShowDialog(new ConnectionViewModel()));
            //    Thread.Sleep(000);
            //    App.Current.Dispatcher.Invoke(() => NotificationService.ShowConnectionDialog(new ConnectionViewModel()));
            //});
        }

        private async void SessionService_StateChanged(object sender, SessionState e)
        {
            if (e == SessionState.Disconnected)
            {
                SessionService.StateChanged -= SessionService_StateChanged;
                await App.Current.Dispatcher.Invoke(() => NotificationService.ShowConnectionDialog(new ConnectionViewModel()));
            }
        }

        private void SessionService_NotificationReceived(object sender, Models.Server.NotificationData e)
        {
            if (e.text.Contains("unofficial", StringComparison.OrdinalIgnoreCase)) return;
            NotificationService.ShowPopupAsync(e.text);
        }

        private void OnSessionAuthorizationCompleted(object sender, bool e)
        {
            if (e)
            {
                App.Current.Dispatcher.Invoke(() => ChildViewModel = new NavigationViewModel());
                SessionService.Authorized -= OnSessionAuthorizationCompleted;
                SessionService.StateChanged += SessionService_StateChanged;
            }
        }

        #region ChildViewModel
        private ViewModel _ChildViewModel = new PlugViewModel();
        /// <summary>
        /// Current UI view model
        /// </summary>
        public ViewModel ChildViewModel
        {
            get => _ChildViewModel;
            set
            {
                if (_ChildViewModel is not null)
                {
                    _ChildViewModel.Dispose();
                }
                if (Set(ref _ChildViewModel, value))
                {
                    if (value is not null)
                    {
                        switch (value)
                        {
                            case NavigationViewModel nav:
                                nav.LogoutRequested += OnLogoutRequested;
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Window Properties

        #region TitleBarVisibility
        private Visibility _TitleBarVisibility = Visibility.Collapsed;
        public Visibility TitleBarVisibility
        {
            get => _TitleBarVisibility;
            set => Set(ref _TitleBarVisibility, value);
        }
        #endregion

        #endregion

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            Settings.Default.PlayerId = 0;
            Settings.Default.PlayerNick = null;
            Settings.Default.access_token = null;
            Settings.Default.refresh_token = null;
            Settings.Default.id_token = null;
            Settings.Default.AutoJoin = false;
            App.Restart();
        }
    }
}
