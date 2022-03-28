using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace beta.ViewModels
{
    /// <summary>
    /// Main view model that controls overall work and stability of application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private readonly IOAuthService OAuthService;

        public MainViewModel()
        {
            OAuthService = App.Services.GetService<IOAuthService>();

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            if (Settings.Default.AutoJoin)
            {
                //Task.Run(async () => await OAuthService.AuthAsync());
                var model = new SessionAuthorizationViewModel();
                model.Completed += OnSessionAuthorizationCompleted;
                ChildViewModel = model;
            }
            else
            {
                ChildViewModel = new AuthorizationViewModel();
                OAuthService.StateChanged += OnOAuthServiceStateChanged;
            }
        }

        private void OnSessionAuthorizationCompleted(object sender, bool e) => 
            ChildViewModel = e ? new NavigationViewModel() : new AuthorizationViewModel();

        private void OnOAuthServiceStateChanged(object sender, Models.OAuthEventArgs e)
        {
            if (e.State == Models.Enums.OAuthState.PendingAuthorization)
            {
                var model = new SessionAuthorizationViewModel();
                model.Completed += OnSessionAuthorizationCompleted;
            }
        }

        #region ChildViewModel
        private ViewModel _ChildViewModel;
        /// <summary>
        /// Current UI view model
        /// </summary>
        public ViewModel ChildViewModel
        {
            get => _ChildViewModel;
            set => Set(ref _ChildViewModel, value);
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
