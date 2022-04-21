using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.OAuth;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace beta.ViewModels
{
    /// <summary>
    /// OAuthorization view model
    /// </summary>
    public class AuthorizationViewModel : Base.ViewModel
    {
        public event EventHandler Authorized;

        private readonly IOAuthService OAuthService;
        private readonly ISessionService SessionService;
        private readonly INotificationService NotificationService;

        private readonly IProgress<string> Progress;
        private CancellationTokenSource CancellationTokenSource = new();

        public AuthorizationViewModel()
        {
            OAuthService = App.Services.GetService<IOAuthService>();
            SessionService = App.Services.GetService<ISessionService>();
            NotificationService = App.Services.GetService<INotificationService>();

            OAuthService.StateChanged += OAuthService_StateChanged;

            // TODO Add settings to not include reporting
            Progress = new Progress<string>((data) => ProgressText = data);

            if (Settings.Default.AutoJoin)
            {
                OAuthService.SetToken(
                    Settings.Default.access_token,
                    Settings.Default.refresh_token,
                    Settings.Default.id_token,
                    Settings.Default.ExpiresIn);

                Task.Run(() => AuthorizeAsync());
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
                            var state = ProgressText;
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
                            ProgressText = data[0] + points;
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

        #region ProgressText - Authorization report text
        private string _ProgressText = "Pending authorization";
        public string ProgressText
        {
            get => _ProgressText;
            set
            {
                if (Set(ref _ProgressText, value))
                {

                }
            }
        }
        #endregion

        #region IsTrailerSoundsOn - Trailer sounds on / off
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

        #region RememberMe - auto join
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

        #region LoginOrEmail - user login or E-mail
        private string _LoginOrEmail;
        public string LoginOrEmail
        {
            get => _LoginOrEmail;
            set => Set(ref _LoginOrEmail, value);
        }
        #endregion

        private async Task HandleOAuthResultTask(Task<TokenBearer> task)
        {
            CancellationTokenSource = new();
            if (task.IsFaulted)
            {
                await NotificationService.ShowExceptionAsync(task.Exception);
            }
            if (task.IsCanceled || task.IsFaulted)
            {
                Settings.Default.access_token = null;
                Settings.Default.refresh_token = null;
                Settings.Default.id_token = null;
                Settings.Default.ExpiresIn = 0;
                Settings.Default.ExpiresAt = DateTime.UtcNow;
                IsPendingAuthorization = false;
            }
            else if ((task.IsCompleted || task.IsCompletedSuccessfully) && task.Result is not null)
            {
                Settings.Default.access_token = task.Result.AccessToken;
                Settings.Default.refresh_token = task.Result.RefreshToken;
                Settings.Default.id_token = task.Result.IdToken;
                Settings.Default.ExpiresIn = task.Result.ExpiresIn;
                Settings.Default.ExpiresAt = task.Result.ExpiresAt;
                await SessionService.AuthorizeAsync(task.Result.AccessToken, CancellationTokenSource.Token)
                    .ContinueWith(async task =>
                    {
                        CancellationTokenSource = new();
                        if (task.IsFaulted)
                        {
                            await NotificationService.ShowExceptionAsync(task.Exception);
                        }
                        else if (task.IsCompleted || task.IsCompletedSuccessfully)
                        {
                            Authorized?.Invoke(this, null);
                        }
                    });
            }
        }

        private async Task AuthorizeAsync() => await OAuthService.AuthAsync(Progress)
            .ContinueWith(task => HandleOAuthResultTask(task));

        #region LoginCommand - Log in using parsing
        private ICommand _LoginCommand;
        /// <summary>
        /// Log in using OAuthservice parsing parsing with GET / POST
        /// </summary>
        public ICommand LoginCommand => _LoginCommand ??= new LambdaCommand(OnLoginCommand, CanLoginCommand);
        private bool CanLoginCommand(object parameter) => !IsPendingAuthorization && !string.IsNullOrWhiteSpace(LoginOrEmail);
        private async void OnLoginCommand(object parameter) =>
            await OAuthService.AuthAsync(LoginOrEmail, parameter?.ToString(), CancellationTokenSource.Token, Progress)
                .ContinueWith(task => HandleOAuthResultTask(task));
        #endregion

        #region LoginWithBrowserCommand - Log in using web browser
        private ICommand _LoginWithBrowserCommand;
        /// <summary>
        /// Log in using browser and callbacks for local HTTP listener
        /// </summary>
        public ICommand LoginWithBrowserCommand => _LoginWithBrowserCommand ??= new LambdaCommand(OnLoginWithBrowserCommand, CanLoginWithBrowserCommand);
        private bool CanLoginWithBrowserCommand(object parameter) => !IsPendingAuthorization;
        private async void OnLoginWithBrowserCommand(object parameter) => await OAuthService.AuthByBrowser(CancellationTokenSource.Token, Progress)
                .ContinueWith(task => HandleOAuthResultTask(task));

        #endregion

        #region CancelAuthorizationCommand - Cancel OAuthorizaion process
        private ICommand _CancelAuthorizationCommand;
        /// <summary>
        /// Cancel OAuthorization process
        /// </summary>
        public ICommand CancelAuthorizationCommand =>_CancelAuthorizationCommand ??= new LambdaCommand(OnCancelAuthorizationCommand, CanCancelAuthorizationCommand);
        private bool CanCancelAuthorizationCommand(object parameter) => IsPendingAuthorization && CancellationTokenSource.Token.CanBeCanceled;
        public void OnCancelAuthorizationCommand(object parameter) => CancellationTokenSource.Cancel();
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OAuthService.StateChanged -= OAuthService_StateChanged;
            }
        }
    }
}
