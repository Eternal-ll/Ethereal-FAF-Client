using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels
{
    public class PlayModeSelectVM : ViewModel
    {
        private readonly ISelfService SelfService;

        public PlayModeSelectVM(ISelfService selfService, CustomGamesViewModel openGamesViewModel, MatchMakerViewModel matchMakerViewModel)
        {
            SelfService = selfService;
            OpenGamesViewModel = openGamesViewModel;
            MatchMakerViewModel = matchMakerViewModel;
        }

        public PlayerInfoMessage Self => App.Services.GetService<IPlayersService>().Self;
        public CustomGamesViewModel OpenGamesViewModel { get; set; }
        public MatchMakerViewModel MatchMakerViewModel { get; set; }
    }
}
