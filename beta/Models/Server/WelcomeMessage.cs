using beta.Models.Server.Base;
using beta.Models.Server.Enums;

namespace beta.Models.Server
{
    public class WelcomeMessage : IServerMessage
    {
        public ServerCommand Command { get; set; }

        public PlayerInfoMessage me { get; set; }
        public int id { get; set; }
        public string login { get; set; }
        public string command { get; set; }
    }
}
