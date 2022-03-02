using beta.Models.Server;
using beta.ViewModels;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGameLauncherService
    {
        public event EventHandler<EventArgs<TestDownloaderModel>> PatchUpdateRequired;

        public void JoinGame();
        public void JoinGame(GameInfoMessage game);
    }
}
