using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace beta.Models
{
    public class ConnectionHandler
    {
        public event EventHandler<string> Response;
        public readonly TcpClient Client;
        private readonly NetworkStream _Stream;
        private readonly Thread _ListenerThread;
        
        protected virtual void OnResponse(string e) => Response?.Invoke(this, e);
        public ConnectionHandler(TcpClient tcpClient)
        {
            Client = tcpClient;
            _Stream = tcpClient.GetStream();
            _ListenerThread = new Thread(Listen);
            _ListenerThread.Start();
        }

        private void Listen()
        {
            StringBuilder completeMessage = new();
            while (true)
            {
                Byte[] readingData = new Byte[Client.ReceiveBufferSize];
                String responseData = String.Empty;
                int numberOfBytesRead = 0;
                do
                {
                    numberOfBytesRead = _Stream.Read(readingData, 0, readingData.Length);
                    var t = Encoding.UTF8.GetString(readingData, 0, numberOfBytesRead);
                    t = t.Replace("\0", string.Empty);
                    var g = t.Split("command");
                    var len = g.Length - 2;

                    string part = g[1];
                    part = part.Substring(3);
                    string command = part.Substring(0, part.IndexOf("\""));
                    part = part.Substring(3, part.IndexOf("\""));
                    
                    var gg = 1;
                    //":"notice"
                    //if (len >= 0)
                    //{
                    //    var gwg = g[g.Length-1];
                    //    var i = 0;
                    //    while(i <= len)
                    //    {
                    //        OnResponse(g[i]);
                    //        i++;
                    //    }
                    //    if(!string.IsNullOrWhiteSpace(gwg))
                    //        completeMessage.Append(gwg);
                    //}
                    //else
                    //{
                    //    completeMessage.Append(t);
                    //}
                    //var vs = 1;
                }
                while (_Stream.DataAvailable);
                //var h = completeMessage.ToString();
                //if(!h.Contains('\n'))
                //{
                        
                //}
                //else
                //{
                //    var gw = h.Split('\n');
                //        if(!string.IsNullOrWhiteSpace(completeMessage.ToString()))
                //        OnResponse(completeMessage.ToString());
                //    completeMessage.Clear();
                //}
                //responseData = completeMessage.ToString();
                //var gt = 1;
                //if (_Stream.CanRead) {
                //    byte[] readBuffer = new byte[Client.ReceiveBufferSize];
                //    string fullServerReply = null;
                //    using (var writer = new MemoryStream())
                //    {
                //        while (_Stream.DataAvailable)
                //        {
                //            int numberOfBytesRead = _Stream.Read(readBuffer, 0, readBuffer.Length);
                //            if (numberOfBytesRead <= 0)
                //            {
                //                break;
                //            }
                //            writer.Write(readBuffer, 0, numberOfBytesRead);
                //        }
                //        fullServerReply = Encoding.UTF8.GetString(writer.ToArray());
                //        if (fullServerReply.Length > 0)
                //        {
                //            OnResponse(fullServerReply);
                //        }
                //    }
                //}
                //int bufferSize = Client.ReceiveBufferSize;
                //var inStream = new byte[bufferSize];

                //_Stream.Read(inStream, 0, bufferSize);

                //string data = Encoding.UTF8.GetString(inStream);
                //if (data.Length > 0 && !_Stream.DataAvailable) OnResponse(data);


            }
        }
        
        public void SendMessage(byte[] bytes) => _Stream.Write(bytes, 0, bytes.Length);
        public void SendMessage(string message) => SendMessage(Encoding.UTF8.GetBytes(message));

    }
}
