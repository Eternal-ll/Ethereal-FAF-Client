using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class CoturnServer : Base.ApiUniversalData
    {
        public bool Active => bool.Parse(Attributes["active"]);
        public string Host => Attributes["host"];
        public string Key => Attributes["key"];
        public int Port => int.Parse(Attributes["port"]);
        public string Region => Attributes["region"];
    }
    public partial class DownlordClientConfiguration
    {
        [JsonPropertyName("gitHubRepo")]
        public GitHubRepo GitHubRepo { get; set; }

        [JsonPropertyName("latestRelease")]
        public LatestRelease LatestRelease { get; set; }

        //[JsonPropertyName("recommendedMaps")]
        //public object[] RecommendedMaps { get; set; }

        [JsonPropertyName("endpoints")]
        public Endpoint[] Endpoints { get; set; }
    }

    public partial class Endpoint
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lobby")]
        public Irc Lobby { get; set; }

        [JsonPropertyName("irc")]
        public Irc Irc { get; set; }

        [JsonPropertyName("liveReplay")]
        public Irc LiveReplay { get; set; }

        [JsonPropertyName("api")]
        public Api Api { get; set; }

        [JsonPropertyName("oauth")]
        public Oauth Oauth { get; set; }
    }

    public partial class Api
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
    }

    public partial class Irc
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }

    public partial class Oauth
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("redirectUris")]
        public Uri[] RedirectUris { get; set; }
    }

    public partial class GitHubRepo
    {
        [JsonPropertyName("apiUrl")]
        public Uri ApiUrl { get; set; }
    }

    public partial class LatestRelease
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("minimumVersion")]
        public string MinimumVersion { get; set; }

        [JsonPropertyName("windowsUrl")]
        public Uri WindowsUrl { get; set; }

        [JsonPropertyName("linuxUrl")]
        public Uri LinuxUrl { get; set; }

        [JsonPropertyName("releaseNotesUrl")]
        public Uri ReleaseNotesUrl { get; set; }

        [JsonPropertyName("mandatory")]
        public bool Mandatory { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
