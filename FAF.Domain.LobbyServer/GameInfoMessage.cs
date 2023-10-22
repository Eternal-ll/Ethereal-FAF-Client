using FAF.Domain.LobbyServer.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	/// <summary>
	/// 
	/// </summary>
	/// <example>{"command":"game_info","visibility":"public","password_protected":true,"uid":19113763,"title":"Mark","state":"open","game_type":"custom","featured_mod":"faf","sim_mods":{},"mapname":"neroxis_map_generator_1.8.8_gnau2dr2nxbuk_bqeaiaa","map_file_path":"maps/neroxis_map_generator_1.8.8_gnau2dr2nxbuk_bqeaiaa.zip","host":"nthunter32","num_players":1,"max_players":12,"launched_at":null,"rating_type":"global","rating_min":null,"rating_max":null,"enforce_rating_range":false,"teams_ids":[{"team_id":2,"player_ids":[391628]}],"teams":{"2":["nthunter32"]}}</example>
	public class GameInfoMessage : INPC
    {
        [JsonPropertyName("games")]
        public GameInfoMessage[] Games { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <example>public</example>
        [JsonPropertyName("visibility")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameVisibility Visibility { get; set; }
        [JsonPropertyName("password_protected")]
        public bool PasswordProtected { get; set; }
        /// <summary>
        /// Game unique ID
        /// </summary>
        /// <example>19113763</example>
        [JsonPropertyName("uid")]
        public long Uid { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        #region State
        private GameState _State;
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameState State { get => _State; set => Set(ref _State, value); } 
        #endregion
        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        [JsonPropertyName("featured_mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }
        [JsonPropertyName("sim_mods")]
        public Dictionary<string, string> SimMods { get; set; }
        #region Mapname
        private string _Mapname;
        /// <summary>
        /// Map name. Example: x1ca_coop_002.v0023 / neroxis_map_generator_1.8.8_gnau2dr2nxbuk_bqeaiaa
        /// </summary>
        /// <example>neroxis_map_generator_1.8.8_gnau2dr2nxbuk_bqeaiaa</example>
        [JsonPropertyName("mapname")]
        public string Mapname { get => _Mapname; set => Set(ref _Mapname, value); }
        #endregion
        #region MapFilePath
        private string _MapFilePath;
        /// <summary>
        /// 
        /// </summary>
        /// <example>maps/neroxis_map_generator_1.8.8_gnau2dr2nxbuk_bqeaiaa.zip</example>
        [JsonPropertyName("map_file_path")]
        public string MapFilePath { get => _MapFilePath; set => Set(ref _MapFilePath, value); }
        #endregion
        #region Host
        private string _Host;
        [JsonPropertyName("host")]
        public string Host { get => _Host; set => Set(ref _Host, value); } 
        #endregion
        #region NumPlayers
        private int _NumPlayers;
        [JsonPropertyName("num_players")]
        public int NumPlayers { get => _NumPlayers; set => Set(ref _NumPlayers, value); }
        #endregion
        #region MaxPlayers
        private int _MaxPlayers;
        [JsonPropertyName("max_players")]
        public int MaxPlayers { get => _MaxPlayers; set => Set(ref _MaxPlayers, value); }
        #endregion
        #region LaunchedAt
        private double? _LaunchedAt;
        [JsonPropertyName("launched_at")]
        public double? LaunchedAt { get => _LaunchedAt; set => Set(ref _LaunchedAt, value); } 
        #endregion
        [JsonPropertyName("rating_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RatingType RatingType { get; set; }
        [JsonPropertyName("rating_min")]
        public double? RatingMin { get; set; }
        [JsonPropertyName("rating_max")]
        public double? RatingMax { get; set; }
        [JsonPropertyName("enforce_rating_range")]
        public bool EnforceRatingRange { get; set; }
        [JsonPropertyName("teams_ids")]
        public TeamIds[] TeamsIds { get; set; }
        [JsonPropertyName("teams")]
        public Dictionary<int, string[]> Teams { get; set; }
    }
}
