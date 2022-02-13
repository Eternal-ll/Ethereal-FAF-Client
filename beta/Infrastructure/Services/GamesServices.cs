using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services
{
    public class GamesServices : ViewModel, IGamesServices
    {
        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;

        #endregion

        #region IdleGames
        private readonly ObservableCollection<GameInfoMessage> _IdleGames = new();
        public ObservableCollection<GameInfoMessage> IdleGames => _IdleGames;
        #endregion

        #region LiveGames
        private readonly ObservableCollection<GameInfoMessage> _LiveGames = new();
        public ObservableCollection<GameInfoMessage> LiveGames => _LiveGames;
        #endregion

        #endregion

        public GamesServices(
            ISessionService sessionService,
            IPlayersService playerService)
        {
            SessionService = sessionService;
            PlayersService = playerService;

            sessionService.NewGame += OnNewGame;
        }

        private void OnNewGame(object sender, EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            var games = _IdleGames;
            // Update in-game players status

            game.Teams = GetInGameTeams(game);

            if (game.Host == null)
                game.Host = PlayersService.GetPlayer(game.host);
            
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].host == game.host)
                {
                    if (game.launched_at != null)
                    {
                        games.RemoveAt(i);
                        _LiveGames.Add(game);
                        return;
                    }

                    if (game.num_players == 0)
                    {
                        games.RemoveAt(i);
                    }
                    else games[i].Update(game);
                    return;
                }
            }

            if (game.launched_at != null)
            {
                var liveGames = _LiveGames;
                liveGames.Add(game);
                for (int i = 0; i < liveGames.Count; i++)
                {
                    if (liveGames[i].host == game.host)
                    {
                        if (game.num_players == 0)
                            liveGames.RemoveAt(i);
                        else liveGames[i] = game;
                        return;
                    }
                }
                return;
            }
            if (game.num_players > 0)
                games.Add(game);
        }

        public InGameTeam[] GetInGameTeams(GameInfoMessage game)
        {
            InGameTeam[] teams = new InGameTeam[game.teams.Count];

            int j = 0;

            // TODO: FIX ME Need to rework player game status

            //var playerStatus = GameState.Open;
            //if (game.launched_at != null)
            //{
            //    var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(game.launched_at.Value);
            //    playerStatus = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
            //}

            foreach (var valuePair in game.teams)
            {
                var players = new IPlayer[valuePair.Value.Length];

                for (int i = 0; i < valuePair.Value.Length; i++)
                {
                    var player = PlayersService.GetPlayer(valuePair.Value[i]);
                    if (player == null)
                    {
                        players[i] = new UnknownPlayer()
                        {
                            login = valuePair.Value[i]
                        };
                        continue;
                    }

                    //player.GameState = playerStatus;
                
                    player.Game = game;

                    players[i] = player;
                }

                teams[j] = new(valuePair.Key, players);
                j++;
            }
            return teams;
        }
    }
}
