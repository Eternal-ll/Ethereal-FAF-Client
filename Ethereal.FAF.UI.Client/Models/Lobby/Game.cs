using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Humanizer;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Lobby
{
    public enum PreviewType : byte
    {
        Normal = 0,
        Coop = 1,
        Neroxis = 2
    }

    public enum MapGeneratorState : byte
    {
        NotGenerated,
        Generating,
        Generated,
        Faulted
    }

    public interface IPlayer
    {
        public long Id { get; set; }
        public string Login { get; set; }
    }
    public sealed class GamePlayer : ViewModel, IPlayer
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public RatingType RatingType { get; set; }
        public int Rating { get; set; }
        public int Games { get; set; }
        public bool IsHost { get; set; }

        public bool HasPlayerInstance => Player is not null;

        #region Player
        private Player _Player;
        public Player Player
        {
            get => _Player;
            set
            {
                if (Set(ref _Player, value))
                {
                    OnPropertyChanged(nameof(HasPlayerInstance));
                    if (value is not null)
                    {
                        Rating = RatingType switch
                        {
                            RatingType.global => value.Global,
                            RatingType.ladder_1v1 => value.Ladder1v1,
                            RatingType.tmm_4v4_full_share => value.Tmm4v4,
                            RatingType.tmm_4v4_share_until_death => value.Tmm4v4,
                            RatingType.tmm_2v2 => value.Tmm2v2,
                            RatingType.tmm_3v3 => value.Ladder1v1,
                        };
                        Games = RatingType switch
                        {
                            RatingType.global => value.GlobalGames,
                            RatingType.ladder_1v1 => value.Ladder1v1Games,
                            RatingType.tmm_4v4_full_share => value.Tmm4v4Games,
                            RatingType.tmm_4v4_share_until_death => value.Tmm4v4Games,
                            RatingType.tmm_2v2 => value.Tmm2v2Games,
                            RatingType.tmm_3v3 => value.Ladder1v1,
                        };
                    }
                }
            }
        }
        #endregion

        #region IsConnected
        private bool _IsConnected = true;
        public bool IsConnected { get => _IsConnected;
            set 
            {
                if (Set(ref _IsConnected, value))
                {
                    if (!value && Player is not null)
                    {
                        Player.Game = null;
                    }
                }
            }
        } 
        #endregion
    }

    public sealed class GameTeam
    {
        public int Id { get; set; }
        public GamePlayer[] GamePlayers { get; set; }

        public string HumanTitle => Id switch
        {
            > 0 => Id - 1 == 0 ? "FFA" : "Team " + (Id - 1),
            -1 => "Observers",
            _ => "Unknown",
        };

        public int TeamRating
        {
            get
            {
                if (GamePlayers is null) return 0;
                var sum = 0;
                foreach (var player in GamePlayers)
                {
                    sum += player.Rating;
                }
                return sum;
            }
        }
    }

    public sealed class Game : GameInfoMessage
    {
        #region Mapname
        private string _Mapname;
        [JsonPropertyName("mapname")]
        new public string Mapname
        {
            get => _Mapname;
            set
            {
                if (Set(ref _Mapname, value))
                {
                    MapGeneratorState = MapGeneratorState.NotGenerated;
                    SmallMapPreview = string.IsNullOrWhiteSpace(value) || IsMapgen ?
                        null :
                        $"https://content.faforever.com/maps/previews/small/{value}.png";
                    OnPropertyChanged(nameof(IsMapgen));
                }
            }
        }
        #endregion

        [JsonIgnore]
        private string _SmallMapPreview = null;
        public string SmallMapPreview
        {
            get => _SmallMapPreview;
            set => Set(ref _SmallMapPreview, value);
        }
        [JsonIgnore]
        public string LargeMapPreview => $"https://content.faforever.com/maps/previews/large/{Mapname}.png";
        [JsonIgnore]
        public string HumanTitle => Title.Truncate(34);
        [JsonIgnore]
        public string HumanLaunchedAt => "";//LaunchedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)LaunchedAt.Value).Humanize() : null;
        public TimeSpan LaunchedAtTimeSpan => LaunchedAt is null ? TimeSpan.Zero :
            DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds((long)LaunchedAt.Value);


        #region Map generator
        public bool IsMapgen => MapGenerator.IsGeneratedMap(Mapname);
        
        #region MapGeneratorException
        private string _MapGeneratorException;
        public string MapGeneratorException { get => _MapGeneratorException; set => Set(ref _MapGeneratorException, value); } 
        #endregion

        #region MapGeneratorState
        private MapGeneratorState _MapGeneratorState;
        public MapGeneratorState MapGeneratorState
        {
            get => _MapGeneratorState;
            set => Set(ref _MapGeneratorState, value);
        }
        #endregion

        #endregion

        private Player _HostPlayer;

        public Player HostPlayer { get => _HostPlayer; set => Set(ref _HostPlayer, value); }

        private GameTeam[] _GameTeams;
        public GameTeam[] GameTeams
        {
            get => _GameTeams;
            set
            {
                if (Set(ref _GameTeams, value))
                {

                }
            }
        }

        public string[] PlayersLogins => Teams.SelectMany(t => t.Value).ToArray();
        public long[] PlayersIds => TeamsIds.SelectMany(t => t.PlayerIds).ToArray();
        public GamePlayer[] Players => GameTeams.SelectMany(t => t.GamePlayers).ToArray();

        public void UpdateTeams()
        {
            var teams = new GameTeam[Teams.Count];
            var teamIndex = 0;
            foreach (var team in Teams)
            {
                var gTeam = new GameTeam
                {
                    Id = team.Key,
                    GamePlayers = new GamePlayer[team.Value.Length]
                };
                for (int i = 0; i < team.Value.Length; i++)
                {
                    gTeam.GamePlayers[i] = new GamePlayer()
                    {
                        Login = team.Value[i],
                        RatingType = RatingType
                    };
                }
                teams[teamIndex] = gTeam;
                teamIndex++;
            }
            teamIndex = 0;
            foreach (var team in TeamsIds)
            {
                var gTeam = teams[teamIndex];
                for (int i = 0; i < team.PlayerIds.Length; i++)
                {
                    gTeam.GamePlayers[i].Id = team.PlayerIds[i];
                }
                teamIndex++;
            }
            //var teams = new GameTeam[TeamsIds.Length];
            //foreach (var team in TeamsIds)
            //{
            //}
            //for (int i = 0; i < TeamsIds.Length; i++)
            //{
            //    var team = TeamsIds[i];
            //    var gTeam = new GameTeam
            //    {
            //        Id = team.TeamId,
            //        Players = new GamePlayer[team.PlayerIds.Length]
            //    };
            //    for (int i = 0; i < team.PlayerIds.Length; i++)
            //    {
            //        gTeam.Players[i] = new GamePlayer()
            //        {
            //            Id = team.PlayerIds[i],
            //            RatingType = RatingType
            //        };
            //    }
            //    teams[teamIndex] = gTeam;
            //}
            GameTeams = teams;
        }

        public GamePlayer GetPlayer(long uid) => Players.FirstOrDefault(p => p.Id == uid);
        public bool TryGetPlayer(long uid, out GamePlayer player)
        {
            if (GameTeams is null)
            {

            }
            player = GetPlayer(uid);
            return player is not null;
        }


        private FA.Vault.MapScenario _MapScenario;
        public FA.Vault.MapScenario MapScenario
        {
            get => _MapScenario;
            set => Set(ref _MapScenario, value);
        }


        public bool IsRanked => ApiGameValidatyState is ApiGameValidatyState.VALID;
        #region ApiGameValidatyState
        private ApiGameValidatyState _ApiGameValidatyState = ApiGameValidatyState.UNKNOWN;
        public ApiGameValidatyState ApiGameValidatyState
        {
            get => _ApiGameValidatyState;
            set
            {
                if (Set(ref _ApiGameValidatyState, value))
                {
                    OnPropertyChanged(nameof(IsRanked));
                }
            }
        }

        #endregion

        #region VictoryCondition
        private string _VictoryCondition;
        public string VictoryCondition { get => _VictoryCondition; set => Set(ref _VictoryCondition, value); }
        #endregion

        public bool IsPassedFilter { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public TimeSpan AfterCreationTimeSpan => DateTime.Now - Created;


        public bool UnSupported => FeaturedMod is not (FeaturedMod.FAF or FeaturedMod.FAFBeta or FeaturedMod.FAFDevelop or FeaturedMod.coop);
        public bool IsDevChannel => FeaturedMod is FeaturedMod.FAFBeta or FeaturedMod.FAFDevelop;
    }
}
