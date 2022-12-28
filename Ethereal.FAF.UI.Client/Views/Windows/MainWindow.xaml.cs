// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.


using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace FAF.UI.EtherealClient.Views.Windows
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public sealed partial class MainWindow : INavigationWindow
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IConfiguration Configuration;
        private readonly IHost Host;

        private readonly NavigationView NavigationView;
        private readonly TokenProvider TokenProvider;
        private readonly LobbyClient LobbyClient;

        public MainWindow(
            INavigationService navigationService,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ITokenProvider tokenProvider,
            IHost host,
            NavigationView navigationView,
            ContainerViewModel viewModel,
            LobbyClient lobbyClient)
        {

            // Context provided by the service provider.
            DataContext = viewModel;

            Configuration = configuration;
            LobbyClient = lobbyClient;
            NavigationView = navigationView;
            ServiceProvider = serviceProvider;
            TokenProvider = (TokenProvider)tokenProvider;
            Host = host;

            LobbyClient.AuthentificationFailed += LobbyClient_AuthentificationFailed;

            // Initial preparation of the window.
            InitializeComponent();

            // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
            navigationService.SetNavigationControl(navigationView.RootNavigation);

            // !! Experimental option
            //RemoveTitlebar();

            // !! Experimental option
            //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

            // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
            //Loaded += (_, _) =>
            //{
            //    Navigate(typeof(LoaderView));
            //};

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            //Wpf.Ui.Appearance.Watcher.Watch(this, Configuration.GetValue<BackgroundType>("UI:BackgroundType"), true, false);
        }

        private void LobbyClient_AuthentificationFailed(object sender, Domain.LobbyServer.AuthentificationFailedData e)
        {
            TokenProvider.Save(null);
            Task.Run(Auth);
        }

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override async void OnClosed(EventArgs e)
        {
            await Host.StopAsync();
            base.OnClosed(e);
        }

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => NavigationView.RootNavigation;

        public bool Navigate(Type pageType) => RootFrame.Navigate(ServiceProvider.GetRequiredService(pageType));

        public void SetPageService(IPageService pageService) 
            => NavigationView.RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;
        }

        private void RootDialog_OnButtonRightClick(object sender, RoutedEventArgs e)
        {
            RootDialog.Hide();
        }
        private IProgress<string> SplashProgress;

        private async Task Auth()
        {
            SplashProgress = new Progress<string>();
            SplashProgress.Report("Processing authorization");
            var token = TokenProvider.TokenBearer;
            var oauth = ServiceProvider.GetService<FafOAuthClient>();
            var result = await Task.Run(async () =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var wait = Configuration.GetValue<int>("FAForever:OAuth:ResponseSeconds");
                var waitTimeSpan = TimeSpan.FromSeconds(wait);
                if (token is not null && token.RefreshToken is not null && !token.IsExpired)
                {
                    SplashProgress.Report($"Using saved access token");
                    return new OAuthResult()
                    {
                        IsError = false,
                        TokenBearer = token
                    };
                }
                var timer = new Timer((e) =>
                {
                    if (cancellationTokenSource.IsCancellationRequested) return;
                    try
                    {
                        SplashProgress.Report($"Waiting {TimeSpan.FromSeconds(wait).Humanize()} for response from FAForever OAuth");
                    }
                    catch
                    {

                    }
                    if (wait == 0)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    wait--;
                }, null, 0, 1000);
                return token is not null && token.RefreshToken is not null && token.IsExpired ?
                    await oauth.RefreshToken(token.RefreshToken, cancellationTokenSource.Token, SplashProgress) : 
                    await oauth.AuthByBrowser(cancellationTokenSource.Token, SplashProgress)
                    .ContinueWith(t =>
                    {
                        timer.Dispose();
                        var human = "";
                        try
                        {
                            human = waitTimeSpan.Humanize();
                        }
                        catch
                        {

                        }
                        var result = new OAuthResult()
                        {
                            ErrorDescription = $"Failed to get respone after {human}"
                        };
                        if (t.IsFaulted) return result;
                        if (t.IsCanceled)
                        {
                            result.ErrorDescription = $"Attempt was cancelled after {human}";
                            return result;
                        }
                        return t.Result;
                    });
            });
            if (result.IsError)
            {
                var wait = 5;
                var timer = new Timer((e) =>
                {
                    if (wait == 0) return;
                    try
                    {
                        SplashProgress.Report(result.ErrorDescription + $". Next attempt in {TimeSpan.FromSeconds(wait).Humanize()}");
                    }
                    catch
                    {

                    }
                    wait--;
                }, null, 0, 1000);
                await Task.Delay(5000);
                timer.Dispose();
                await Auth();
            }
            else
            {
                TokenProvider.Save(result.TokenBearer);
                if (TokenProvider.JwtSecurityToken.Payload.TryGetValue("ext", out var ext))
                {
                    var user = JsonSerializer.Deserialize<Ext>(ext.ToString()).Username;
                    SplashProgress.Report($"Hi, {user} !");
                    LobbyClient.ConnectAndAuthorizeAsync(result.TokenBearer.AccessToken, SplashProgress);
                }
            }
        }
    }
}

