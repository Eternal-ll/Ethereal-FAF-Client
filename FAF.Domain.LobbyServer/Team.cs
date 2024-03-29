﻿using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class TeamIds
    {
        [JsonPropertyName("team_id")]
        public int TeamId { get; set; }

        [JsonPropertyName("player_ids")]
        public long[] PlayerIds { get; set; }
    }
}
