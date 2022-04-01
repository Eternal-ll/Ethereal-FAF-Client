using beta.Infrastructure.Navigation;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
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
                // looping view controls and hide them
                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Collapsed;
                
                ProgressRing.Visibility = Visibility.Visible;

                // launch OAuth2 authorization in service
                new Thread(() => OAuthService.AuthAsync())
                {
                    Name = "OAuthorization thread",
                    IsBackground = true
                }.Start();
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
            OAuthService.StateChanged += OnOAuthAuthorizationFinish;
            
            // raising up after finishing lobby session authorization
            SessionService.Authorized += OnLobbySessionServiceAuthorizationFinish;

            // load user setting of auto join ot local checkbox
            AutoJoinCheckBox.IsChecked = Settings.Default.AutoJoin;
        }
            
        private void OnLobbySessionServiceAuthorizationFinish(object sender, bool e) =>
            Dispatcher.Invoke(() =>
            {
                if (e)
                {
                    // release used resources
                    Canvas.Children.Clear();
                    Canvas = null;

                    // remove listeners of events
                    OAuthService.StateChanged -= OnOAuthAuthorizationFinish;
                    SessionService.Authorized -= OnLobbySessionServiceAuthorizationFinish;
                    // call UI thread and invoke our instructions
                    // events coming from different thread and can raise exception
                    NavigationManager.Navigate(new NavigationView());
                }
                else
                {
                    for (int i = 0; i < Canvas.Children.Count; i++)
                        Canvas.Children[i].Visibility = Visibility.Visible;

                    ProgressRing.Visibility = Visibility.Collapsed;

                    WarnDialog.Content = "Cant connect to FAF lobby server";
                    WarnDialog.ShowAsync();
                }
            });

        private void OnCheckBoxClick(object sender, RoutedEventArgs e) => Settings.Default.AutoJoin = ((CheckBox)sender).IsChecked.Value;

        /// <summary>
        /// Запускаем авторизацию в нвоом потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAuthorizationButtonClick(object sender, RoutedEventArgs e)
        {
            // здесь все очень костыльно, нужно было все выносить в сам сервис,
            // поднимать события авторизации и уведомлять оттуда...

            // получаем введенные данные
            var login = LoginInput.Text;
            var password = PasswordInput.Password;

            // скрываем весь UI интерфейс, опять же можно было сделать по событиям (если правильно)
            for (int i = 0; i < Canvas.Children.Count; i++)
                Canvas.Children[i].Visibility = Visibility.Collapsed;
            
            // делаем видимым кольцо загрузки, уведомляем пользователя о начале авторизации
            ProgressRing.Visibility = Visibility.Visible;

            // создаем новый поток и запускаем метод авторизации с передачей нужных параметров
            new Thread(()=> OAuthService.AuthAsync(login, password))
            {
                Name = "OAuthorization thread",
                IsBackground=true
            }.Start();
        }

        private readonly ContentDialog WarnDialog = new()
        {
            Title = "Warning",
            CloseButtonText = "Ok"
        };

        private void OnOAuthAuthorizationFinish(object sender, OAuthEventArgs e)
        {
            if (e.State == OAuthState.AUTHORIZED) return;
            Dispatcher.Invoke(() =>
            {
                WarnDialog.Content = e.Message;

                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Visible;

                ProgressRing.Visibility = Visibility.Collapsed;
                
                WarnDialog.ShowAsync();
            });
        }
    }
}
