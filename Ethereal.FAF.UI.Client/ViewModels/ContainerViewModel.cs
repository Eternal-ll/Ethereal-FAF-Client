using Ethereal.FAF.UI.Client.Infrastructure.Lobby;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ContainerViewModel : Base.ViewModel
    {
        public LobbyClient LobbyViewModel { get; }
        public BackgroundViewModel BackgroundViewModel { get; }

        private readonly GameLauncher GameLauncher;

        public ContainerViewModel(LobbyClient lobbyViewModel, GameLauncher gameLauncher, BackgroundViewModel backgroundViewModel)
        {
            LobbyViewModel = lobbyViewModel;
            GameLauncher = gameLauncher;
            BackgroundViewModel = backgroundViewModel;
        }
    }
}
