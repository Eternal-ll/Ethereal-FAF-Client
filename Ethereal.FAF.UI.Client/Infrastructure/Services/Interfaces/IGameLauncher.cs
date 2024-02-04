using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public enum GameLauncherState
    {
        Idle,
        Joining,
        Running,
    }
    public interface IGameLauncher
    {
        public event EventHandler<GameLauncherState> OnState;
        public GameLauncherState State { get; }
        public Task JoinGameAsync(
            Game game,
            string password = null,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
    }
}
