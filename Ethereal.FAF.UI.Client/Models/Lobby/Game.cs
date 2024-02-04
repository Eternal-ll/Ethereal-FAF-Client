using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Models.Lobby
{
    public class Mod
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class SimMod : Mod
    {

    }
    public class SimModReadOnlyCollection : ReadOnlyCollection<SimMod>
    {
        public SimModReadOnlyCollection(IList<SimMod> list) : base(list)
        {
        }
    }
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

    public sealed partial class Game : ObservableObject
    {
        private bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

        #region Original properties
        /// <summary>
        /// 
        /// </summary>
        /// <example>public</example>
        [ObservableProperty]
        private GameVisibility _Visibility;
        [ObservableProperty]
        private bool _PasswordProtected;
        /// <summary>
        /// Game unique ID
        /// </summary>
        /// <example>19113763</example>
        [ObservableProperty]
        private long _Uid;
        [ObservableProperty]
        private string _Title;
        [ObservableProperty]
        private GameState _State;
        [ObservableProperty]
        private GameType _GameType;
        [ObservableProperty]
        private FeaturedMod _FeaturedMod;
        [ObservableProperty]
        private SimModReadOnlyCollection _SimMods;
        [ObservableProperty]
        private GameMap _Map;
        [ObservableProperty]
        private string _Host;
        [ObservableProperty]
        private int _NumPlayers;
        [ObservableProperty]
        private int _MaxPlayers;
        [ObservableProperty]
        private double? _LaunchedAt;
        public RatingType RatingType { get; set; }
        public double? RatingMin { get; set; }
        public double? RatingMax { get; set; }
        public bool EnforceRatingRange { get; set; }
        [ObservableProperty]
        private TeamIds[] _TeamsIds;
        [ObservableProperty]
        private Dictionary<int, string[]> _Teams;
        #endregion

        [ObservableProperty]
        private BitmapImage _MapSmallBitmapImage;

        #region Map generator
        public bool IsMapgen => false;
        [ObservableProperty]
        private string _MapGeneratorException;
        [ObservableProperty]
        private MapGeneratorState _MapGeneratorState;

        #endregion

        [ObservableProperty]
        private Player _HostPlayer;
        [ObservableProperty]
        private GameTeam[] _GameTeams;

        [Obsolete]
        [ObservableProperty]
        private string _SmallMapPreview;

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
            player = GetPlayer(uid);
            return player is not null;
        }

        [ObservableProperty]
        private FA.Vault.MapScenario _MapScenario;
    }
}
