using Ethereal.FAF.UI.Client.ViewModels.Base;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models
{
	public class CoturnServer : ViewModel
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("active")]
		public bool Active { get; set; }

		[JsonPropertyName("credential")]
		public string Credential { get; set; }

		[JsonPropertyName("credentialType")]
		public string CredentialType { get; set; }

		[JsonPropertyName("region")]
		public string Region { get; set; }

		[JsonPropertyName("urls")]
		public string[] Urls { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; }

		private long _RoundtripTime;
		public long RoundtripTime
		{
			get => _RoundtripTime;
			set
			{
				if (Set(ref _RoundtripTime, value)) OnPropertyChanged(nameof(Progress));
			}
		}
		public double Progress => RoundtripTime * 0.005 * 100;
	}
}
