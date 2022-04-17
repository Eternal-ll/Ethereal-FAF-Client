using beta.Infrastructure.Services.Interfaces;
using DevExpress.Data.Utils;

namespace beta.ViewModels
{
    internal class PlayersViewModel : Base.ViewModel
    {
        private readonly IPlayersService PlayersService;
        private readonly IGamesService GamesService;
        public PlayersViewModel()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
        }
    }
}
