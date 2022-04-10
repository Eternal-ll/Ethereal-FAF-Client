using beta.Models.Server.Base;
using beta.Models.Server.Enums;

namespace beta.Models.Server
{
    public class NotificationData : ServerMessage
    {
        public string style { get; set; }
        public string text { get; set; }
    }
}
