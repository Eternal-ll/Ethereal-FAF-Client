using beta.Models.Server.Base;

namespace beta.Views
{
    public partial class MainView
    {
        public class IRCPasswordMessage : IServerMessage
        {
            public string command { get; set; }
            public string password { get; set; }
        }
    }
}
