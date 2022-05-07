using beta.Models.Debugger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Models
{
    /// <summary>
    /// This is a simple class that takes all the FA replay data input from
    /// its inputSocket, writes it to a file, and relays it to an internet
    /// server via its relaySocket.
    /// </summary>
    public class ReplayRecorder
    {
        public event EventHandler StreamFinished;

        private readonly TcpClient TcpClient;
        private Socket RelaySocket;

        public Process Game;

        public ReplayRecorder(TcpClient tcpClient) => TcpClient = tcpClient;
        public void Initialize(bool relayToFAF = true)
        {
            if (Game is null)
            {
                throw new ArgumentNullException(nameof(Game));
            }
            if (relayToFAF)
            {
                try
                {
                    RelaySocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    RelaySocket.Connect("lobby.faforever.com", 15000);
                }
                catch (Exception ex)
                {
                    RelaySocket = null;
                }
            }

            Task.Run(() =>
            {
                var game = Game;
                var relay = RelaySocket;
                var client = TcpClient;
                var stream = client.GetStream();
                List<byte> cache = new();
                byte[] last = Array.Empty<byte>();
                while (client.Connected && !game.HasExited)
                {
                    if (client.Available == 0)
                    {
                        if (last.Length > 0)
                        {
                            if (RelaySocket is not null && RelaySocket.Connected)
                            {
                                RelaySocket.Send(last);
                            }
                            cache.AddRange(last);
                            //var data = Encoding.ASCII.GetString(last);
                            //AppDebugger.LOGReplayOutput(data);
                            last = Array.Empty<byte>(); 
                        }
                        Thread.Sleep(50);
                        continue;
                    }
                    last = new byte[client.Available];
                    stream.Read(last, 0, last.Length);
                }
                StreamFinished?.Invoke(this, null);
                stream.Dispose();
                client.Close();
            });
        }
    }
}
