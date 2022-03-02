using beta.Infrastructure.Services;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace beta.ViewModels
{

    public static class GameVMExtensions
    {
        public static void UpdateTeams(this GameVM game, InGameTeam[] newTeams)
        {
            // We have to change status of players that left or join
            // TODO FIX ME Rewrite this shit
            #region Updating status of left players
            IPlayer[] origPlayers = new IPlayer[30];
            int k = 0;
            if (game.Teams != null)
                for (int i = 0; i < game.Teams.Length; i++)
                {
                    var players = game.Teams[i].Players;
                    for (int j = 0; j < players.Length; j++)
                    {
                        origPlayers[k] = players[j];
                        k++;
                    }
                }

            IPlayer[] newPlayers = new IPlayer[30];
            k = 0;
            for (int i = 0; i < newTeams.Length; i++)
            {
                var players = newTeams[i].Players;
                for (int j = 0; j < players.Length; j++)
                {
                    newPlayers[k] = players[j];
                    if (players[j] is PlayerInfoMessage player)
                    {
                        player.Game = game;
                    }
                    k++;
                }
            }

            var enumerator = origPlayers.Except(newPlayers).GetEnumerator();
            while (enumerator.MoveNext())
                if (enumerator.Current is PlayerInfoMessage player)
                    player.Game = null;

            #endregion

            game.Teams = newTeams;
        }
        public static bool Update(this GameVM orig, GameInfoMessage newInfo)
        {
            if (orig.PlayersCount > newInfo.num_players && newInfo.num_players == 0)
                return false;

            if (orig.PlayersCount < newInfo.num_players)
                orig.PlayersCountChanged = 1;
            else if (orig.PlayersCount > newInfo.num_players)
                orig.PlayersCountChanged = -1;
            else orig.PlayersCountChanged = null;

            orig.Title = newInfo.title;

            orig.PlayersCount = newInfo.num_players;

            
            //orig.teams = newInfo.teams;

            orig.sim_mods = newInfo.sim_mods;

            //Map update
            if (orig.MapName != newInfo.mapname)
            {
                //orig.map_file_path = newInfo.map_file_path;
                orig.MaxPlayersCount = newInfo.max_players;
                orig.MapName = newInfo.mapname;
            }
            
            return true;
        }
    }
    public class GameVM : ViewModel
    {
        #region Custom properties

        #region MapName
        private string _MapName;
        public string MapName
        {
            get => _MapName;
            set
            {
                if (Set(ref _MapName, value))
                {
                    OnPropertyChanged(nameof(MapVersion));
                    OnPropertyChanged(nameof(MapNameFormatted));
                    //OnPropertyChanged(nameof(MaxPlayersCount));
                }
            }
        }
        #endregion

        #region PlayersCount
        private int _PlayersCount;
        public int PlayersCount
        {
            get => _PlayersCount;
            set => Set(ref _PlayersCount, value);
        }
        #endregion

        #region MaxPlayersCount
        private int _MaxPlayersCount;
        public int MaxPlayersCount
        {
            get => _MaxPlayersCount;
            set => Set(ref _MaxPlayersCount, value);
        }
        #endregion

        #region Map
        private Map _Map;
        public Map Map
        {
            get => _Map;
            set
            {
                if (Set(ref _Map, value))
                {
                    OnPropertyChanged(nameof(Map.MapPreviewSource));
                }
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

        #region PreviewType // On fly
        /// <summary>
        /// On fly
        /// </summary>
        public PreviewType PreviewType
        {
            get
            {
                if (game_type == "coop")
                    return PreviewType.Coop;
                if (MapName.Substring(0, 7) == "neroxis")
                    return PreviewType.Neroxis;
                return PreviewType.Normal;
            }
        }
        #endregion

        #region AverageRating // On fly
        /// <summary>
        /// On fly
        /// </summary>
        public int AverageRating
        {
            get
            {
                if (_Teams == null || _Teams.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < _Teams.Length; i++)
                    sum += _Teams[i].AverageRating;
                return (int)Math.Round((sum / _Teams.Length) * .01) * 100;
            }
        }

        #endregion

        // TODO REMOVE?
        #region MapNameFormatted // On fly
        /// <summary>
        /// On fly
        /// </summary>
        public string MapNameFormatted
        {
            get
            {
                if (Map.Scenario != null)
                    if (Map.Scenario.TryGetValue("name", out var name))
                        return name;

                string formattedMapName = string.Empty;
                for (int i = 0; i < _MapName.Length; i++)
                {
                    var letter = _MapName[i];
                    if (letter == '_' || letter == '-')
                    {
                        formattedMapName += ' ';
                        formattedMapName += char.ToUpper(_MapName[i + 1]);
                        i++;
                    }
                    else if (letter == '.')
                        break;
                    else if (i == 0) formattedMapName += char.ToUpper(letter);
                    else formattedMapName += letter;
                }
                return formattedMapName;
            }
        }

        #endregion

        // TODO REMOVE?
        #region MapVersion // On fly
        /// <summary>
        /// On fly
        /// </summary>
        public string MapVersion
        {
            get
            {
                string mapVersion = string.Empty;

                if (_MapName.StartsWith("neroxis", StringComparison.OrdinalIgnoreCase))
                    return string.Empty;

                for (int i = 0; i < _MapName.Length; i++)
                {
                    if (_MapName[i] == '.')
                    {
                        var version = _MapName.Substring(i + 2);
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

        #region Title
        private string _Title;
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        public long UID { get; set; }

        public DateTime? CreatedTime { get; set; }
        public IPlayer Host { get; set; }
        // PlayerInfoMessage
        // UnknownPlayer

        public FeaturedMod FeaturedMod { get; set; }
        public string Visibility { get; set; }
        public bool IsPasswordProtected { get; set; }

        #endregion

        public string State { get; set; }
        public string game_type { get; set; }


        public Dictionary<string, string> sim_mods { get; set; }

        public double? launched_at { get; set; }
        public string rating_type { get; set; }
        public double? MinPlayerRatingToJoin { get; set; }
        public double? MaxPlayerRatingToJoin { get; set; }
        public bool enforce_rating_range { get; set; }
    }
}
