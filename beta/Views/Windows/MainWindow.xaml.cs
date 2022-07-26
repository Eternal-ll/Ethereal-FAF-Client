using beta.Infrastructure.Services;
using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NavigationService NavigationService;
        private readonly ISessionService SessionService;
        private readonly IOAuthService OAuthService;
        private readonly MainViewModel MainViewModel;

        public MainWindow(NavigationService navigationService, ISessionService sessionService, MainViewModel viewModel, IOAuthService oAuthService)
        {
            MainViewModel = viewModel;
            DataContext = viewModel;
            NavigationService = navigationService;
            OAuthService = oAuthService;
            SessionService = sessionService;
            InitializeComponent();
            navigationService.SetFrame(NavigationFrame);
            Loaded += (_, _) => InvokeSplashScreen();
            Closing += OnWindowClosing;
            sessionService.Authorized += SessionService_Authorized;
        }

        private void SessionService_Authorized(object sender, bool e)
        {
            AuthorizationResult = e;
        }

        private string ProgressData = "Initializing";
        private bool? AuthorizationResult;
        private void InvokeSplashScreen()
        {
            Settings.Default.AutoJoin = false;
            Width = 500;
            Height = 500;
            ResizeMode = ResizeMode.NoResize;
            NavigationView.Visibility = Visibility.Collapsed;
            //TitleBarButton.Visibility = Visibility.Collapsed;
            LoadingScreen.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
            Task.Run(async () =>
            {
                var isPreparing = true;
                _ = Task.Run(async () =>
                {
                    string points = string.Empty;
                    while (isPreparing && AuthorizationResult is null)
                    {
                        if (!isPreparing || AuthorizationResult is not null) break;
                        points = points.Length switch
                        {
                            0 => ".",
                            1 => "..",
                            2 => "...",
                            _ => string.Empty,
                        };
                        await Dispatcher.InvokeAsync(() => ProgressText.Text = ProgressData + points);
                        await Task.Delay(500);
                    }
                });
                await Task.Delay(1500);
                if (Settings.Default.AutoJoin)
                {
                    ProgressData = "Authenticating";
                    Progress<string> progress = new((data) =>
                    ProgressData = data);
                    var authorization = new AuthorizationViewModel(progress);
                    authorization.Authorized += (_, arg) => AuthorizationResult = arg;
                    await authorization.AuthBySavedToken();
                    // TODO One-Task authorization process is not ready yet
                    while (AuthorizationResult is null)
                    {
                        if (!authorization.IsPendingAuthorization) break;
                        await Task.Delay(500);
                    }
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (AuthorizationResult.HasValue && AuthorizationResult.Value)
                        {
                            ProgressData = "Authorization succeful!";
                            ProgressText.Foreground = new SolidColorBrush(Colors.LightGreen);
                            MainViewModel.AuthorizedVisibility = Visibility.Visible;
                            MainViewModel.UnAuthorizedVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ProgressData = "Authorization failed!";
                            ProgressText.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        ProgressText.Text = ProgressData;
                    }, System.Windows.Threading.DispatcherPriority.Render);
                    await Task.Delay(2000);
                    ProgressData = "Preparing app for you";
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ProgressText.Text = ProgressData;
                        ProgressText.Foreground = new SolidColorBrush(Colors.White);
                    });
                    AuthorizationResult = null;
                    if (AuthorizationResult.HasValue && AuthorizationResult.Value)
                    {
                        await Task.Delay(4000);
                    }
                }
                else
                {
                    ProgressData = "Preparing app for you";
                }
                await Task.Delay(2000);
                isPreparing = false;
                await Dispatcher.InvokeAsync(() => ProgressText.Text = "App is ready!",
                    System.Windows.Threading.DispatcherPriority.Render);
                await Task.Delay(1000);
                await Dispatcher.InvokeAsync(() =>
                {
                    Width = 1280;
                    Height = 720;
                    if (Settings.Default.IsWindowMaximized)
                    {
                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        Settings.Default.WindowLocation = RestoreBounds.Location;
                        Settings.Default.WindowSize = RestoreBounds.Size;
                    }
                    ResizeMode = ResizeMode.CanResize;
                    NavigationView.Visibility = Visibility.Visible;
                    //TitleBarButton.Visibility = Visibility.Visible;
                    LoadingScreen.Visibility = Visibility.Collapsed;
                    ProgressRing.IsActive = false;
                    //Navigate(typeof(Pages.Dashboard));
                }, System.Windows.Threading.DispatcherPriority.Render);
                ProgressData = null;
                //MainViewModel.AuthorizedVisibility = Visibility.Visible;
                //MainViewModel.UnAuthorizedVisibility = Visibility.Collapsed;
            });
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.WindowLocation = RestoreBounds.Location;
            Settings.Default.WindowSize = RestoreBounds.Size;
            Settings.Default.IsWindowMaximized = WindowState == WindowState.Maximized;
            App.Current.Shutdown();
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem tab) return;
            var tag = tab.Tag;
            if (tag is null) return;
            if (tag is Type viewType)
            {
                NavigationService.Navigate(viewType);
            }
            if (tag is string command)
            {
                if (Uri.IsWellFormedUriString(command, UriKind.Absolute))
                {
                    NavigationService.Navigate(new Uri(command));
                    return;
                }
                switch (command)
                {
                    default:
                        break;
                }
            }
        }

        private void NavigationView_BackRequested(ModernWpf.Controls.NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (NavigationService.CanGoBack()) NavigationService.GoBack();
        }
    }
}
