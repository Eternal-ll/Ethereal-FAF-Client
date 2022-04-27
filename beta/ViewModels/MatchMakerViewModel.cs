using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels.Base;

namespace beta.ViewModels
{
    internal class MatchMakerViewModel : ViewModel
    {
        private readonly ISessionService SessionService;
        public MatchMakerViewModel()
        {

        }

        #region GamesViewModel
        private MatchMakerGamesViewModel _GamesViewModel;
        public MatchMakerGamesViewModel GamesViewModel
        {
            get => _GamesViewModel;
            set => Set(ref _GamesViewModel, value);
        }
        #endregion
    }
}
