using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class NoticeMessage : IServerMessage
    {
        public string command { get; set; }
        public string style { get; set; }
        public string text { get; set; }
    }
}
