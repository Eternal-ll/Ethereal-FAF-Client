using Ethereal.FAF.UI.Client.Models.Lobby;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IGameLauncher
    {
        public Task JoinGameAsync(Game game);
    }
}
