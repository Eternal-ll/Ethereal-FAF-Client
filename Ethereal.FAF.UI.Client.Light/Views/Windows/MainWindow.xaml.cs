// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Ethereal.FAF.UI.Client.Light.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Light.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Light.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;

namespace FAF.UI.EtherealClient.Views.Windows
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        private readonly IThemeService _themeService;
        private readonly ITaskBarService _taskBarService;
        private readonly IConfiguration Configuration;
        private readonly FafOAuthClient FafOAuthClient;
        private readonly LobbyClient LobbyClient;
        
        private bool _initialized = false;

        private IProgress<string> SpashProgress;

        // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()

        public MainWindow(
            ContainerViewModel viewModel,
            INavigationService navigationService,
            IPageService pageService,
            IThemeService themeService,
            ITaskBarService taskBarService,
            FafOAuthClient oauthClient,
            IConfiguration configuration,
            LobbyClient lobbyClient)
        {
            // Attach the theme service
            _themeService = themeService;

            // Attach the taskbar service
            _taskBarService = taskBarService;

            // Context provided by the service provider.
            DataContext = viewModel;

            FafOAuthClient = oauthClient;
            Configuration = configuration;

            // Initial preparation of the window.
            InitializeComponent();

            // We define a page provider for navigation
            SetPageService(pageService);

            // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
            navigationService.SetNavigationControl(RootNavigation);

            // !! Experimental option
            //RemoveTitlebar();

            // !! Experimental option
            //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

            // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
            Loaded += (_, _) => InvokeSplashScreen();
            LobbyClient = lobbyClient;

            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            // Wpf.Ui.Appearance.Watcher.Watch(this, Appearance.BackgroundType.Mica, true, false);
        }

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        private void InvokeSplashScreen()
        {
            if (_initialized)
                return;
            _initialized = true;
            SpashProgress = new Progress<string>(data => SplashProgressLabel.Text = data);

            RootMainGrid.Visibility = Visibility.Collapsed;
            TitleBar.Visibility = Visibility.Collapsed;
            RootWelcomeGrid.Visibility = Visibility.Visible;

            _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);

            Task.Run(async () =>
            {
                var jwt = Configuration.GetSection("JwtToken").Get<TokenBearer>();
                if (jwt.AccessToken is null)
                {
                    SpashProgress.Report("Waiting for authorization");
                    await Auth();
                }

                // Remember to always include Delays and Sleeps in
                // your applications to be able to charge the client for optimizations later.
                //await Task.Delay(-1);

                Dispatcher.Invoke(() =>
                {
                    RootWelcomeGrid.Visibility = Visibility.Hidden;
                    RootMainGrid.Visibility = Visibility.Visible;
                    TitleBar.Visibility = Visibility.Visible;
                    //RenderSize = new(1280, 720);
                    Width = 1280;
                    Height = 720;
                    //Navigate(typeof(.Dashboard));

                    _taskBarService.SetState(this, TaskBarProgressState.None);
                }, System.Windows.Threading.DispatcherPriority.Send);

                return true;
            });
        }

        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(_themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark);
        }

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Tray clicked: {menuItem.Tag}", "Wpf.Ui.Demo");
        }

        private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}", "Wpf.Ui.Demo");

            // This funky solution allows us to impose a negative
            // margin for Frame only for the Dashboard page, thanks
            // to which the banner will cover the entire page nicely.
            RootFrame.Margin = new Thickness(
                left: 0,
                top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
                right: 0,
                bottom: 0);
        }

        private void RootDialog_OnButtonRightClick(object sender, RoutedEventArgs e)
        {
            RootDialog.Hide();
        }

        private async Task Auth()
        {
            var token = await Task.Run(async () =>
            {
                var client = FafOAuthClient;
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(60000);
                return await FafOAuthClient.AuthByBrowser(cancellationTokenSource.Token);
            });
            if (token is null)
            {
                var result = await Dispatcher.InvokeAsync(() => MessageBox.Show("Failed to get response from FAForever.\nPress \"OK\" to try again, or \"Cancel\" to close application",
                    "Oauth", MessageBoxButton.OKCancel), System.Windows.Threading.DispatcherPriority.Background);
                if (result == MessageBoxResult.OK || result == MessageBoxResult.None)
                {
                    await Auth();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadToken(token.AccessToken) as JwtSecurityToken;
                if (jwtSecurityToken.Payload.TryGetValue("ext", out var ext))
                {
                    var user = JsonSerializer.Deserialize<Ext>(ext.ToString()).Username;
                    MessageBox.Show($"Succefully got response from FAForever\nWelcome {user}", "OAuth");
                }
                SpashProgress.Report("Connecting to server");

            }
        }
    }
}

