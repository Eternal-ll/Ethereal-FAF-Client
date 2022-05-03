using beta.Infrastructure.Utils;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.Server
{
    public enum PreviewType : byte
    {
        Normal = 0,
        Coop = 1,
        Neroxis = 2
    }

    public class InGameTeam
    {
        public string Name => Number switch
        {
            > 0 => Number - 1 == 0 ? "No team" : "Team " + (Number - 1),
            -1 => "Observers",
            _ => "Unknown",
        };

        public int Number { get; }
        public IPlayer[] Players { get; }
        public int SumRating
        {
            get
            {
                if (Players is null || Players.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Players.Length; i++)
                    if (Players[i] is PlayerInfoMessage player)
                        sum += player.ratings["global"].DisplayedRating;
                return sum;
            }
        }
        public int AverageRating
        {
            get
            {
                if (Players.Length == 0) return 0;

                var avg = SumRating / Players.Length;
                if (avg > 1000)
                    return (int)Math.Round(avg * .01) * 100;

                if (avg > 100)
                    return (int)Math.Round(avg * .01) * 100;

                return SumRating / Players.Length;
            }
        }

        public InGameTeam(int number, IPlayer[] players)
        {
            Number = number;
            Players = players;
        }
    }

    public static class GameInfoExtensions
    {
        /// <summary>
        /// Returns false if game is end
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="newInfo"></param>
        /// <returns></returns>
        public static bool Update(this GameInfoMessage orig, GameInfoMessage newInfo)
        {
            if (orig.num_players > newInfo.num_players && newInfo.num_players == 0)
                return false;

            if (orig.num_players < newInfo.num_players)
                orig.PlayersCountChanged = 1;
            else if (orig.num_players > newInfo.num_players)
                orig.PlayersCountChanged = -1;
            else orig.PlayersCountChanged = null;

            orig.title = newInfo.title;

            orig.num_players = newInfo.num_players;

            // We have to change status of players that left or join
            // TODO FIX ME Rewrite this shit
            #region Updating status of left players
            IPlayer[] origPlayers = new IPlayer[30];
            int k = 0;
            for (int i = 0; i < orig.Teams.Length; i++)
            {
                var players = orig.Teams[i].Players;
                for (int j = 0; j < players.Length; j++)
                {
                    origPlayers[k] = players[j];
                    k++;
                }
            }

            IPlayer[] newPlayers = new IPlayer[30];
            k = 0;
            for (int i = 0; i < newInfo.Teams.Length; i++)
            {
                var players = newInfo.Teams[i].Players;
                for (int j = 0; j < players.Length; j++)
                {
                    newPlayers[k] = players[j];
                    k++;
                }
            }

            var enumerator = origPlayers.Except(newPlayers).GetEnumerator();
            while (enumerator.MoveNext())
                if(enumerator.Current is PlayerInfoMessage player)
                    player.Game = null;
            
            #endregion

            orig.Teams = newInfo.Teams;
            orig.teams = newInfo.teams;
                
            orig.sim_mods = newInfo.sim_mods;

            //orig.Map = newInfo.Map;

            //Map update
            if (orig.mapname != newInfo.mapname)
            {
                orig.map_file_path = newInfo.map_file_path;
                orig.max_players = newInfo.max_players;
                orig.mapname = newInfo.mapname;
            }

            orig.password_protected = newInfo.password_protected;

            return true;
        }
    }

    public class GameInfoMessage: ViewModel, IServerMessage
    {
        public ServerCommand Command { get; set; }
        public GameInfoMessage[] games { get; set; }

        #region Custom properties

        #region Map
        private GameMap _Map;
        public GameMap Map
        {
            get => _Map;
            set
            {
                if (Set(ref _Map, value))
                {
                    OnPropertyChanged(nameof(Map.NewPreview));
                }
            }
        }
        #endregion

        #region PreviewType // On fly
        public PreviewType PreviewType
        {
            get
            {
                if (GameType == GameType.Coop)
                    return PreviewType.Coop;
                if (mapname.Contains("neroxis", StringComparison.OrdinalIgnoreCase))
                    return PreviewType.Neroxis;
                return PreviewType.Normal;
            }
        }
        #endregion

        #region Teams
        private InGameTeam[] _Teams;
        public InGameTeam[] Teams
        {
            get => _Teams;
            set
            {
                var old = AverageRating;
                if (Set(ref _Teams, value))
                {
                    if (old != AverageRating)
                        OnPropertyChanged(nameof(AverageRating));

                    var favouritesCount = 0;
                    var friendsCount = 0;
                    var clanmatesCount = 0;
                    var foesCount = 0;
                    if (value is not null)
                    {
                        List<PlayerInfoMessage> players = new();
                        for (int i = 0; i < value.Length; i++)
                        {
                            var team = value[i];
                            for (int j = 0; j < team.Players.Length; j++)
                            {
                                var iPlayer = team.Players[j];
                                if (iPlayer is not PlayerInfoMessage player) continue;

                                if (player.IsFavourite)
                                    favouritesCount++;
                                if (player.IsClanmate)
                                    clanmatesCount++;
                                if (player.RelationShip == PlayerRelationShip.Friend)
                                    friendsCount++;
                                if (player.RelationShip == PlayerRelationShip.Foe)
                                    foesCount++;
                                players.Add(player);
                            }
                        }
                        Players = players.ToArray();
                    }
                    else
                    {
                        Players = null;
                    }
                    Favourites = favouritesCount;
                    Friends = friendsCount;
                    Clanmates = clanmatesCount;
                    Foes = foesCount;
                }
            }
        }
        #endregion

        #region Players
        private PlayerInfoMessage[] _Players;
        public PlayerInfoMessage[] Players
        {
            get => _Players;
            set => Set(ref _Players, value);
        }
        #endregion

        #region AverageRating // On fly
        public int AverageRating
        {
            get
            {
                if (_Teams is null || _Teams.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < _Teams.Length; i++)
                    sum += _Teams[i].AverageRating;
                return (int)Math.Round((sum / _Teams.Length) * .01) * 100;
            }
        }

        #endregion

        #region PlayersCountChanged
        private int? _PlayersCountChanged;
        public int? PlayersCountChanged
        {
            get => _PlayersCountChanged;
            set => Set(ref _PlayersCountChanged, value);
        }
        #endregion

        #region Size
        private string _Size;
        public string Size
        {
            get => _Size;
            set => Set(ref _Size, value);
        }

        #endregion

        #region MapName // On fly

        public string MapName
        {
            get
            {
                //switch (Map)
                //{
                //    case GameMap gameMap:
                if (Map?.Name is not null) return Map.Name;
                else
                {
                    var parts = _mapname.Split('.')[0].Split('_');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i].Length == 0) continue;
                        parts[i] = char.ToUpper(parts[i][0]) + parts[i][1..];
                    }
                    return string.Join(' ', parts);
                }
                //    case NeroxisMap neroxisMap:
                //        return neroxisMap.Name;
                //        case CoopMap coopMap:
                //        return "Coop : " + _mapname;
                //    default:
                //        return "Unknown: " + _mapname;
                //}
            }
        }

        #endregion

        #region MapVersion // On fly
        public string MapVersion
        {
            get
            {
                string mapVersion = string.Empty;

                if (_mapname.StartsWith("neroxis", StringComparison.OrdinalIgnoreCase))
                    return string.Empty;

                for (int i = 0; i < _mapname.Length; i++)
                {
                    if (_mapname[i] == '.')
                    {
                        var version = _mapname.Substring(i + 2);
                        if (version.Length <= 4)
                            if (int.TryParse(version, out var res))
                                mapVersion = res.ToString();
                        break;
                    }
                }

                return mapVersion.Length > 4 ? string.Empty : mapVersion;
            }
        }
        #endregion

        #region Duration
        private TimeSpan _Duration;
        public TimeSpan Duration
        {
            get => _Duration;
            set => Set(ref _Duration, value);
        }
        #endregion

        #region Relationsships

        #region Favourites
        private int _Favourites;
        public int Favourites
        {
            get => _Favourites;
            set => Set(ref _Favourites, value);
        }
        #endregion

        #region Friends
        private int _Friends;
        public int Friends
        {
            get => _Friends;
            set => Set(ref _Friends, value);
        }
        #endregion

        #region Clanmates
        private int _Clanmates;
        public int Clanmates
        {
            get => _Clanmates;
            set => Set(ref _Clanmates, value);
        }
        #endregion

        #region Foes
        private int _Foes;
        public int Foes
        {
            get => _Foes;
            set => Set(ref _Foes, value);
        }
        #endregion

        #endregion

        public bool ReplayLessThanFiveMinutes => !launched_at.HasValue || (DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(launched_at.Value))
                    .TotalSeconds > 300;

        public DateTime? CreatedTime { get; set; }
        public DateTime Updated { get; set; }

        public IPlayer Host { get; set; }

        #endregion

        //public string command { get; set; }

        [JsonPropertyName("visibility")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameVisibility Visibility { get; set; }
        public bool password_protected { get; set; }
        public long uid { get; set; }

        #region title
        private string _title;
        public string title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        public GameState OldState { get; set; }

        #region State
        private GameState _State;
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        //Open / Playing / Closed
        //TODO Cant converter "closed" to enum
        public GameState State
        {
            get => _State;
            set
            {
                var oldState = _State;
                if (Set(ref _State, value))
                {
                    OldState = oldState;
                    OnPropertyChanged(nameof(OldState));
                }
            }
        } 
        #endregion

        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonPropertyName("featured_mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }

        #region sim_mods
        private Dictionary<string, string> _sim_mods;
        public Dictionary<string, string> sim_mods
        {
            get => _sim_mods;
            set => Set(ref _sim_mods, value);
        } 
        #endregion
         
        #region mapname
        private string _mapname;
        public string mapname
        {
            get => _mapname;
            set
            {
                if (Set(ref _mapname, value))
                {
                    OnPropertyChanged(nameof(MapVersion));
                    OnPropertyChanged(nameof(MapName));
                    OnPropertyChanged(nameof(max_players));
                    _AvatarImage = null;
                    OnPropertyChanged(nameof(AvatarImage));
                }
            }
        }
        #endregion


        #region AvatarImage
        private ImageSource _AvatarImage;
        public ImageSource AvatarImage
        {
            get
            {
                if (_AvatarImage is null)
                {
                    if (mapname.StartsWith("neroxis"))
                    {
                        _AvatarImage = App.Current.Resources["MapGenIcon"] as ImageSource;
                    }
                    else
                    {
                        _AvatarImage = ImageTools.InitializeLazyBitmapImage($"https://content.faforever.com/maps/previews/small/{mapname}.png", 100, 100);
                    }
                }
                return _AvatarImage;
            }
        } 
        #endregion

        #region map_file_path
        private string _map_file_path;
        public string map_file_path
        {
            get => _map_file_path;
            set => Set(ref _map_file_path, value);
        }
        #endregion


        public bool IsNeroxisMap => mapname.Contains("Neroxis", StringComparison.OrdinalIgnoreCase);

        public string host { get; set; }

        #region num_players
        private int _num_players;
        public int num_players
        {
            get => _num_players;
            set => Set(ref _num_players, value);
        }
        #endregion

        #region max_players
        private int _max_players;
        public int max_players
        {
            get => _max_players;
            set => Set(ref _max_players, value);
        } 
        #endregion

        public double? launched_at { get; set; }
        [JsonPropertyName("rating_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RatingType RatingType { get; set; }
        public double? rating_min { get; set; }
        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }

        #region teams
        private Dictionary<int, string[]> _teams;
        public Dictionary<int, string[]> teams
        {
            get => _teams;
            set
            {
                if (Set(ref _teams, value))
                {
                    if (value is not null)
                    {
                        if (value.Count == 0)
                        {
                            PlayersLogins = Array.Empty<string>();
                            return;
                        }
                        List<string> players = new();
                        foreach (var teammates in value.Values)
                        {
                            for (int i = 0; i < teammates.Length; i++)
                            {
                                players.Add(teammates[i]);
                            }
                        }
                        PlayersLogins = players.ToArray();
                    }
                    else
                    {
                        PlayersLogins = null;
                    }
                }
            }
        } 
        #endregion

        public string[] PlayersLogins { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Map?.Dispose();
                _Teams = null;
            }
            base.Dispose(disposing);
        }
    }
}
