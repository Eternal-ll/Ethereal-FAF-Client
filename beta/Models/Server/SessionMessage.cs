using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class SessionMessage : IServerMessage
    {
        public string command { get; set; }
        public long session { get; set; }
    }
}
