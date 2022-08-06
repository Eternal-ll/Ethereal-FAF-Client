using FAF.Domain.LobbyServer.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{

    public class PlayerInfoMessage
    {
        public ServerCommand Command { get; set; }
        public string command { get; set; }
        public int id { get; set; }
        public string login { get; set; }
        public string country { get; set; }
        public string clan { get; set; }
        public Dictionary<string, Rating> ratings { get; set; }
        [JsonPropertyName("avatar")]
        public PlayerAvatar Avatar { get; set; }
        public PlayerInfoMessage[] players { get; set; }
    }
}
