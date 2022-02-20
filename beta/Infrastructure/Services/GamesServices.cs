using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services
{
    public class GamesServices : ViewModel, IGamesServices
    {
        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;
        private readonly IMapService MapService;

        #endregion

        #region IdleGames
        private readonly ObservableCollection<GameInfoMessage> _IdleGames = new();
        public ObservableCollection<GameInfoMessage> IdleGames => _IdleGames;
        #endregion

        #region LiveGames
        private readonly ObservableCollection<GameInfoMessage> _LiveGames = new();
        public ObservableCollection<GameInfoMessage> LiveGames => _LiveGames;
        #endregion

        /// <summary>
        /// Idle games without players. Bugged or just created
        /// </summary>
        private readonly List<GameInfoMessage> SuspiciousGames = new();

        #endregion

        public GamesServices(
            ISessionService sessionService,
            IPlayersService playerService,
            IMapService mapService)
        {
            SessionService = sessionService;
            PlayersService = playerService;
            MapService = mapService;

            sessionService.NewGame += OnNewGame;
            //Random rndm = new();
            //for (int i = 0; i < 10; i++)
            //{
            //    IdleGames.Add(new GameInfoMessage()
            //    {
            //        title = "Neroxis_test_test_1.2.4_",
            //        mapname = "Neroxis_test_test_1.2.4_",
            //        MapPreviewSource = App.Current.Resources["MapGenIcon"] as ImageSource,
            //        max_players = rndm.Next(2, 16),
            //        featured_mod = "faf",
            //        game_type = "custom",
            //        sim_mods = new(),
            //        num_players = rndm.Next(2, 16),
            //        host = rndm.Next(1000, 10000).ToString()
            //    });
            //}
        }

        private void OnNewGame(object sender, EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            var games = _IdleGames;
            var suspiciousGames = SuspiciousGames;
            // Update in-game players status

            game.Teams = GetInGameTeams(game);

            #region Checking suspicious games with NO PLAYERS

            for (int i = 0; i < suspiciousGames.Count; i++)
            {
                if (suspiciousGames[i].host == game.host)
                {
                    if (game.num_players == 0) continue;
                    suspiciousGames.RemoveAt(i);
                    i++;
                    continue;
                }

                var difference = System.DateTime.UtcNow - suspiciousGames[i].CreatedTime.Value;

                if (difference.TotalSeconds > 120)
                {
                    games.Remove(suspiciousGames[i]);
                    suspiciousGames.RemoveAt(i);
                }
            }
            #endregion

            #region Searching matches in list of idle games
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].host == game.host)
                {
                    if (game.launched_at != null)
                    {
                        // game is launched, removing from IdleGames and moving to LiveGames
                        games.RemoveAt(i);
                        _LiveGames.Add(game);
                        return;
                    }

                    // Updating idle game states
                    if (!games[i].Update(game))
                    {
                        // returns false if num_players == 0, game is died
                        games.RemoveAt(i);
                    }
                    return;
                }
            }
            #endregion

            #region Processing if game is live
            // if game is live
            if (game.launched_at != null)
            {
                var liveGames = _LiveGames;

                for (int i = 0; i < liveGames.Count; i++)
                {
                    if (liveGames[i].host == game.host)
                    {
                        if (liveGames[i].mapname != game.mapname)
                        {
                            // add small map preview for normal games
                            if (game.PreviewType == PreviewType.Normal)
                                game.MapPreviewSource = App.Current.Dispatcher.Invoke(() =>
                                {
                                    var preview = MapService.GetMap(new("https://content.faforever.com/maps/previews/small/" + game.mapname + ".png"));
                                    // some maps cant be found on faf server.
                                    return preview;
                                },
                                    System.Windows.Threading.DispatcherPriority.Background);
                        }
                        // Updating idle game states
                        if (!liveGames[i].Update(game))
                        {
                            // returns false if num_players == 0, game is died
                            liveGames.RemoveAt(i);
                        }
                        return;
                    }
                }

                // if we passed this way, that we didnt found matches in LiveGames

                // filling host by player instance
                if (game.Host == null)
                    game.Host = PlayersService.GetPlayer(game.host);

                liveGames.Add(game);
                return;
            } 
            #endregion

            // filling host by player instance
            if (game.Host == null)
                game.Host = PlayersService.GetPlayer(game.host);

            if (game.num_players == 0)
            {
                // if game is empty, we adding it to suspicious list and monitoring it during next updates
                game.CreatedTime = System.DateTime.UtcNow;
                SuspiciousGames.Add(game);
            }

            // add small map preview for normal games
            if (game.PreviewType == PreviewType.Normal)
                game.MapPreviewSource = App.Current.Dispatcher.Invoke(() =>
                {
                    var preview = MapService.GetMap(new("https://content.faforever.com/maps/previews/small/" + game.mapname + ".png"));
                    // some maps cant be found on faf server.
                    return preview;
                },
                    System.Windows.Threading.DispatcherPriority.Background);

            // finally if nothing matched we adding it to IdleGames
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
