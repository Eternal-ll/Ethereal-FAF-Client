using beta.Infrastructure.Commands;
using beta.Infrastructure.Services;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.OAuth;
using beta.Properties;
using beta.Views;
using Ethereal.FAF.LobbyServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Linq;
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
        public event EventHandler<bool> Authorized;
        private readonly IServiceProvider ServiceProvider;
        private readonly IOAuthService OAuthService;
        private readonly ISessionService SessionService;
        private readonly INotificationService NotificationService;
        private readonly NavigationService NavigationService;
        private readonly LobbyServer LobbyServer;
        public ServersViewModel Servers { get; private set; }

        public IProgress<string> Progress;
        private CancellationTokenSource CancellationTokenSource = new();
        public AuthorizationViewModel(IProgress<string> progress, IServiceProvider serviceProvider) : this(serviceProvider)
        {
            Progress = progress;
        }
        public AuthorizationViewModel(IServiceProvider serviceProvider)
        {
            OAuthService = serviceProvider.GetService<IOAuthService>();
            SessionService = serviceProvider.GetService<ISessionService>();
            NotificationService = serviceProvider.GetService<INotificationService>();
            NavigationService = serviceProvider.GetService<NavigationService>();
            Servers = serviceProvider.GetService<ServersViewModel>();
            LobbyServer = serviceProvider.GetService<LobbyServer>();


            LobbyServer.ServerLaunched += (s, e) =>
            {
                var servers = Servers.ClientEnvironments;
                servers.Remove(servers.FirstOrDefault(s => s.Name == "Local"));
                var local = new ClientEnvironment()
                {
                    Name = "Local",
                };
                local.Servers.Add(new()
                {
                    Name = "Lobby",
                    Host = e.Split(':')[0],
                    Port = int.Parse(e.Split(':')[1])
                });
                servers.Add(local);
                MessageBox.Show(e);
            };

            SessionService.AuthentificationFailed += SessionService_AuthentificationFailed;
            SessionService.Authorized += OnSessionAuthorizationCompleted;
            OAuthService.StateChanged += OAuthService_StateChanged;

            OAuthService.SetToken(
                Settings.Default.access_token,
                Settings.Default.refresh_token,
                Settings.Default.id_token,
                Settings.Default.ExpiresIn);

            if (Progress is null)
            {
                Progress = new Progress<string>((data) => ProgressData = data);
                ProgressTextThread = new Thread(UpdateProgressText)
                {
                    IsBackground = true
                };
                ProgressTextThread.Start();
            }

            BrowserName = GetDefaultBrowser();
            ServiceProvider = serviceProvider;
        }
        public async Task AuthBySavedToken()
        {
            IsPendingAuthorization = true;
            await OAuthService.AuthAsync(Progress)
                    .ContinueWith(async task => await HandleOAuthResultTask(task));
        }
        private void OnSessionAuthorizationCompleted(object sender, bool e)
        {
            Authorized?.Invoke(this, e);
        }

        /// <summary>
        /// Gets default user browser by registry data
        /// </summary>
        /// <returns>Name of browser or registry value or unknown</returns>
        private static string GetDefaultBrowser()
        {
            string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            using RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoice);
            return userChoiceKey is not null && userChoiceKey.GetValue("Progid") is string browser
                ? browser switch
                {
                    "IE.HTTP" => "Internet Explorer",
                    "FirefoxURL" => "Firefox",
                    "ChromeHTML" => "Chrome",
                    "OperaStable" => "Opera",
                    "SafariHTML" => "Safari",
                    "MSEdgeHTM" or "AppXq0fevzme2pys62n3e0fbqa7peapykr8v" => "Edge",
                    _ => browser,
                }
                : "Unknown";
        }

        public string BrowserName { get; set; }

        private void SessionService_AuthentificationFailed(object sender, Models.Server.AuthentificationFailedData e)
        {
            IsPendingAuthorization = false;
            NotificationService.ShowPopupAsync("Server authentification failed");
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
                }
            }
        }
        #endregion

        Thread ProgressTextThread;

        private void UpdateProgressText()
        {
            string points = string.Empty;
            while (true)
            {
                Thread.Sleep(500);
                if (!IsPendingAuthorization) continue;
                points = points.Length switch
                {
                    0 => ".",
                    1 => "..",
                    2 => "...",
                    _ => string.Empty,
                };
                ProgressText = ProgressData + points;
            }
        }

        public Visibility InputVisibility => !IsPendingAuthorization ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoadingInputVisibility => IsPendingAuthorization ? Visibility.Visible : Visibility.Collapsed;

        private string ProgressData;
        #region ProgressText - Authorization report text
        private string _ProgressText;
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
                Authorized?.Invoke(this, false);
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
                var server = Server.Servers.First(s => s.Name == "Lobby");
                var taskg = SessionService.AuthorizeAsync(server.Host, server.Port, task.Result.AccessToken, CancellationTokenSource.Token);
                taskg.Wait();
                CancellationTokenSource = new();
                if (taskg.IsFaulted)
                {
                    await NotificationService.ShowExceptionAsync(task.Exception);
                    IsPendingAuthorization = false;
                }
            }
        }

        #region Server
        private ClientEnvironment _Server;
        public ClientEnvironment Server
        {
            get => _Server;
            set => Set(ref _Server, value);
        }
        #endregion

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
            if (Server == null)
            {
                MessageBox.Show("Select server");
                return;
            }
            if (!Server.Servers.Any(s => s.Name == "Lobby"))
            {
                MessageBox.Show("No information about \"Lobby\" server in selected environment");
                return;
            }
            Visibility = Visibility.Visible;

            if (Server.Name == "Local")
            {
                var rndm = new Random();
                Task.Run(() => HandleOAuthResultTask(Task.Run(() => new TokenBearer()
                {
                    IdToken = rndm.Next(10000, 1000000).ToString(),
                    RefreshToken = rndm.Next(10000, 1000000).ToString(),
                    AccessToken = rndm.Next(10000, 1000000).ToString(),
                    ExpiresIn = 3600,
                    TokenType = "Bearer"
                })));
                return;
            }
            
            await OAuthService.AuthByBrowser(CancellationTokenSource.Token, Progress)
                .ContinueWith(task => HandleOAuthResultTask(task));
        }

        #endregion

        #region EditServersCommand
        private ICommand _EditServersCommand;
        public ICommand EditServersCommand => _EditServersCommand ??= new LambdaCommand(OnEditServersCommand);
        private void OnEditServersCommand(object paramter)
        {
            NavigationService.Navigate(typeof(ServersView));
        }
        #endregion

        #region CancelAuthorizationCommand - Cancel OAuthorizaion process
        private ICommand _CancelAuthorizationCommand;
        /// <summary>
        /// Cancel OAuthorization process
        /// </summary>
        public ICommand CancelAuthorizationCommand =>_CancelAuthorizationCommand ??= new LambdaCommand(OnCancelAuthorizationCommand, CanCancelAuthorizationCommand);
        private bool CanCancelAuthorizationCommand(object parameter) => IsPendingAuthorization && CancellationTokenSource.Token.CanBeCanceled;
        public void OnCancelAuthorizationCommand(object parameter)
        {
            CancellationTokenSource.Cancel();
            IsPendingAuthorization = false;
        }
        #endregion

        private ICommand _HostServerCommand;

        public ICommand HostServerCommand => _HostServerCommand ??= new LambdaCommand(OnHostServerCommand);
        private void OnHostServerCommand(object p)
        {
            LobbyServer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OAuthService.StateChanged -= OAuthService_StateChanged;
            }
        }
    }
}
