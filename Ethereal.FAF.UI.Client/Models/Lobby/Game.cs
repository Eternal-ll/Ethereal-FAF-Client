using Ethereal.FAF.UI.Client.Infrastructure.Services;
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
        Generated
    }

    public interface IPlayer
    {
        public long Id { get; set; }
        public string Login { get; set; }
    }
    public class GamePlayer : IPlayer
    {
        public long Id { get; set; }
        public string Login { get; set; }
    }

    public class GameTeam
    {
        public int Id { get; set; }
        public IPlayer[] Players { get; set; }

        public string HumanTitle => Id switch
        {
            > 0 => Id - 1 == 0 ? "No team" : "Team " + (Id - 1),
            -1 => "Observers",
            _ => "Unknown",
        };
    }

    public class Game : GameInfoMessage
    {
        [JsonPropertyName("games")]
        public new Game[] Games { get; set; }

        [JsonIgnore]
        public PreviewType PreviewType => 
            GameType == GameType.Coop
            ? PreviewType.Coop
            : IsMapgen ?
                PreviewType.Neroxis :
                PreviewType.Normal;

        [JsonIgnore]
        public string SmallMapPreview { get; set; }
        [JsonIgnore]
        public string LargeMapPreview => $"https://content.faforever.com/maps/previews/large/{Mapname}.png";
        [JsonIgnore]
        public string HumanTitle => Title.Truncate(34);
        [JsonIgnore]
        public string HumanLaunchedAt => LaunchedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)LaunchedAt.Value).Humanize() : null;


        #region Map generator
        public bool IsMapgen => MapGenerator.IsGeneratedMap(Mapname);

        #region MapGeneratorState
        private MapGeneratorState _MapGeneratorState;
        public MapGeneratorState MapGeneratorState
        {
            get => _MapGeneratorState;
            set => Set(ref _MapGeneratorState, value);
        }
        #endregion

        #endregion


        public IPlayer HostPlayer => GameTeams.SelectMany(t => t.Players).FirstOrDefault(p => p.Login == Host);

        private GameTeam[] _GameTeams;
        public GameTeam[] GameTeams
        {
            get
            {
                if (_GameTeams is not null) return _GameTeams;
                var teams = new GameTeam[Teams.Count];
                var teamIndex = 0;
                foreach (var team in Teams)
                {
                    var gTeam = new GameTeam()
                    {
                        Id = team.Key
                    };
                    gTeam.Players = new IPlayer[team.Value.Length];
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
                return teams;
            }
        }


        private FA.Vault.MapScenario _MapScenario;
        public FA.Vault.MapScenario MapScenario
        {
            get => _MapScenario;
            set => Set(ref _MapScenario, value);
        }
    }
}
