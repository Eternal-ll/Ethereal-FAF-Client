﻿using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class SocialData : Base.ServerMessage
    {
        [JsonPropertyName("autojoin")]
        public string[] autojoin { get; set; }
        public string[] channels { get; set; }
        public int[] friends { get; set; }
        public int[] foes { get; set; }
        public int power { get; set; }
    }
}
