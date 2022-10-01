// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.


using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.ViewModels;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
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
        private readonly LobbyClient LobbyClient;
        private readonly ContainerViewModel Container;
        private readonly IServiceProvider ServiceProvider;
        private readonly TokenProvider TokenProvider;
        private readonly PatchClient PatchClient;

        private readonly IHost Host;

        // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()

        public MainWindow(
            ContainerViewModel viewModel,
            INavigationService navigationService,
            IPageService pageService,
            IThemeService themeService,
            ITaskBarService taskBarService,
            IConfiguration configuration,
            LobbyClient lobbyClient,
            IServiceProvider serviceProvider,
            TokenProvider tokenProvider,
            IHost host,
            PatchClient patchClient)
        {
            // Attach the theme service
            _themeService = themeService;

            // Attach the taskbar service
            _taskBarService = taskBarService;

            // Context provided by the service provider.
            DataContext = viewModel;

            Container = viewModel;
            Configuration = configuration;
            LobbyClient = lobbyClient;
            lobbyClient.Authorized += LobbyViewModel_Authorized;

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
            ServiceProvider = serviceProvider;
            TokenProvider = tokenProvider;
            Host = host;
            PatchClient = patchClient;

            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            // Wpf.Ui.Appearance.Watcher.Watch(this, Appearance.BackgroundType.Mica, true, false);
        }

        private void LobbyViewModel_Authorized(object sender, bool e) => 
            Dispatcher.Invoke(() =>
            {
                if (e)
                {
                    _taskBarService.SetState(this, TaskBarProgressState.None);
                    if (Container.Content is not null) return;
                    Container.SplashVisibility = Visibility.Collapsed;
                    Container.SplashProgressVisibility = Visibility.Collapsed;
                    //Container.SplashText = "Waiting ending of match";
                    //RenderSize = new(1280, 720);
                    //Width = 1280;
                    //Height = 720;
                    //Navigate(typeof(.Dashboard));

                }
                else
                {
                    _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);
                    Container.SplashProgressVisibility = Visibility.Visible;
                    Container.SplashVisibility = Visibility.Visible;
                }
            }, System.Windows.Threading.DispatcherPriority.Send);

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override async void OnClosed(EventArgs e)
        {
            await Host.StopAsync();
            base.OnClosed(e);
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
            _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);
            Task.Run(async () =>
            {
                await PrepareJavaRuntime();
                await InitalizePathWatchers();
                await Auth();
            });
        }

        private async Task InitalizePathWatchers()
        {
            Container.SplashText = "Initializing patch watcher";
            var progress = new Progress<string>(d => Container.SplashText = d);
            await PatchClient.InitializeWatchers(progress);
            Container.SplashText = "Patch watcher initialized";
        }

        private async Task PrepareJavaRuntime()
        {
            if (!Directory.Exists("External/jre"))
            {
                Container.SplashText = "Extracting portable Java runtime";
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "External/7z.exe",
                        Arguments = "x External/jre.7z -oExternal/",
                        UseShellExecute = false
                    }

                };
                process.Start();
                await process.WaitForExitAsync();
                Container.SplashText = "Poratble Java runtime extracted";
            }
        }

        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(_themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark);
        }

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;
        }

        private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
        {
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
        private IProgress<string> SplashProgress;

        private async Task Auth()
        {
            SplashProgress = new Progress<string>(d => Container.SplashText = d);
            SplashProgress.Report("Processing authorization");
            var token = TokenProvider.TokenBearer;
            var oauth = ServiceProvider.GetService<FafOAuthClient>();
            oauth.OAuthLinkGenerated += OnOAuthLinkGenerated;
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
                        ClearLink();
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

        private void OnOAuthLinkGenerated(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                LinkTextBox.Text = e;
                LinkTextBox.Visibility = Visibility.Visible;
            });
        }
        private void ClearLink() => Dispatcher.Invoke(() =>
        {
            LinkTextBox.Visibility = Visibility.Collapsed;
            LinkTextBox.Text = null;
        });

        public Task<bool> ShowNotification(string title, string message, SymbolRegular icon)
        {
            return RootSnackbar.ShowAsync(title, message, icon);
        }

        private void LinkTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(LinkTextBox.Text);
            ShowNotification("Notification", "Copied!", SymbolRegular.Copy24);
        }
    }
}

