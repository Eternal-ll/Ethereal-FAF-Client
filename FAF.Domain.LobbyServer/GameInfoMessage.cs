using FAF.Domain.LobbyServer.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class GameInfoMessage
    {
        public ServerCommand Command { get; set; }
        public GameInfoMessage[] games { get; set; }
        [JsonPropertyName("visibility")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameVisibility Visibility { get; set; }
        public bool password_protected { get; set; }
        public long uid { get; set; }
        public string title { get; set; }
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameState State{ get; set; }
        [JsonPropertyName("game_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        [JsonPropertyName("featured_mod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod FeaturedMod { get; set; }
        public Dictionary<string, string> sim_mods { get; set; }
        public string mapname { get; set; }
        public string map_file_path { get; set; }
        public string host { get; set; }
        public int num_players { get; set; }
        public int max_players { get; set; }
        public double? launched_at { get; set; }
        [JsonPropertyName("rating_type")]
        public string RatingType { get; set; }
        public double? rating_min { get; set; }
        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }
        public Dictionary<int, string[]> teams { get; set; }
    }
}
