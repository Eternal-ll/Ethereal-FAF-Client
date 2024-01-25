using Ethereal.FAF.UI.Client.Models.Lobby;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafGamesService
    {
        public Game[] GetGames();
        public Game GetGame(long id);
    }
}
