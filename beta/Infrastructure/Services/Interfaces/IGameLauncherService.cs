using beta.ViewModels;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGameLauncherService
    {
        public event EventHandler<EventArgs<TestDownloaderModel>> PatchUpdateRequired;

        public void JoinGame();
        public void JoinGame(GameVM game);
    }
}
