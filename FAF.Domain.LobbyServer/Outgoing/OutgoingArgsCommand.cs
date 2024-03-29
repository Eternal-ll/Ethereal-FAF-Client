﻿using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    public class OutgoingArgsCommand : OutgoingCommand
    {
        public OutgoingArgsCommand(string command, params object[] args) : base(command)
        {
            Args = args;
        }
        [JsonPropertyName("target")]
        public string Target { get; } = "game";
        [JsonPropertyName("args")]
        public object[] Args { get; }
    }
}
