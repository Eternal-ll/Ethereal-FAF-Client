using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.OAuth;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    internal class ConnectionViewModel : Base.ViewModel
    {
        public event EventHandler Authorized;

        private readonly ISessionService SessionService;
        private readonly IOAuthService OAuthService;
        private readonly IProgress<string> Progress;

        public ConnectionViewModel()
        {
            SessionService = App.Services.GetService<ISessionService>();
            OAuthService = App.Services.GetService<IOAuthService>();

            Progress = new Progress<string>((data) => ProgressText = data);

            SessionService.AuthentificationFailed += SessionService_AuthentificationFailed;
            SessionService.StateChanged += SessionService_StateChanged;
            SessionService.Authorized += SessionService_Authorized;
            OAuthService.StateChanged += OAuthService_StateChanged;
            OAuthService.SetToken(
                Settings.Default.access_token,
                Settings.Default.refresh_token,
                Settings.Default.id_token,
                Settings.Default.ExpiresIn);
            ConnectCommand.Execute(null);
        }

        private void SessionService_AuthentificationFailed(object sender, Models.Server.AuthentificationFailedData e)
        {
            IsPendingAuthorization = false;
            IsOAuthRequested = true;
            try
            {
                throw new ArgumentException(e.text);
            }
            catch (Exception ex)
            {
                Exception = new(ex);
            }
        }

        #region ProgressText
        private string _ProgressText = string.Empty;
        public string ProgressText
        {
            get => _ProgressText;
            set => Set(ref _ProgressText, value);
        }
        #endregion

        #region Exception
        private ExceptionWrapper _Exception;
        public ExceptionWrapper Exception
        {
            get => _Exception;
            set => Set(ref _Exception, value);
        }
        #endregion

        #region IsOAuthRequested
        private bool _IsOAuthRequested;
        public bool IsOAuthRequested
        {
            get => _IsOAuthRequested;
            set => Set(ref _IsOAuthRequested, value);
        }
        #endregion

        #region IsPendingAuthorization
        private bool _IsPendingAuthorization;
        public bool IsPendingAuthorization
        {
            get => _IsPendingAuthorization;
            set
            {
                if (Set(ref _IsPendingAuthorization, value))
                {
                    ProgressText = string.Empty;
                    OnPropertyChanged(nameof(IsInputEnabled));
                    if (value)
                    {
                        Exception = null;
                        new Thread(() =>
                        {
                            while (IsPendingAuthorization)
                            {
                                var state = ProgressText;
                                var data = state.Split('.');

                                string points = string.Empty;
                                switch (data.Length)
                                {
                                    case 1:
                                        points = ".";
                                        break;
                                    case 2:
                                        points = "..";
                                        break;
                                    case 3:
                                        points = "...";
                                        break;
                                }
                                ProgressText = data[0] + points;
                                Thread.Sleep(500);
                            }
                        })
                        {
                            IsBackground = true
                        }.Start();
                    }
                }
            }
        }
        #endregion
        public bool IsInputEnabled => !IsPendingAuthorization;

        public ICommand LogoutCommand => (ICommand)App.Current.Resources["LogoutCommand"];
        public ICommand RestartCommand => (ICommand)App.Current.Resources["RestartCommand"];

        #region ConnectCommand
        private ICommand _ConnectCommand;
        public ICommand ConnectCommand => _ConnectCommand ??= new LambdaCommand(OnConnectCommand, CanConnectCommand);
        private bool CanConnectCommand(object parameter) => IsInputEnabled;
        private async void OnConnectCommand(object parameter)
        {
            if (IsOAuthRequested) await OAuthService.AuthAsync(Progress)
            .ContinueWith(task => HandleOAuthResultTask(task));
            else await SessionService.AuthorizeAsync(Settings.Default.access_token, new())
                    .ContinueWith(task => HandleSessionResultTask(task));
        }
        #endregion

        private async Task HandleOAuthResultTask(Task<TokenBearer> task)
        {
            if (task.IsFaulted)
            {
                Exception = new(task.Exception);
                IsPendingAuthorization = false;
            }
            else if ((task.IsCompleted || task.IsCompletedSuccessfully) && task.Result is not null)
            {
                Settings.Default.access_token = task.Result.AccessToken;
                Settings.Default.refresh_token = task.Result.RefreshToken;
                Settings.Default.id_token = task.Result.IdToken;
                Settings.Default.ExpiresIn = task.Result.ExpiresIn;
                Settings.Default.ExpiresAt = task.Result.ExpiresAt;
                await SessionService.AuthorizeAsync(task.Result.AccessToken, new())
                    .ContinueWith(task => HandleSessionResultTask(task));
            }
            else IsPendingAuthorization = false;
        }
        private void HandleSessionResultTask(Task task)
        {
            if (task.IsFaulted)
            {
                Exception = new(task.Exception);
            }
        }

        private void SessionService_Authorized(object sender, bool e)
        {
            if (e)
            {
                Authorized?.Invoke(this, null);
            }
            else
            {
                IsPendingAuthorization = false;
            }
        }

        private void OAuthService_StateChanged(object sender, Models.OAuthEventArgs e)
        {
            IsPendingAuthorization = e.State == Models.Enums.OAuthState.PendingAuthorization;
        }

        private void SessionService_StateChanged(object sender, SessionState e)
        {
            if (e == SessionState.AuthentificationFailed)
            {

            }
            else if(e == SessionState.PendingConnection)
            {
                IsPendingAuthorization = true;
            }
        }
    }
}
