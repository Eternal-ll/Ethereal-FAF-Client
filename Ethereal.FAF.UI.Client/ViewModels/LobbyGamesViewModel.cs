using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Meziantou.Framework.WPF.Collections;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LobbyGamesViewModel : Base.ViewModel
    {
        private readonly IFafGamesEventsService _fafGamesEventsService;
        public ConcurrentObservableCollection<Game> Games { get; private set; } = new();
        public LobbyGamesViewModel(IFafGamesEventsService fafGamesEventsService)
        {
            _fafGamesEventsService = fafGamesEventsService;
            _fafGamesEventsService.GameAdded += FafGamesEventsService_GameAdded;
            _fafGamesEventsService.GameRemoved += FafGamesEventsService_GameRemoved;
            _fafGamesEventsService.GameUpdated += _fafGamesEventsService_GameUpdated;
        }

        private void _fafGamesEventsService_GameUpdated(object sender, (Game Cached, Game Incoming) e)
        {
            var index = Games.IndexOf(e.Cached);
            if (index > 0)
            {
                Games[index] = e.Cached;
            }
        }

        private void FafGamesEventsService_GameRemoved(object sender, Game e)
        {
            Games.Remove(e);
        }
        private void FafGamesEventsService_GameAdded(object sender, Game e)
        {
            Games.Add(e);
        }
    }
}