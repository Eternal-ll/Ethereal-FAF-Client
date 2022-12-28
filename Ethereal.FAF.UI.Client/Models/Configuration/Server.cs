using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Configuration
{
    public class ClientServers
    {
        IDisposable OptionsDisposable;
        public ClientServers(IOptionsMonitor<ClientServers> optionsMonitor)
        {
            OptionsDisposable = optionsMonitor.OnChange((servers, data) =>
            {

            });
        }
        public ObservableCollection<Server> Servers { get; set; }
    }
    public partial class Server : ViewModels.Base.ViewModel
    {
        [JsonPropertyName("ShortName")]
        public string ShortName { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Logo")]
        public Uri Logo { get; set; }
        [JsonPropertyName("Site")]
        public Uri Site { get; set; }

        [JsonPropertyName("IsVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("Lobby")]
        public ServerAddress Lobby { get; set; }

        [JsonPropertyName("Replay")]
        public ServerAddress Replay { get; set; }

        [JsonPropertyName("Relay")]
        public ServerAddress Relay { get; set; }

        [JsonPropertyName("IRC")]
        public ServerAddress Irc { get; set; }

        [JsonPropertyName("API")]
        public Uri Api { get; set; }

        [JsonPropertyName("Content")]
        public Uri Content { get; set; }

        [JsonPropertyName("OAuth")]
        public OAuth OAuth { get; set; }

        [JsonPropertyName("Cloudfare")]
        public Cloudfare Cloudfare { get; set; }
        [JsonIgnore]
        public int _PlayersCount;
        public int PlayersCount { get => _PlayersCount; private set => Set(ref _PlayersCount, value); }
        public void SetPlayersCount(int count) => PlayersCount = count;
        [JsonIgnore]
        private int _GamesCount;
        public int GamesCount { get => _GamesCount; private set => Set(ref _GamesCount, value); }
        public void SetGamesCount(int count) => GamesCount = count;
    }

    public partial class Cloudfare
    {
        [JsonPropertyName("HMAC")]
        public Hmac Hmac { get; set; }
    }

    public partial class Hmac
    {
        [JsonPropertyName("Secret")]
        public string Secret { get; set; }

        [JsonPropertyName("Param")]
        public string Param { get; set; }
    }

    public class ServerAddress
    {
        [JsonPropertyName("Host")]
        public string Host { get; set; }

        [JsonPropertyName("Port")]
        public int Port { get; set; }
    }

    public partial class OAuth
    {
        [JsonPropertyName("BaseAddress")]
        public Uri BaseAddress { get; set; }

        [JsonPropertyName("ResponseSeconds")]
        public long ResponseSeconds { get; set; }

        [JsonPropertyName("ClientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("Scope")]
        public string Scope { get; set; }

        [JsonPropertyName("RedirectPorts")]
        public long[] RedirectPorts { get; set; }
    }
}
