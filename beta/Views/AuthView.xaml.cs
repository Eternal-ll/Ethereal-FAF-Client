using beta.Infrastructure.Navigation;
using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : INavigationAware
    {
        private readonly IOAuthService OAuthService;
        private readonly ISessionService SessionService;
        private INavigationManager NavigationManager;

        /// <summary>
        /// Raising event on navigating, give us a NavigationManager to move to next view
        /// </summary>
        /// <param name="navigationManager"></param>
        /// <returns></returns>
        void INavigationAware.OnViewChanged(INavigationManager navigationManager)
        {
            NavigationManager = navigationManager;

            // getting user setting for auto join
            if (Settings.Default.AutoJoin)
            {
                return;
                // looping view controls and hide them
                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Collapsed;
                
                ProgressRing.Visibility = Visibility.Visible;

                // launch OAuth2 authorization in service
                OAuthService.Auth();
            }
        }

        public AuthView()
        {
            InitializeComponent();

            // loading required services
            OAuthService = App.Services.GetRequiredService<IOAuthService>();
            SessionService = App.Services.GetRequiredService<ISessionService>();

            // starting listening events

            // raising up after finishing OAuth2 authorization
            OAuthService.Result += OnOAuthAuthorizationFinish;
            
            // raising up after finishing lobby session authorization
            SessionService.Authorized += OnLobbySessionServiceAuthorizationFinish;

            // load user setting of auto join ot local checkbox
            AutoJoinCheckBox.IsChecked = Settings.Default.AutoJoin;
        }
            
        private void OnLobbySessionServiceAuthorizationFinish(object sender, Infrastructure.EventArgs<bool> e) =>
            Dispatcher.Invoke(() =>
            {
                // release used resources
                Canvas.Children.Clear();
                Canvas = null;

                // remove listeners of events
                OAuthService.Result -= OnOAuthAuthorizationFinish;
                SessionService.Authorized -= OnLobbySessionServiceAuthorizationFinish;
                // call UI thread and invoke our instructions
                // events coming from different thread and can raise exception
                NavigationManager.Navigate(new MainView());
                return;
            });

        private void OnCheckBoxClick(object sender, RoutedEventArgs e) => Settings.Default.AutoJoin = ((CheckBox)sender).IsChecked.Value;

        private void OnAuthorizationButtonClick(object sender, RoutedEventArgs e)
        {
            var login = LoginInput.Text;
            var password = PasswordInput.Password;

            for (int i = 0; i < Canvas.Children.Count; i++)
                Canvas.Children[i].Visibility = Visibility.Collapsed;

            ProgressRing.Visibility = Visibility.Visible;

            new Thread(()=> OAuthService.Auth(login, password)).Start();
        }

        private readonly ContentDialog WarnDialog = new()
        {
            Title = "Warning",
            CloseButtonText = "Ok"
        };

        private void OnOAuthAuthorizationFinish(object sender, Infrastructure.EventArgs<OAuthStates> e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.Arg)
                {
                    case OAuthStates.AUTHORIZED:
                        for (int i = 0; i < Canvas.Children.Count; i++)
                            Canvas.Children[i].Visibility = Visibility.Collapsed;
                        ProgressRing.Visibility = Visibility.Visible;
                        return;
                    case OAuthStates.INVALID:
                        WarnDialog.Content = "Invalid authorization parameters";
                        break;
                    case OAuthStates.NO_CONNECTION:
                        WarnDialog.Content = "No connection.\nPlease check your internet connection";
                        break;
                    case OAuthStates.NO_TOKEN:
                        WarnDialog.Content = "Something went wrong on auto-join.\nPlease use authorization form again";
                        break;
                    case OAuthStates.TIMED_OUT:
                        WarnDialog.Content = "Server is not responses";
                        break;
                    case OAuthStates.EMPTY_FIELDS:
                        WarnDialog.Content = "Empty fields";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Visible;

                ProgressRing.Visibility = Visibility.Collapsed;

                WarnDialog.ShowAsync();
            });
        }
    }
}
