using beta.Models.Server;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGameLauncherService
    {
        public event EventHandler PatchUpdateRequired;
        public Task JoinGame(GameInfoMessage game);
    }
}
