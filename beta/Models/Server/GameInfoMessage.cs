using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public enum PreviewType : byte
    {
        Normal = 0,
        Coop = 1,
        Neroxis = 2
    }

    public struct InGameTeam
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

        public InGameTeam(int number, IPlayer[] players) : this()
        {
            Number = number;
            Players = players;
        }
    }

    public static class GameInfoExtensions
    {
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
                    OnPropertyChanged(nameof(Map.SmallPreview));
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
                }
            }
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

        public DateTime? CreatedTime { get; set; }

        public PlayerInfoMessage Host { get; set; }

        #endregion

        //public string command { get; set; }

        public GameInfoMessage[] games { get; set; }
        public string visibility { get; set; }
        public bool password_protected { get; set; }
        public long uid { get; set; }
        public string title { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        //Open / Playing / Closed
        //TODO Cant converter "closed" to enum
        public GameState State { get; set; }

        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonPropertyName("featured_mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }

        public Dictionary<string, string> sim_mods { get; set; }

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
                }
            }
        }
        #endregion

        public string map_file_path { get; set; }
        public string host { get; set; }

        #region num_players
        private int _num_players;
        public int num_players
        {
            get => _num_players;
            set => Set(ref _num_players, value);
        } 
        #endregion

        public int max_players { get; set; }
        public double? launched_at { get; set; }
        public string rating_type { get; set; }
        public double? rating_min { get; set; }
        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }
        public Dictionary<int, string[]> teams { get; set; }
    }
}
