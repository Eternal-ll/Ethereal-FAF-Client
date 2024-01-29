using FAF.Domain.LobbyServer.Enums;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public enum InitMode : int
    {
        Normal,
        Auto
    }
    public class GameLaunchData : Base.ServerMessage
    {
        // Custom game
        // {"command":"game_launch","args":["/numgames",788],"uid":19113672,"mod":"faf","name":"noobs <300 games","init_mode":0,"game_type":"custom","rating_type":"global"}
        /// <summary>
        /// 
        /// </summary>
        /// <example>["/numgames",788]</example>
        [JsonPropertyName("args")]
        public object[] args { get; set; }
        /// <summary>
        /// Game uid
        /// </summary>
        /// <example>19113672</example>
        [JsonPropertyName("uid")]
        public long GameUid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <example>faf</example>
        [JsonPropertyName("mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }
        [JsonPropertyName("init_mode")]
        public InitMode InitMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <example>Example game title</example>
        [JsonPropertyName("name")]
        public string GameTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <example>custom</example>
        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rating_type")]
        public string RatingType { get; set; }


        public int team { get; set; }
        [JsonConverter(typeof (JsonStringEnumConverter))]
        public Faction faction { get; set; }
        public int expected_players { get; set; }
        public int map_position { get; set; }
        public string mapname { get; set; }
        [JsonConverter(typeof(RawStringConverter))]
        public string game_options { get; set; }
    }
}
