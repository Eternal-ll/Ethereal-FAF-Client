using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.OAuth;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
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
            SessionService.AuthentificationFailed += SessionService_AuthentificationFailed;
            NotificationService = App.Services.GetService<INotificationService>();

            OAuthService.StateChanged += OAuthService_StateChanged;

            // TODO Add settings to not include reporting
            Progress = new Progress<string>((data) => ProgressText = data);

            if (Settings.Default.AutoJoin && File.Exists("User.TokenBearer.txt"))
            {
                Task.Run(() =>
                {
                    OAuthService.SetToken(
                        Settings.Default.access_token,
                        Settings.Default.refresh_token,
                        Settings.Default.id_token,
                        Settings.Default.ExpiresIn);
                    OAuthService.AuthAsync(Progress)
                    .ContinueWith(task => HandleOAuthResultTask(task));
                });
            }

            BrowserName = GetDefaultBrowser();
        }

        private string GetDefaultBrowser()
        {
            string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            string progId;
            using RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoice);
            if (userChoiceKey == null)
            {
                return "Unknown";
            }
            object progIdValue = userChoiceKey.GetValue("Progid");
            if (progIdValue == null)
            {
                return "Unknown";
            }
            progId = progIdValue.ToString();
            return progId switch
            {
                "IE.HTTP" => "Internet Explorer",
                "FirefoxURL" => "Firefox",
                "ChromeHTML" => "Chrome",
                "OperaStable" => "Opera",
                "SafariHTML" => "Safari",
                "MSEdgeHTM" or "AppXq0fevzme2pys62n3e0fbqa7peapykr8v" => "Edge",
                _ => progId,
            };
        }

        public string BrowserName { get; set; }

        private void SessionService_AuthentificationFailed(object sender, Models.Server.AuthentificationFailedData e)
        {
            IsPendingAuthorization = false;
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

                    if (!value)
                    {
                        Visibility = Visibility.Collapsed;
                    }
                    if (ProgressTextThread is not null) return;
                    ProgressTextThread = new Thread(UpdateProgressText)
                    {
                        IsBackground = true
                    };
                    ProgressTextThread.Start();
                }
            }
        }
        #endregion

        Thread ProgressTextThread;

        private void UpdateProgressText()
        {
            string points = string.Empty;
            while (IsPendingAuthorization)
            {
                //bool removeThree = points.Length == 3;
                //ProgressText = removeThree ? ProgressText[..^3] : ProgressText[..^(points.Length > 0 ? 3 - points.Length : 0)] + points;
                //points = points.Length switch
                //{
                //    0 => ".",
                //    1 => "..",
                //    2 => "...",
                //    _ => string.Empty,
                //};
                Thread.Sleep(3500);
            }
            ProgressTextThread = null;
        }

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
            if (CancellationTokenSource.IsCancellationRequested)
            {
                IsPendingAuthorization = false;
            }
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
                            IsPendingAuthorization = false;
                        }
                        else if (task.IsCompleted || task.IsCompletedSuccessfully)
                        {
                            Authorized?.Invoke(this, null);
                        }
                    });
            }
        }

        #region Visibility
        private Visibility _Visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _Visibility;
            set => Set(ref _Visibility, value);
        }
        #endregion

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
        private async void OnLoginWithBrowserCommand(object parameter)
        {
            Visibility = Visibility.Visible;
            await OAuthService.AuthByBrowser(CancellationTokenSource.Token, Progress)
                .ContinueWith(task => HandleOAuthResultTask(task));
        }

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
