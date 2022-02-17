using beta.Models.Server.Base;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace beta.Models.Server
{
    public enum LobbyState
    {
        Broken = -1,
        Init = 0,
        New = 1,
        Unknown = 2,
        Old = 3,
        Launched = 4
    }

    public enum PreviewType
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
                if (Players == null || Players.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Players.Length; i++)
                    if (Players[i] is PlayerInfoMessage player)
                        sum += player.ratings["global"].DisplayedRating;
                return sum;
            }
        }
        public int AverageRating => Players.Length != 0 ? SumRating / Players.Length : 0;

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
        #region Custom properties

        #region LobbyState
        //private LobbyState _LobbyState;
        //public LobbyState LobbyState
        //{
        //    get
        //    {
        //        _LobbyState = num_players == 0 ? LobbyState.New : _LobbyState;
        //        return _LobbyState;
        //    }
        //    set => Set(ref _LobbyState, value);
        //}
        #endregion

        #region PreviewType
        public PreviewType PreviewType
        {
            get
            {
                if (game_type == "coop")
                    return PreviewType.Coop;
                if (map_file_path.Split('/')[1].Split("_")[0] == "neroxis")
                    return PreviewType.Neroxis;
                return PreviewType.Normal;
            }
        }
        #endregion

        #region MapPreview
        public ImageSource MapPreviewSource;
        public ImageSource MapPreview => PreviewType switch
        {
            PreviewType.Normal => MapPreviewSource,
            PreviewType.Coop => App.Current.Resources["CoopIcon"] as ImageSource,
            PreviewType.Neroxis => App.Current.Resources["MapGenIcon"] as ImageSource,
            _ => App.Current.Resources["QuestionIcon"] as ImageSource
        };
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

        #region AverageRating
        public int AverageRating
        {
            get
            {
                if (Teams == null || Teams.Length == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Teams.Length; i++)
                    sum += Teams[i].AverageRating;
                return sum / Teams.Length;
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

        public DateTime? CreatedTime { get; set; }

        public PlayerInfoMessage Host { get; set; }

        #endregion

        public string command { get; set; }

        public GameInfoMessage[] games { get; set; }
        public string visibility { get; set; }
        public bool password_protected { get; set; }
        public long uid { get; set; }
        public string title { get; set; }
        public string state { get; set; }
        public string game_type { get; set; }
        public string featured_mod { get; set; }
        public Dictionary<string, string> sim_mods { get; set; }

        #region mapname
        private string _mapname;
        public string mapname
        {
            get => _mapname;
            set => Set(ref _mapname, value);
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
