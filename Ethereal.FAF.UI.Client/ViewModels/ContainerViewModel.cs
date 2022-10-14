using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using System.Windows;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ContainerViewModel : Base.ViewModel
    {
        public LobbyClient LobbyViewModel { get; }
        public BackgroundViewModel BackgroundViewModel { get; }

        private readonly GameLauncher GameLauncher;

        public ContainerViewModel(LobbyClient lobbyViewModel, GameLauncher gameLauncher, BackgroundViewModel backgroundViewModel)
        {
            gameLauncher.StateChanged += GameLauncher_StateChanged;
            gameLauncher.GameLaunching += GameLauncher_GameLaunching;

            LobbyViewModel = lobbyViewModel;
            GameLauncher = gameLauncher;
            BackgroundViewModel = backgroundViewModel;
        }

        private void GameLauncher_GameLaunching(object sender, System.Progress<string> e)
        {
            return;
            SplashProgressVisibility = Visibility.Visible;
            SplashVisibility = Visibility.Visible;
            e.ProgressChanged += (s, e) => SplashText = e;
        }

        private void GameLauncher_StateChanged(object sender, GameLauncherState e)
        {
            return;
            if (e is GameLauncherState.Running)
            {
                SplashProgressVisibility = Visibility.Collapsed;
                SplashVisibility = Visibility.Visible;
            }
            if (e is GameLauncherState.Idle)
            {
                SplashVisibility = Visibility.Collapsed;
            }
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
        private Visibility _SplashVisibility = Visibility.Visible;
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
            set
            {
                if (!Set(ref _SplashText, value))
                {
                    _SplashText = value;
                    OnPropertyChanged(nameof(SplashText));
                }
            }
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
