using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class PlayerInfoMessage : INPC
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("avatar")]
        public PlayerAvatar Avatar { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("clan")]
        public string Clan { get; set; }

        [JsonPropertyName("ratings")]
        public Ratings Ratings { get; set; }

        //[JsonPropertyName("global_rating")]
        //public double[] GlobalRating { get; set; }

        //[JsonPropertyName("ladder_rating")]
        //public double[] LadderRating { get; set; }

        [JsonPropertyName("number_of_games")]
        public long NumberOfGames { get; set; }

        [JsonPropertyName("players")]
        public PlayerInfoMessage[] Players { get; set; }

        [JsonIgnore]
        public GameInfoMessage Game { get; set; }

    }
}
