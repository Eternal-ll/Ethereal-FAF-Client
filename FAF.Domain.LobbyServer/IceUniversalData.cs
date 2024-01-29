using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	/*
    
    {
        "command":"JoinGame",
        "args":["Ninrai", 66531],
        "target":"game"
    }

    */
	public class IceUniversalData : Base.ServerMessage
    {
        [JsonConverter(typeof(RawStringConverter))]
        public string args { get; set; }
        public string target { get; set; }
    }
    public class IceUniversalData2 : Base.ServerMessage
    {
        public List<object> args { get; set; }
        public string target { get; set; }
    }
}
