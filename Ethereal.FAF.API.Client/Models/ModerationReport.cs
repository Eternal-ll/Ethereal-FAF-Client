using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ModerationReport
    {
        [JsonPropertyName("gameIncidentTimecode")]
        public object GameIncidentTimecode { get; set; }
        [JsonPropertyName("moderatorNotice")]
        public object ModeratorNotice { get; set; }
        [JsonPropertyName("reportDescription")]
        public string ReportDescription { get; set; }
        [JsonPropertyName("reportStatus")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Enums.ReportStatus ReportStatus { get; set; }
        [JsonPropertyName("createTime")]
        public DateTimeOffset CreateTime { get; set; }
        [JsonPropertyName("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }

        public List<ApiPlayerData> ReportedPlayers { get; set; }
        //public ApiGame Game { get; set; }
    }
}
