using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels;
using beta.ViewModels.Base;
using System;
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
        private readonly IMapsService MapService;

        #endregion

        #region IdleGames
        private readonly ObservableCollection<GameVM> _IdleGames = new();
        public ObservableCollection<GameVM> IdleGames => _IdleGames;
        #endregion

        #region LiveGames
        private readonly ObservableCollection<GameVM> _LiveGames = new();
        public ObservableCollection<GameVM> LiveGames => _LiveGames;
        #endregion

        /// <summary>
        /// Idle games without players. Bugged or just created
        /// </summary>
        private readonly List<GameVM> SuspiciousGames = new();

        #endregion

        public GamesServices(
            ISessionService sessionService,
            IPlayersService playerService,
            IMapsService mapService)
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

            var idleGames = _IdleGames;
            var liveGames = _LiveGames;
            var suspiciousGames = SuspiciousGames;

            var newTeams = GetInGameTeams(game.teams);

            #region Checking suspicious games with NO PLAYERS

            for (int i = 0; i < suspiciousGames.Count; i++)
            {
                if (suspiciousGames[i].Host.login.Equals(game.host, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (game.num_players == 0) continue;
                    suspiciousGames.RemoveAt(i);
                    i++;
                    continue;
                }

                var difference = System.DateTime.UtcNow - suspiciousGames[i].CreatedTime.Value;

                if (difference.TotalSeconds > 120)
                {
                    if (suspiciousGames[i].PlayersCount == 0)
                        idleGames.Remove(suspiciousGames[i]);
                    suspiciousGames.RemoveAt(i);
                }
            }
            #endregion

            #region Searching matches in list of idle games
            for (int i = 0; i < idleGames.Count; i++)
            {
                var idleGame = idleGames[i];
                if (idleGame.Host.login.Equals(game.host, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (game.launched_at != null)
                    {
                        // game is launched, removing from IdleGames and moving to LiveGames
                        idleGames.RemoveAt(i);
                        liveGames.Add(idleGame);
                        return;
                    }

                    if (idleGame.MapName != game.mapname)
                    {
                        idleGame.Map = MapService.GetMap(new("https://content.faforever.com/maps/previews/small/" + game.mapname + ".png"),
                            attachScenario: true);
                    }

                    // TODO rework process of updating players in game
                    idleGame.UpdateTeams(newTeams);

                    // Updating idle game stats
                    if (!idleGame.Update(game))
                    {
                        // returns false if num_players == 0, game is died
                        // 2 -> 1 -> 0 -> died, end
                        idleGames.RemoveAt(i);
                    }
                    return;
                }
            }
            #endregion

            #region Processing if game is live
            // if game is live
            if (game.launched_at != null)
            {
                for (int i = 0; i < liveGames.Count; i++)
                {
                    var liveGame = liveGames[i];
                    if (liveGame.Host.login.Equals(game.host, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO rework process of updating players in game
                        liveGame.UpdateTeams(newTeams);

                        // Updating idle game states
                        if (!liveGame.Update(game))
                        {
                            // returns false if num_players == 0, game is died
                            // 2 -> 1 -> 0 -> died, end
                            liveGames.RemoveAt(i);
                        }
                        return;
                    }
                }
            }
            #endregion

            // if we passed this way, that we didnt found matches in LiveGames

            GameVM newGame = CreateNewGame(game);
            // TODO REWORK????
            // loading teams
            // TODO rework process of updating players in game
            newGame.UpdateTeams(newTeams);

            // if started, adding to live games and the end
            if (game.launched_at != null)
            {
                liveGames.Add(newGame);
                return;
            }

            if (game.num_players == 0)
            {
                // if game is empty, we adding it to suspicious list and monitoring it during next updates
                newGame.CreatedTime = System.DateTime.UtcNow;
                SuspiciousGames.Add(newGame);
            }

            // finally if nothing matched we adding it to IdleGames
            idleGames.Add(newGame);
        }

        private GameVM CreateNewGame(GameInfoMessage game)
        {
            GameVM newGame = new()
            {
                UID = game.uid,
                Title = game.title,
                Host = PlayersService.GetPlayer(game.host),
                Map = MapService.GetMap(new("https://content.faforever.com/maps/previews/small/" + game.mapname + ".png"),
                // detailed info: size / mexs / hydros / name / description
                attachScenario: true),
                // TODO Move to Map.cs?
                MapName = game.mapname,
                IsPasswordProtected = game.password_protected,
                // -----------
                // TODO Move to separate struct?
                MinPlayerRatingToJoin = game.rating_min,
                MaxPlayerRatingToJoin = game.rating_max,
                enforce_rating_range = game.enforce_rating_range,
                // -----------
                State = game.state,
                game_type = game.game_type,
                PlayersCount = game.num_players,
                // TODO Move to Map.cs?
                MaxPlayersCount = game.max_players,
                rating_type = game.rating_type,
                sim_mods = game.sim_mods,
                launched_at = game.launched_at,
                Visibility = game.visibility,
                FeaturedMod = Enum.Parse<FeaturedMod>(game.featured_mod, true)
            };
            if (newGame.Host == null)
            {

            }
            return newGame;
        }

        public InGameTeam[] GetInGameTeams(Dictionary<int, string[]> gameTeams)
        {
            InGameTeam[] teams = new InGameTeam[gameTeams.Count];

            int j = 0;

            // TODO: FIX ME Need to rework player game status

            //var playerStatus = GameState.Open;
            //if (game.launched_at != null)
            //{
            //    var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(game.launched_at.Value);
            //    playerStatus = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
            //}

            foreach (var valuePair in gameTeams)
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
                    players[i] = player;
                }

                teams[j] = new(valuePair.Key, players);
                j++;
            }
            return teams;
        }
    }
}
