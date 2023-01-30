using Humanizer;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models
{
    public class Game
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("replayAvailable")]
        public bool IsReplayAvailable { get; set; }
        [JsonPropertyName("replayTicks")]
        public long replayTicks { get; set; }
        [JsonPropertyName("replayUrl")] 
        public string ReplayUrl { get; set; }
        [JsonPropertyName("validity")]
        public ApiGameValidatyState Validity { get; set; }
        [JsonPropertyName("victoryCondition")]
        public string VictoryCondition { get; set; }
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }
        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        public TimeSpan FinishedTimeSpan => DateTime.Now - EndTime;
        public string HumanFinishedTimeSpan => FinishedTimeSpan.Humanize();
    }
    public partial class GamePlayerStats
    {
        [JsonPropertyName("afterDeviation")]
        public double AfterDeviation { get; set; }

        [JsonPropertyName("afterMean")]
        public double AfterMean { get; set; }

        [JsonPropertyName("ai")]
        public bool Ai { get; set; }

        [JsonPropertyName("beforeDeviation")]
        public double BeforeDeviation { get; set; }

        [JsonPropertyName("beforeMean")]
        public double BeforeMean { get; set; }

        [JsonPropertyName("color")]
        public long Color { get; set; }

        [JsonPropertyName("faction")]
        public long Faction { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("score")]
        public long Score { get; set; }

        [JsonPropertyName("scoreTime")]
        public DateTimeOffset ScoreTime { get; set; }

        [JsonPropertyName("startSpot")]
        public long StartSpot { get; set; }

        [JsonPropertyName("team")]
        public long Team { get; set; }
        public Game Game { get; set; }
        public ApiPlayerData Player { get; set; }
    }
}
