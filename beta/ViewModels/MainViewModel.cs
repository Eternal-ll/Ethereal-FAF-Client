using beta.Infrastructure.Services;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.Windows;

namespace beta.ViewModels
{
    /// <summary>
    /// Main view model for <see cref="Views.Windows.MainWindow"/>
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private readonly NavigationService NavigationService;
        private readonly IServiceProvider ServiceProvider;
        private readonly ISessionService SessionService;
        private readonly IOAuthService OAuthService;
        private readonly IPlayersService PlayersService;

        public IrcViewModel IrcViewModel { get; private set; }

        public MainViewModel(IrcViewModel ircViewModel, NavigationService navigationService, ISessionService sessionService, IOAuthService oAuthService, IPlayersService playersService, IServiceProvider serviceProvider)
        {
            IrcViewModel = ircViewModel;
            NavigationService = navigationService;
            SessionService = sessionService;
            sessionService.Authorized += SessionService_Authorized;
            OAuthService = oAuthService;
            OAuthService.StateChanged += OAuthService_StateChanged;
            PlayersService = playersService;
            playersService.SelfReceived += PlayersService_SelfReceived;
            ServiceProvider = serviceProvider;
        }

        private void PlayersService_SelfReceived(object sender, PlayerInfoMessage e)
        {
            Me = e;
        }

        private void OAuthService_StateChanged(object sender, Models.OAuthEventArgs e)
        {
            if (e.State is Models.Enums.OAuthState.PendingAuthorization)
            {
                LoaderVisibility = Visibility.Visible;
            }
        }
        private void InitializeViewModels() => App
            .Current.Dispatcher.Invoke(() =>
            {
                var serviceProvider = ServiceProvider;
                serviceProvider.GetService<CustomGamesViewModel>();
                serviceProvider.GetService<CustomLiveGamesViewModel>();
                serviceProvider.GetService<MatchMakerGamesViewModel>();
            }, System.Windows.Threading.DispatcherPriority.Background);

        private void SessionService_Authorized(object sender, bool e)
        {
            if (e)
            {
                LoaderVisibility = Visibility.Collapsed;
                UnAuthorizedVisibility = Visibility.Collapsed;
                AuthorizedVisibility = Visibility.Visible;
                InitializeViewModels();
                return;
            }
            UnAuthorizedVisibility = Visibility.Visible;
            AuthorizedVisibility = Visibility.Collapsed;
        }

        #region Me
        private PlayerInfoMessage _Me;
        public PlayerInfoMessage Me
        {
            get => _Me;
            set => Set(ref _Me, value);
        }
        #endregion

        #region LoaderVisibility
        private Visibility _LoaderVisibility = Visibility.Hidden;
        public Visibility LoaderVisibility
        {
            get => _LoaderVisibility;
            set => Set(ref _LoaderVisibility, value);
        }
        #endregion

        #region AuthorizedVisibility
        private Visibility _AuthorizedVisibility = Visibility.Collapsed;
        public Visibility AuthorizedVisibility
        {
            get => _AuthorizedVisibility;
            set => Set(ref _AuthorizedVisibility, value);
        }
        #endregion

        #region UnAuthorizedVisibility
        public Visibility _UnAuthorizedVisibility;
        public Visibility UnAuthorizedVisibility
        {
            get => _UnAuthorizedVisibility;
            set => Set(ref _UnAuthorizedVisibility, value);
        }
        #endregion

        #region NavigationViewPaneDisplayMode
        private NavigationViewPaneDisplayMode _NavigationViewPaneDisplayMode = Properties.Settings.Default.NavigationViewPaneDisplayMode;
        public NavigationViewPaneDisplayMode NavigationViewPaneDisplayMode
        {
            get => _NavigationViewPaneDisplayMode;
            set => Set(ref _NavigationViewPaneDisplayMode, value);
        }
        #endregion

        #region IsBackButtonVisible
        private NavigationViewBackButtonVisible _IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        public NavigationViewBackButtonVisible IsBackButtonVisible
        {
            get => _IsBackButtonVisible;
            set => Set(ref _IsBackButtonVisible, value);
        }
        #endregion

        #region IsBackButtonEnabled
        private bool _IsBackButtonEnabled;
        public bool IsBackButtonEnabled
        {
            get => _IsBackButtonEnabled;
            set => Set(ref _IsBackButtonEnabled, value);
        }
        #endregion
    }
}
