using beta.Models.Server.Base;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;

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
        public string Name => Number > 1 ? "Team " + (Number - 1) : "Team? " + Number;
        
        public int Number { get; }
        public IPlayer[] Players { get; }

        public InGameTeam(int number, IPlayer[] players) : this()
        {
            Number = number;
            Players = players;
        }
    }

    public static class GameInfoExtensions
    {
        public static GameInfoMessage Update(this GameInfoMessage orig, GameInfoMessage newInfo)
        {
            orig.title = newInfo.title;

            orig.num_players = newInfo.num_players;
            
            orig.Teams = newInfo.Teams;
            orig.teams = newInfo.teams;

            orig.sim_mods = newInfo.sim_mods;
            return orig;
        }
    }

    public class GameInfoMessage: ViewModel, IServerMessage
    {
        #region Custom properties

        #region MapName
        private string _MapName;
        public string MapName
        {
            get
            {
                if (_MapName is null)
                {
                    _MapName += char.ToUpper(mapname[0]);
                    for (int i = 1; i < mapname.Length; i++)
                    {
                        if (mapname[i] == '.') break;
                        if (mapname[i] == '_')
                        {
                            _MapName += " ";
                            _MapName += mapname[i + 1];
                            i++;
                        }
                        else _MapName += mapname[i];
                    }
                }
                return _MapName;
            }
        }
        #endregion

        #region MapVersion
        private string _MapVersion;
        public string MapVersion => _MapVersion ??= mapname.Split('.').Length > 1? mapname.Split('.')[1] : string.Empty;
        #endregion

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

        #region MapPreview // UNUSED
        // UNUSED
        private object _MapPreview;
        public object MapPreview => _MapPreview ??= PreviewType switch
        {
            PreviewType.Normal => new Uri("https://content.faforever.com/maps/previews/small/" + mapname + ".png"),
            PreviewType.Coop => App.Current.Resources["CoopIcon"],
            PreviewType.Neroxis => App.Current.Resources["MapGenIcon"],
            _ => App.Current.Resources["QuestionIcon"]
        };
        #endregion

        #region MapNameType
        private KeyValuePair<string, PreviewType> _MapNameType;
        public KeyValuePair<string, PreviewType> MapNameType => _MapNameType.Key == null ?
            new KeyValuePair<string, PreviewType>(mapname, PreviewType) : _MapNameType;
        #endregion

        #region Teams
        private InGameTeam[] _Teams;
        public InGameTeam[] Teams
        {
            get => _Teams;
            set => Set(ref _Teams, value);
        }
        #endregion

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
        public string mapname { get; set; }
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
