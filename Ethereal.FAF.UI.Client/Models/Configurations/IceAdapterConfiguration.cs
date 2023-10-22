using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Configurations
{
	internal class IceAdapterConfiguration
	{
		[JsonPropertyName("Executable")]
		public string Executable { get; set; }

		[JsonPropertyName("Telemetry")]
		public string Telemetry { get; set; }

		[JsonPropertyName("Logs")]
		public string Logs { get; set; }

		[JsonPropertyName("LogLevel")]
		public string LogLevel { get; set; }

		[JsonPropertyName("DelayUI")]
		public int DelayUi { get; set; }

		[JsonPropertyName("IsLogsEnabled")]
		public bool IsLogsEnabled { get; set; }

		[JsonPropertyName("IsDebugEnabled")]
		public bool IsDebugEnabled { get; set; }

		[JsonPropertyName("IsInfoEnabled")]
		public bool IsInfoEnabled { get; set; }
		[JsonPropertyName("SelectedCoturnServers")]
		public int[] SelectedCoturnServers { get; set; }
	}
}
