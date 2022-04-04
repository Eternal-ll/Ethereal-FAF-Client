using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
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

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

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
                Thread.Sleep(500);
                ChildViewModel = new AuthorizationViewModel();
                Thread.Sleep(5000);
                ChildViewModel = new SessionAuthorizationViewModel();
                Thread.Sleep(5000);
                ChildViewModel = new SessionAuthorizationViewModel();
            });
        }

        private void OnSessionAuthorizationCompleted(object sender, bool e) => 
            ChildViewModel = e ? new NavigationViewModel() : new AuthorizationViewModel();

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

        #region Width
        private double _Width;
        public double Width { get => _Width; set => Set(ref _Width, value); }
        #endregion

        #region Height
        private double _Height;
        public double Height { get => _Height; set => Set(ref _Height, value); }
        #endregion

        #region Left
        private double _Left;
        public double Left { get => _Left; set => Set(ref _Left, value); }
        #endregion


        #region Top
        private double _Top;
        public double Top { get => _Top; set => Set(ref _Top, value); }
        #endregion

        #endregion
    }
}
