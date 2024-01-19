using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels.Dialogs;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Windows;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class AuthViewModel : Base.ViewModel
    {
        private readonly IWindowService _windowService;
        private readonly ISnackbarService _snackbarService;
        private readonly INavigationWindow _navigationWindow;
        private readonly IFafAuthService _fafAuthService;
        private readonly IFafLobbyService _fafLobbyService;
        private readonly HttpAuthServer _httpAuthServer;

        private readonly ClientManager _serverManager;

        private int redirectPort;

        public AuthViewModel(IWindowService windowService, ISnackbarService snackbarService, ClientManager serverManager, INavigationWindow navigationWindow, IFafAuthService fafAuthService, IFafLobbyService lobbyService, UpdateViewModel updateViewModel)
        {
            LoginAsLastUserCommand = new LambdaCommand(async (arg) => await ConnectToLobby(),
                (arg) =>
                _fafAuthService.IsAuthorized());
            LoginCommand = new LambdaCommand(OnLoginCommand);
            CancelAuthenticatingCommand = new LambdaCommand(OnCancelAuthenticatingCommand, CanCancelAuthenticatingCommand);
            GoToSelectServerViewCommand = new LambdaCommand(OnGoToSelectServerViewCommand);
            AuthByPopupBrowserCommand = new LambdaCommand(OnAuthByPopupBrowser, CanAuthByPopupBrowser);
            AuthByBrowserCommand = new LambdaCommand(OnAuthByBrowserCommand, CanAuthByBrowserCommand);
            CopyAuthUrlCommand = new LambdaCommand(OnCopyAuthUrlCommand, CanCopyAuthUrlCommand);

            _httpAuthServer = new();
            _httpAuthServer.CodeReceived += _httpAuthServer_CodeReceived;

            SelectedServer = serverManager.GetServer();

            _windowService = windowService;
            _snackbarService = snackbarService;
            _navigationWindow = navigationWindow;
            _fafAuthService = fafAuthService;
            _serverManager = serverManager;
            _fafLobbyService = lobbyService;
            UpdateVM = updateViewModel;
        }

        private void _httpAuthServer_CodeReceived(object sender, (string code, string state) e)
        {
            if (_navigationWindow is Window window)
            {
                window.Dispatcher.Invoke(window.Activate);
            }
            SplashText = "Athorization code received. Fetching OAuth2 token...";
            _fafAuthService
                .FetchTokenByCode(e.code, GetRedirectUrl())
                .ContinueWith(async x =>
                {
                    if (_fafAuthService.IsAuthorized())
                    {
                        AuthorizedUserName = _fafAuthService.GetUserName();
                    }
                    OnPropertyChanged(nameof(IsAuthorized));

                    SplashText = "OAuth2 token received!";
                })
                .SafeFireAndForget();
        }
        private CancellationTokenSource CancellationTokenSource;
        private async Task ConnectToLobby()
        {
            IsAuthenticating = true;
            CancellationTokenSource = new();
            SplashText = "Connecting to lobby...";
            await _fafLobbyService
                .ConnectAsync(CancellationTokenSource.Token)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        Application.Current.Dispatcher.Invoke(() => _snackbarService.Show("Lobby", "Unable to connect to lobby", ControlAppearance.Primary,  null, TimeSpan.FromSeconds(5)));
                        IsAuthenticating = false;
                        return;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    _navigationWindow.Navigate(typeof(Views.NavigationView)));
                });
        }

        protected override void OnInitialLoaded()
        {
            if (_fafAuthService.IsAuthorized())
            {
                AuthorizedUserName = _fafAuthService.GetUserName();
            }
        }

        public override void OnUnloaded()
        {
            if (_httpAuthServer.IsListening) _httpAuthServer.StopListener();
        }
        private string GetRedirectUrl() => $"http://localhost:{redirectPort}";

        private string GetAuthUrl()
        {
            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var redirectUrl = GetRedirectUrl();
            var sb = new StringBuilder()
                .Append($"{SelectedServer.OAuth.BaseAddress.ToString()}oauth2/auth?")
                .Append($"response_type=code&client_id={SelectedServer.OAuth.ClientId}&scope={SelectedServer.OAuth.Scope}&state={generatedState}&redirect_uri={redirectUrl}");
            return sb.ToString();
        }

        public Server SelectedServer { get; set; }
        public UpdateViewModel UpdateVM { get; }

        [ObservableProperty]
        private bool _UpdateAvailable;

        #region IsAuthorized
        public bool IsAuthorized => _fafAuthService.IsAuthorized();
        #endregion

        #region AuthorizedUserName
        private string _AuthorizedUserName;
        public string AuthorizedUserName { get => _AuthorizedUserName; set => Set(ref _AuthorizedUserName, value); }
        #endregion

        #region IsAuthenticating
        private bool _IsAuthenticating;
        public bool IsAuthenticating { get => _IsAuthenticating; set => Set(ref _IsAuthenticating, value); }
        #endregion

        #region SplashText
        private string _SplashText;
        public string SplashText { get => _SplashText; set => Set(ref _SplashText, value); }
        #endregion

        [RelayCommand]
        private void OpenUpdateClientView()
        {
            _navigationWindow.Navigate(typeof(UpdateClientView));
        }

        #region LoginAsLastUserCommand
        public ICommand LoginAsLastUserCommand { get; set; }
        #endregion

        #region GoToSelectServerViewCommand
        public ICommand GoToSelectServerViewCommand { get; set; }
        private void OnGoToSelectServerViewCommand(object arg)
        {
            if (_httpAuthServer.IsListening)
            {
                _httpAuthServer.StopListener();
            }
            _navigationWindow.Navigate(typeof(SelectServerView));
        }
        #endregion

        #region LoginCommand
        public ICommand LoginCommand { get; set; }
        private void OnLoginCommand(object arg)
        {
            IsAuthenticating = true;
            SplashText = "Waiting for your authorization...";
            redirectPort = _httpAuthServer.StartListener(SelectedServer.OAuth.RedirectPorts);
            //OnAuthByBrowserCommand(arg);
        }

        #endregion

        #region CancelAuthenticatingCommand
        public ICommand CancelAuthenticatingCommand { get; set; }
        public bool CanCancelAuthenticatingCommand(object arg) => IsAuthenticating;
        public void OnCancelAuthenticatingCommand(object arg)
        {
            IsAuthenticating = false;
            CancellationTokenSource?.Cancel();
            _httpAuthServer.StopListener();
        }
        #endregion

        #region CopyAuthUrlCommand
        public ICommand CopyAuthUrlCommand { get; }
        public bool CanCopyAuthUrlCommand(object arg) => true;
        public void OnCopyAuthUrlCommand(object arg)
        {
            System.Windows.Clipboard.SetText(GetAuthUrl());
            _snackbarService.Show("Clipboard", "Link copied!", ControlAppearance.Primary, null, TimeSpan.FromSeconds(5));
        }
        #endregion

        #region AuthByBrowserCommand
        public ICommand AuthByBrowserCommand { get; }
        public bool CanAuthByBrowserCommand(object arg) => true;
        public void OnAuthByBrowserCommand(object arg) => Process.Start(new ProcessStartInfo
        {
            FileName = GetAuthUrl(),
            UseShellExecute = true,
        });
        #endregion

        #region AuthByPopupBrowserCommand
        public ICommand AuthByPopupBrowserCommand { get; }
        private bool CanAuthByPopupBrowser(object arg) => OperatingSystem.IsWindows();
        private void OnAuthByPopupBrowser(object arg)
        {
            var window = _windowService.GetWindow<WebViewWindow>();
            _httpAuthServer.CodeReceived += (s, e) =>
            {
                window.Close();
            };
            var url = GetAuthUrl();
            window.WebView.Source = new(url);
            window.Show();
        } 
        #endregion
    }
}
