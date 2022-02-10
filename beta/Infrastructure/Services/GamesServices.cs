using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using System;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services
{
    public class GamesServices : ViewModel, IGamesServices
    {
        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayerService;

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
            PlayerService = playerService;

            sessionService.NewGame += OnNewGame;
        }

        private void OnNewGame(object sender, EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            var games = _IdleGames;

            // Update in-game players status

            foreach (var key in game.teams.Keys)
            {
                for (int i = 0; i < game.teams[key].Length; i++)
                {
                    var nick = game.teams[key][i];

                    var player = PlayerService.GetPlayer(nick);

                    if (player != null)
                    {
                        // TODO FIX ME: rewrite

                        if (game.launched_at == null)
                        {
                            if (player.login == game.host)
                            {
                                player.GameState = game.password_protected ? GameState.PrivateHost : GameState.Host;
                                continue;
                            }
                            player.GameState = game.password_protected ? GameState.PrivateOpen : GameState.Open;
                        }
                        else
                        {
                            var time = DateTime.UnixEpoch.AddSeconds(game.launched_at.Value);
                            var difference = DateTime.UtcNow - time;
                            if (difference.TotalSeconds < 300)
                                player.GameState = game.password_protected ? GameState.PrivatePlaying5 : GameState.Playing5;
                            else player.GameState = game.password_protected ? GameState.PrivatePlaying : GameState.Playing;
                        }
                    }
                }
            }

            for (int i = 0; i < games.Count; i++)
            {
                var idleGame = games[i];
                if (game.uid == idleGame.uid)
                {
                    // TODO: Update stats manually
                    games[i] = game;
                }
            }

            //
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].uid == game.uid)
                {
                    if (game.launched_at != null)
                    {
                        _IdleGames.RemoveAt(i);
                        _LiveGames.Add(game);
                        return;
                    }
                    games[i] = game;
                    return;
                }
            }

            if (game.launched_at != null)
            {
                _LiveGames.Add(game);
                return;
            }

            _IdleGames.Add(game);
        }
    }
}
