using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Meziantou.Framework.WPF.Collections;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class LobbyGamesViewModel : Base.ViewModel
    {
        public ConcurrentObservableCollection<Game> Games { get; private set; }
        public LobbyGamesViewModel(IFafGamesService fafGamesService, IFafGamesEventsService fafGamesEventsService)
        {
            var games = fafGamesService.GetGames();
            Games = new();
            Games.AddRange(games);           

            fafGamesEventsService.GameAdded += FafGamesEventsService_GameAdded;
            fafGamesEventsService.GameRemoved += FafGamesEventsService_GameRemoved;
        }
        private void FafGamesEventsService_GameRemoved(object sender, Game e) => Games.Remove(e);
        private void FafGamesEventsService_GameAdded(object sender, Game e) => Games.Add(e);
    }
}