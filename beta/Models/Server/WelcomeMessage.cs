using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class WelcomeMessage : IServerMessage
    {
        public PlayerInfoMessage me { get; set; }
        public int id { get; set; }
        public string login { get; set; }
        public string command { get; set; }
    }
}
