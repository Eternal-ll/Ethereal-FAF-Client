using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    public class SessionAuthorizationViewModel : Base.ViewModel
    {
        public event EventHandler<bool> Completed;

        private IOAuthService OAuthService;
        private ISessionService SessionService;

        public SessionAuthorizationViewModel()
        {
            return;
            OAuthService = App.Services.GetService<IOAuthService>();
            SessionService = App.Services.GetService<ISessionService>();

            OAuthService.StateChanged += OnStateChanged;

            CurrentState = "Pending authorization";

            Task.Run(() =>
            {
                while (true)
                {
                    var state = CurrentState;
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
                    CurrentState = data[0] + points;
                    Thread.Sleep(500);
                }
            });
        }

        #region Progress
        private int _Progress;
        public int Progress
        {
            get => _Progress;
            private set
            {
                if (Set(ref _Progress, value))
                {

                }
            }
        }

        #endregion

        #region CurrentState
        private string _CurrentState;
        public string CurrentState
        {
            get => _CurrentState;
            private set
            {
                if (Set(ref _CurrentState, value))
                {

                }
            }
        }
        #endregion

        private void OnStateChanged(object sender, Models.OAuthEventArgs e)
        {
            switch (e.State)
            {
                case Models.Enums.OAuthState.NO_CONNECTION:
                    break;
                case Models.Enums.OAuthState.NO_TOKEN:
                    break;
                case Models.Enums.OAuthState.INVALID:
                    break;
                case Models.Enums.OAuthState.TIMED_OUT:
                    break;
                case Models.Enums.OAuthState.AUTHORIZED:
                    CurrentState = "Authorized";
                    break;
                default:
                    break;
            }
        }
    }
}
