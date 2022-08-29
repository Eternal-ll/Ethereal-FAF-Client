using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using System.Windows;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ContainerViewModel : Base.ViewModel
    {
        public LobbyClient LobbyViewModel { get; }

        public ContainerViewModel(LobbyClient lobbyViewModel)
        {
            LobbyViewModel = lobbyViewModel;
        }
        public Visibility MainVisibility => SplashVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        #region SplashProgressVisibility
        private Visibility _SplashProgressVisibility = Visibility.Visible;
        public Visibility SplashProgressVisibility
        {
            get => _SplashProgressVisibility;
            set => Set(ref _SplashProgressVisibility, value);
        }
        #endregion
        #region SplashVisibility
        private Visibility _SplashVisibility;
        public Visibility SplashVisibility
        {
            get => _SplashVisibility;
            set
            {
                if (Set(ref _SplashVisibility, value))
                {
                    OnPropertyChanged(nameof(MainVisibility));
                }
            }
        }
        #endregion
        #region SplashText
        private string _SplashText;
        public string SplashText
        {
            get => _SplashText;
            set => Set(ref _SplashText, value);
        }
        #endregion
        #region Content
        private object _Content;
        public object Content
        {
            get => _Content;
            set => Set(ref _Content, value);
        }
        #endregion
    }
}
