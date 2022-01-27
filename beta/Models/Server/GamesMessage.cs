using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class GamesMessage : IServerMessage
    {
        public string command { get; set; }
        public GameInfoMessage[] games { get; set; }
    }
}
