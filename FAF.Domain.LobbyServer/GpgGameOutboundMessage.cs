using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    /// <summary>
    /// Gpg outbound messages are message sent from the local running game itself and will be sent via ICE to others.
    /// They are distinguishable from the [target] property.
    /// You will also notice, that the command uses camel case, where the rest of the protocol uses snake case.
    /// </summary>
    public class GpgGameOutboundMessage : Base.ServerMessage
    {
        [JsonPropertyName("target")]
        public string Target { get; set; } = "game";
        [JsonPropertyName("args")]
        public List<object> Args { get; set; }
        //public class disconnectedMessage : GpgGameOutboundMessage
        //{
        //    public disconnectedMessage()
        //    {
        //        Command = discon
        //    }
        //}
        //public class connectedMessage : GpgGameOutboundMessage
        //{
        //}
        //public class gameStateMessage : GpgGameOutboundMessage
        //{
        //}
        //public class gameOptionMessage : GpgGameOutboundMessage
        //{
        //}
        //public class gameModsMessage : GpgGameOutboundMessage
        //{
        //}
        //public class playerOptionMessage : GpgGameOutboundMessage
        //{
        //}
        //public class disconnectFromPeerMessage : GpgGameOutboundMessage
        //{
        //}
        //public class chatMessage : GpgGameOutboundMessage
        //{
        //}
        //public class gameResultMessage : GpgGameOutboundMessage
        //{
        //}
        //public class statsMessage : GpgGameOutboundMessage
        //{
        //}
        //public class clearSlotsMessage : GpgGameOutboundMessage
        //{
        //}
        //public class aiOptionMessage : GpgGameOutboundMessage
        //{
        //}
        //public class jsonStatsMessage : GpgGameOutboundMessage
        //{
        //}
        //public class rehostMessage : GpgGameOutboundMessage
        //{
        //}
        //public class desyncMessage : GpgGameOutboundMessage
        //{
        //}
        //public class gameFullMessage : GpgGameOutboundMessage
        //{
        //}
        //public class iceMessage : GpgGameOutboundMessage
        //{
        //}
    }
}
