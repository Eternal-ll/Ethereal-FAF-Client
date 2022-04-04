using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows;

namespace beta.ViewModels
{
    /// <summary>
    /// Main view model that controls overall work and stability of application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private readonly IOAuthService OAuthService;
        private readonly ISessionService SessionService;

        public MainViewModel()
        {
            OAuthService = App.Services.GetService<IOAuthService>();
            SessionService = App.Services.GetService<ISessionService>();

            var isAutojoin = Settings.Default.AutoJoin;

            isAutojoin = false;

            SessionService.Authorized += OnSessionAuthorizationCompleted;

            //if (isAutojoin)
            //{
            //    //Task.Run(async () => await OAuthService.AuthAsync());
            //    var model = new SessionAuthorizationViewModel();
            //    model.Completed += OnSessionAuthorizationCompleted;
            //    ChildViewModel = model;
            //}
            //else
            //{
            //OAuthService.StateChanged += OnOAuthServiceStateChanged;
            //}

            Task.Run(() =>
            {
                ChildViewModel = new AuthorizationViewModel();
                //ChildViewModel = new NavigationViewModel();
            });
        }

        private void OnSessionAuthorizationCompleted(object sender, bool e)
        {
            if (e)
            {
                ChildViewModel = new NavigationViewModel();
                SessionService.Authorized -= OnSessionAuthorizationCompleted;
            }
        }

        #region ChildViewModel
        private ViewModel _ChildViewModel = new PlugViewModel();
        /// <summary>
        /// Current UI view model
        /// </summary>
        public ViewModel ChildViewModel
        {
            get => _ChildViewModel;
            set
            {
                if (_ChildViewModel is not null)
                {
                    _ChildViewModel.Dispose();
                }
                if (Set(ref _ChildViewModel, value))
                {

                }
            }
        }
        #endregion

        #region Window Properties

        #region TitleBarVisibility
        private Visibility _TitleBarVisibility = Visibility.Collapsed;
        public Visibility TitleBarVisibility
        {
            get => _TitleBarVisibility;
            set => Set(ref _TitleBarVisibility, value);
        }
        #endregion

        #endregion
    }
}
