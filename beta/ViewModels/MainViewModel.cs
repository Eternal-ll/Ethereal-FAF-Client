using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
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

            var isAutojoin = Settings.Default.AutoJoin;

            isAutojoin = false;

            SessionService.Authorized += OnSessionAuthorizationCompleted;
            SessionService.NotificationReceived += SessionService_NotificationReceived;

            //if (isAutojoin)
            //{
            //    //Task.Run(async () => await OAuthService.AuthAsync());
            //    var model = new SessionAuthorizationViewModel();
            //    model.Completed += OnSessionAuthorizationCompleted;
            //    ChildViewModel = model;
            //}
            //else
            //{
            //OAuthService.StateChanged += OnOAuthServiceStateChanged;
            //}

            Task.Run(() =>
            {
                ChildViewModel = new AuthorizationViewModel();
                //ChildViewModel = new NavigationViewModel();
            });
        }

        private void SessionService_NotificationReceived(object sender, Models.Server.NotificationData e)
        {
            if (e.text.Contains("unofficial", System.StringComparison.OrdinalIgnoreCase)) return;
            NotificationService.ShowPopupAsync(e.text);
        }

        private void OnSessionAuthorizationCompleted(object sender, bool e)
        {
            if (e)
            {
                ChildViewModel = new NavigationViewModel();
                SessionService.Authorized -= OnSessionAuthorizationCompleted;
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

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            Settings.Default.PlayerId = 0;
            Settings.Default.PlayerNick = null;
            Settings.Default.access_token = null;
            Settings.Default.refresh_token = null;
            Settings.Default.id_token = null;
            Settings.Default.AutoJoin = false;
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location[..^3] + "exe");
            Application.Current.Shutdown();
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
    }
}
