using beta.Infrastructure.Services;
using beta.ViewModels.Base;
using ModernWpf.Controls;
using System.Windows;

namespace beta.ViewModels
{
    /// <summary>
    /// Main view model for <see cref="Views.Windows.MainWindow"/>
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private readonly NavigationService NavigationService;

        public IrcViewModel IrcViewModel { get; private set; }

        public MainViewModel(IrcViewModel ircViewModel, NavigationService navigationService)
        {
            IrcViewModel = ircViewModel;
            NavigationService = navigationService;
        }

        #region AuthorizedVisibility
        private Visibility _AuthorizedVisibility = Visibility.Collapsed;
        public Visibility AuthorizedVisibility
        {
            get => _AuthorizedVisibility;
            set => Set(ref _AuthorizedVisibility, value);
        }
        #endregion

        #region UnAuthorizedVisibility
        public Visibility _UnAuthorizedVisibility = Visibility.Visible;
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
