using beta.Models.Server.Base;
using beta.Models.Server.Enums;

namespace beta.Models.Server
{
    public class NoticeMessage : IServerMessage
    {
        public ServerCommand Command { get; set; }

        public string style { get; set; }
        public string text { get; set; }
    }
}
