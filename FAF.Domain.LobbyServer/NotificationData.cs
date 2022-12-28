using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;

namespace FAF.Domain.LobbyServer
{
    public class NotificationData : ServerMessage
    {
        public string style { get; set; }
        public string text { get; set; }
    }
}
