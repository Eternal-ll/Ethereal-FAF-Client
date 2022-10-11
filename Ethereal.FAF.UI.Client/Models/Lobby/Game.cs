using Ethereal.FAF.API.Client.Models.Game;
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
    public class GamePlayer : ViewModel, IPlayer
    {
        public long Id { get; set; }
        public string Login { get; set; }

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

    public class GameTeam
    {
        public int Id { get; set; }
        public GamePlayer[] Players { get; set; }

        public string HumanTitle => Id switch
        {
            > 0 => Id - 1 == 0 ? "No team" : "Team " + (Id - 1),
            -1 => "Observers",
            _ => "Unknown",
        };
    }

    public class Game : GameInfoMessage
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
                    SmallMapPreview = string.IsNullOrWhiteSpace(value) ?
                        "/Resources/Images/1x1.png" :
                        $"https://content.faforever.com/maps/previews/small/{value}.png";
                    OnPropertyChanged(nameof(IsMapgen));
                }
            }
        }
        #endregion

        [JsonPropertyName("games")]
        public new Game[] Games { get; set; }

        [JsonIgnore]
        private string _SmallMapPreview = "/Resources/Images/1x1.png";
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
        public string HumanLaunchedAt => LaunchedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)LaunchedAt.Value).Humanize() : null;
        public TimeSpan LaunchedAtTimeSpan => DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds((long)LaunchedAt.Value);


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

        public GamePlayer HostPlayer => GameTeams.SelectMany(t => t.Players).FirstOrDefault(p => p.Login == Host);

        private GameTeam[] _GameTeams;
        public GameTeam[] GameTeams
        {
            get => _GameTeams;
            set
            {
                if (Set(ref _GameTeams, value))
                {
                    OnPropertyChanged(nameof(HostPlayer));
                }
            }
        }

        public string[] PlayersLogins => Teams.SelectMany(t => t.Value).ToArray();
        public long[] PlayersIds => TeamsIds.SelectMany(t => t.PlayerIds).ToArray();
        public GamePlayer[] Players => GameTeams.SelectMany(t => t.Players).ToArray();

        public void UpdateTeams()
        {
            var teams = new GameTeam[Teams.Count];
            var teamIndex = 0;
            foreach (var team in Teams)
            {
                var gTeam = new GameTeam()
                {
                    Id = team.Key
                };
                gTeam.Players = new GamePlayer[team.Value.Length];
                for (int i = 0; i < team.Value.Length; i++)
                {
                    gTeam.Players[i] = new GamePlayer()
                    {
                        Login = team.Value[i],
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
                    gTeam.Players[i].Id = team.PlayerIds[i];
                }
                teamIndex++;
            }
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
        
    }
}
