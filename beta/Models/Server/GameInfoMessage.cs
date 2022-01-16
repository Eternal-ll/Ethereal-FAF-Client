using beta.Views;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public enum LobbyState
    {
        Broken = -1,
        Warn = 0,
        New = 1,
        Launched = 2
    }

    public enum PreviewType
    {
        Normal = 0,
        Coop = 1,
        Neroxis = 2
    }
    public struct GameInfoMessage: MainView.IServerMessage
    {
        public static GameInfoMessage New() => new();
        public static GameInfoMessage New(GameInfoMessage old) => new()
        {
            command = old.command,
            games = old.games,
            visibility = old.visibility,
            password_protected = old.password_protected,
            uid=old.uid,
            title=old.title,
            state = old.state,
            game_type = old.game_type,
            featured_mod = old.featured_mod,
            sim_mods = old.sim_mods,
            mapname = old.mapname,
            map_file_path = old.map_file_path,
            host=old.host,
            num_players = old.num_players,
            max_players = old.max_players,
            launched_at = old.launched_at,
            rating_type = old.rating_type,
            rating_max = old.rating_max,
            rating_min = old.rating_min,
            enforce_rating_range = old.enforce_rating_range,
            teams = old.teams
        };


        #region Custom properties

        #region MapName
        public string _MapName;
        public string MapName
        {
            get
            {
                if(_MapName is null)
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
        public string _MapVersion;
        public string MapVersion => _MapVersion ??= mapname.Split('.')[1];
        #endregion

        #endregion

        public string command { get; set; }

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

        public int LifyCycle;
        
            
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
        public int num_players { get; set; }
        public int max_players { get; set; }
        public double? launched_at { get; set; }
        public string rating_type { get; set; }
            
        public double? rating_min { get; set; }

        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }
        public Dictionary<int, string[]> teams { get; set; }
    }
}
