using System.Collections.Generic;

namespace FAF.Domain.LobbyServer
{
    public class IceUniversalData2 : Base.ServerMessage
    {
        public List<object> args { get; set; }
        public string target { get; set; }
    }
}
