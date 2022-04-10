using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class AuthorizationViewModel : Base.ViewModel
    {
        private readonly IOAuthService OAuthService;
        private readonly INotificationService NotificationService;

        public AuthorizationViewModel()
        {
            OAuthService = App.Services.GetService<IOAuthService>();
            NotificationService = App.Services.GetService<INotificationService>();

            OAuthService.StateChanged += OAuthService_StateChanged;

            if (Settings.Default.AutoJoin)
            {
                Task.Run(() => OAuthService.AuthAsync());
            }
        }

        private void OAuthService_StateChanged(object sender, Models.OAuthEventArgs e)
        {
            IsPendingAuthorization = e.State == Models.Enums.OAuthState.PendingAuthorization || e.State == Models.Enums.OAuthState.AUTHORIZED;
            if (e.State != Models.Enums.OAuthState.AUTHORIZED & e.State != Models.Enums.OAuthState.PendingAuthorization)
            {
                NotificationService.ShowPopupAsync(e);
            }
        }

        #region IsPendingAuthorization
        private bool _IsPendingAuthorization;
        public bool IsPendingAuthorization
        {
            get => _IsPendingAuthorization;
            set
            {
                if (Set(ref _IsPendingAuthorization, value))
                {
                    OnPropertyChanged(nameof(InputVisibility));
                    OnPropertyChanged(nameof(LoadingInputVisibility));

                    new Thread(() =>
                    {
                        while (IsPendingAuthorization)
                        {
                            var state = State;
                            var data = state.Split('.');

                            string points = string.Empty;
                            switch (data.Length)
                            {
                                case 1:
                                    points = ".";
                                    break;
                                case 2:
                                    points = "..";
                                    break;
                                case 3:
                                    points = "...";
                                    break;
                            }
                            State = data[0] + points;
                            Thread.Sleep(500);
                        }
                    })
                    {
                        IsBackground = true
                    }.Start();
                }
            }
        }
        #endregion

        public Visibility InputVisibility => !IsPendingAuthorization ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoadingInputVisibility => IsPendingAuthorization ? Visibility.Visible : Visibility.Collapsed;

        #region IsTrailerSoundsOn
        private bool _IsTrailerSoundsOn = Settings.Default.IsTrailerSoundsOn;
        public bool IsTrailerSoundsOn
        {
            get => _IsTrailerSoundsOn;
            set
            {
                if (Set(ref _IsTrailerSoundsOn, value))
                {
                    Settings.Default.IsTrailerSoundsOn = value;
                }
            }
        }
        #endregion

        #region State
        private string _State = "Pending authorization";
        public string State
        {
            get => _State;
            set => Set(ref _State, value);
        }
        #endregion

        #region RememberMe
        private bool _RememberMe = Settings.Default.AutoJoin;
        public bool RememberMe
        {
            get => _RememberMe;
            set
            {
                if (Set(ref _RememberMe, value))
                {
                    Settings.Default.AutoJoin = value;
                }
            }
        }
        #endregion

        #region LoginOrEmail
        private string _LoginOrEmail;
        public string LoginOrEmail
        {
            get => _LoginOrEmail;
            set => Set(ref _LoginOrEmail, value);
        }
        #endregion

        #region LoginCommand
        private ICommand _LoginCommand;
        public ICommand LoginCommand => _LoginCommand ??= new LambdaCommand(OnLoginCommand, CanLoginCommand);
        private bool CanLoginCommand(object parameter) => !IsPendingAuthorization && !string.IsNullOrWhiteSpace(LoginOrEmail);
        private void OnLoginCommand(object parameter)
        {
            if (parameter == null) parameter = string.Empty;
            IsPendingAuthorization = true;
            var password = parameter.ToString();
            Task.Run(() => OAuthService.AuthAsync(LoginOrEmail, password));
        }
        #endregion

        #region LoginWithBrowserCommand
        private ICommand _LoginWithBrowserCommand;
        public ICommand LoginWithBrowserCommand => _LoginWithBrowserCommand ??= new LambdaCommand(OnLoginWithBrowserCommand, CanLoginWithBrowserCommand);
        private bool CanLoginWithBrowserCommand(object parameter) => !IsPendingAuthorization;
        private void OnLoginWithBrowserCommand(object parameter)
        {
            IsPendingAuthorization = true;
            Task.Run(() => OAuthService.AuthByBrowser());
        }
        #endregion

        #region CancelAuthorizationCommand
        private ICommand _CancelAuthorizationCommand;
        public ICommand CancelAuthorizationCommand => _CancelAuthorizationCommand ??= new LambdaCommand(OnCancelAuthorizationCommand, CanCancelAuthorizationCommand);
        private bool CanCancelAuthorizationCommand(object parameter) => IsPendingAuthorization;
        public void OnCancelAuthorizationCommand(object parameter)
        {
            IsPendingAuthorization = false;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OAuthService.StateChanged -= OAuthService_StateChanged;
            }

            base.Dispose(disposing);
        }
    }
}
