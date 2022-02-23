using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels.Base;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using beta.Properties;

namespace beta.ViewModels
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public  class AuthViewModel : ViewModel
    {
        private readonly IOAuthService OAuthService;

        public AuthViewModel(IOAuthService oAuthService)
        {
            OAuthService = oAuthService;

            OAuthService.Result += OnOAuthService_Result;
            ReloadTokenTimer();
            //AutoJoinCheckBox.IsChecked = Properties.Settings.Default.AutoJoin;
            OAuthService.RefreshOAuthToken(Settings.Default.refresh_token);
        }

        private Thread TokenTimerThread;
        private TimeSpan span;
        void ReloadTokenTimer()
        {
            if (string.IsNullOrEmpty(Settings.Default.access_token))
            {
                //PingLabel.Content = "No token";
                return;
            }
            span = Settings.Default.expires_in - DateTime.Now;
            span = new TimeSpan(0, 0, 0, Convert.ToInt32(Math.Round(span.TotalSeconds)));
            if (TokenTimerThread == null)
            {
                TokenTimerThread = new Thread(async () =>
                {
                    while (true)
                    {
                        while (span.TotalSeconds > 0)
                        {
                            //await Dispatcher.InvokeAsync(() => { PingLabel.Content = "Token expires in "+ span.TotalSeconds + " seconds"; });
                            span = new TimeSpan(0, 0, 0, Convert.ToInt32(span.TotalSeconds - 1));
                            Thread.Sleep(1000);
                        }
                        //await Dispatcher.InvokeAsync(() => { PingLabel.Content = "Token expired"; });
                    }
                });
                TokenTimerThread.Start();
            }
        }

        private void OnOAuthService_Result(object sender, Infrastructure.EventArgs<OAuthState> e)
        {
            if (e.Arg == OAuthState.AUTHORIZED)
            {
                ReloadTokenTimer();
            }
            MessageBox.Show(e.Arg.ToString());
        }

        private async void OnAuthorization(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            //await OAuthService.Auth(LoginInput.Text, PasswordInput.Text);
            ((Button)sender).IsEnabled = true;
        }
        private void OnCheckBoxClick(object sender, RoutedEventArgs e) => Settings.Default.AutoJoin = ((CheckBox)sender).IsChecked!.Value;
    }
}
