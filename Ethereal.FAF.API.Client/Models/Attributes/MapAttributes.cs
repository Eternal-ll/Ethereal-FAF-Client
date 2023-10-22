using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Attributes
{
	public class MapAttributes
	{
		[JsonPropertyName("battleType")]
		public string BattleType { get; set; }

		[JsonPropertyName("createTime")]
		public DateTimeOffset CreateTime { get; set; }

		[JsonPropertyName("displayName")]
		public string DisplayName { get; set; }

		[JsonPropertyName("gamesPlayed")]
		public int GamesPlayed { get; set; }

		[JsonPropertyName("mapType")]
		public string MapType { get; set; }

		[JsonPropertyName("recommended")]
		public bool Recommended { get; set; }

		[JsonPropertyName("updateTime")]
		public DateTimeOffset UpdateTime { get; set; }
	}
}
