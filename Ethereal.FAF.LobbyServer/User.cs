using System.Net.Sockets;

namespace Ethereal.FAF.LobbyServer
{
    public class User
    {
        private readonly TcpClient Client;


        public string Login { get; set; }
        public bool IsAuthentificated { get; set; }
        
        public User(TcpClient client)
        {
            Client = client;
        }
    }
}
