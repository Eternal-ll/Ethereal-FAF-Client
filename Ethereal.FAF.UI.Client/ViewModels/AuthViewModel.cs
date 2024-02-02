using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class AuthViewModel : Base.ViewModel
    {
        private readonly IWindowService _windowService;
        private readonly ISnackbarService _snackbarService;
        private readonly INavigationWindow _navigationWindow;
        private readonly IFafAuthService _fafAuthService;
        private readonly HttpAuthServer _httpAuthServer;

        private int redirectPort;

        public AuthViewModel(IWindowService windowService, ISnackbarService snackbarService, ClientManager serverManager, INavigationWindow navigationWindow, IFafAuthService fafAuthService, UpdateViewModel updateViewModel)
        {
            _httpAuthServer = new();
            _httpAuthServer.CodeReceived += _httpAuthServer_CodeReceived;

            SelectedServer = serverManager.GetServer();

            _windowService = windowService;
            _snackbarService = snackbarService;
            _navigationWindow = navigationWindow;
            _fafAuthService = fafAuthService;
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
                .ContinueWith(x =>
                {
                    if (_fafAuthService.IsAuthorized())
                    {
                        AuthorizedUserName = _fafAuthService.GetUserName();
                    }
                    IsAuthorized = true;
                    _navigationWindow.Navigate(typeof(LobbyConnectionView));
                }, TaskScheduler.Default)
                .SafeFireAndForget();
        }

        protected override void OnInitialLoaded()
        {
            IsAuthorized = _fafAuthService.IsAuthorized();
            if (IsAuthorized)
            {
                AuthorizedUserName = _fafAuthService.GetUserName();
            }
        }

        public override void OnUnloaded()
        {
            if (_httpAuthServer.IsListening) _httpAuthServer.StopListener();
            _httpAuthServer.CodeReceived -= _httpAuthServer_CodeReceived;
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
        [ObservableProperty]
        public bool _IsAuthorized;
        [ObservableProperty]
        private string _AuthorizedUserName;
        [ObservableProperty]
        private bool _IsAuthenticating;
        [ObservableProperty]
        private string _SplashText;

        [RelayCommand]
        private void OpenUpdateClientView() => _navigationWindow.Navigate(typeof(UpdateClientView));
        [RelayCommand]
        private void LoginAsLastUser() => _navigationWindow.Navigate(typeof(LobbyConnectionView));
        [RelayCommand]
        private void OnSelectServer(object arg)
        {
            if (_httpAuthServer.IsListening) _httpAuthServer.StopListener();
            _navigationWindow.Navigate(typeof(SelectServerView));
        }
        [RelayCommand]
        private void OnLogin(object arg)
        {
            IsAuthenticating = true;
            SplashText = "Waiting for your authorization...";
            redirectPort = _httpAuthServer.StartListener(SelectedServer.OAuth.RedirectPorts);
            OnAuthByBrowser(null);
        }
        [RelayCommand]
        public void OnCancelAuthenticating(object arg)
        {
            if (!IsAuthenticating) return;
            IsAuthenticating = false;
            _httpAuthServer.StopListener();
        }
        [RelayCommand]
        public void OnCopyAuthUrl(object arg)
        {
            Clipboard.SetText(GetAuthUrl());
            _snackbarService.Show("Clipboard", "Link copied!", ControlAppearance.Primary, null, TimeSpan.FromSeconds(5));
        }
        [RelayCommand]
        public void OnAuthByBrowser(object arg)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GetAuthUrl(),
                    UseShellExecute = true,
                });
            }
            catch
            {
                _snackbarService.Show("Auth", "Failed to open browser, copy url and open it manually.");
            }
        }
        [RelayCommand]
        private void OnAuthByPopupBrowser(object arg)
        {
            var window = _windowService.GetWindow<WebViewWindow>();
            _httpAuthServer.CodeReceived += window.CodeReceived;
            var url = GetAuthUrl();
            window.WebView.Source = new(url);
            window.Show();
        } 
    }
}
