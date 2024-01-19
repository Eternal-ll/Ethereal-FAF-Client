using Ethereal.FAF.UI.Client.ViewModels;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafPlayersService
    {
        public Player[] GetPlayers();
        public bool TryGetPlayer(string login, out Player player);
        public bool TryGetPlayer(long id, out Player player);
    }
}
