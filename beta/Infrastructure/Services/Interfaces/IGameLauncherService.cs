using beta.Models.Server;
using beta.ViewModels;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGameLauncherService
    {
        public event EventHandler<EventArgs<TestDownloaderVM>> PatchUpdateRequired;

        public void JoinGame();
        public Task JoinGame(GameInfoMessage game);
    }
}
