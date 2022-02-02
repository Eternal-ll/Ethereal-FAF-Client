using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using beta.Models.Server.Base;

namespace beta.Models
{
    public class SimpleTcpClient : IDisposable
    {
        public event EventHandler<string> DelimiterDataReceived;
        //public event EventHandler<Message> DataReceived;

        public SimpleTcpClient()
        {
            ReadLoopIntervalMs = 10;
            Delimiter = 10;
        }
        private Thread _rxThread;

        private readonly Encoding StringEncoder = Encoding.UTF8;
        private readonly List<byte> _queuedMsg = new();

        public byte Delimiter { get; set; }


        internal bool IsListening { get; set; }
        internal int ReadLoopIntervalMs { get; set; }

        public SimpleTcpClient Connect(string hostNameOrIpAddress, int port)
        {
            if (string.IsNullOrEmpty(hostNameOrIpAddress))
            {
                throw new ArgumentNullException("hostNameOrIpAddress");
            }

            TcpClient = new TcpClient(AddressFamily.InterNetwork);
            TcpClient.Connect(hostNameOrIpAddress, port);
            StartRxThread();

            return this;
        }

        private void StartRxThread()
        {
            if (_rxThread != null) { return; }

            _rxThread = new Thread(ListenerLoop)
            {
                Name = "TCP Client",
                IsBackground = true
            };
            _rxThread.Start();
        }

        public SimpleTcpClient Disconnect()
        {
            if (TcpClient == null) { return this; }
            TcpClient.Close();
            TcpClient = null;
            return this;
        }

        public TcpClient TcpClient { get; private set; }

        private void ListenerLoop(object state)
        {
            while (!IsListening)
            {
                RunLoopStep();

                Thread.Sleep(ReadLoopIntervalMs);
            }

            _rxThread = null;
        }
        private void RunLoopStep()
        {
            if (TcpClient == null) return;
            if (TcpClient.Connected == false) return;

            var c = TcpClient;

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
            {
                Thread.Sleep(10);
                return;
            }
            while (bytesAvailable > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                c.Client.Receive(nextByte, 0, 1, SocketFlags.None);

                //              == \n
                if (nextByte[0] == Delimiter)
                {
                    NotifyDelimiterMessageRx(c, _queuedMsg);
                    _queuedMsg.Clear();
                }
                else _queuedMsg.Add(nextByte[0]);
            }
        }

        private readonly StringBuilder builder = new();
        private void NotifyDelimiterMessageRx(TcpClient client, List<byte> msg)
        {
            DelimiterDataReceived.Invoke(this, builder
                .Clear()
                .Append(StringEncoder.GetChars(msg.ToArray()))
                .ToString());
        }
        public void Write(byte[] data)
        {
            //if (_client == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
            TcpClient.GetStream().Write(data, 0, data.Length);
        }

        public void Write(string data)
        {
            if (data == null) { return; }
            Write(StringEncoder.GetBytes(data));
        }

        public void WriteLine(string data)
        {
            if (data.Length == 0) return;

            if (data[^1] != '\n')
                Write(data + '\n');
            else Write(data);
        }

        //public Message WriteLineAndGetReply(string data, TimeSpan timeout)
        //{
        //    Message mReply = null;
        //    //DataReceived += (s, e) => { mReply = e; };
        //    WriteLine(data);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    while (mReply == null && sw.Elapsed < timeout)
        //    {
        //        Thread.Sleep(10);
        //    }

        //    return mReply;
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                IsListening = true;
                if (TcpClient != null)
                {
                    try
                    {
                        TcpClient.Close();
                    }
                    catch { }
                    TcpClient = null;
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SimpleTcpClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class SSLClient : IDisposable
    {
        public event EventHandler<string> DelimiterDataReceived;

        public SSLClient()
        {
            ReadLoopIntervalMs = 10;
        }
        private Thread _rxThread;

        private readonly Encoding StringEncoder = Encoding.UTF8;
        private readonly List<byte> _queuedMsg = new();
        private TcpClient _client;

        internal bool QueueStop { get; set; }
        internal int ReadLoopIntervalMs { get; set; }

        public SSLClient Connect(string hostNameOrIpAddress, int port)
        {
            //if (string.IsNullOrEmpty(hostNameOrIpAddress))
            //{
            //    throw new ArgumentNullException("hostNameOrIpAddress");
            //}

            //_client = new TcpClient(AddressFamily.InterNetwork);
            //_client.Connect(hostNameOrIpAddress, port);

            //SslStream = new SslStream(_client.GetStream(),false,
            //    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            //SslStream.AuthenticateAsClient("irc.faforever.com");

            //StartRxThread();

            return this;
        }
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void StartRxThread()
        {
            if (_rxThread != null) { return; }

            _rxThread = new Thread(ListenerLoop)
            {
                IsBackground = true
            };
            _rxThread.Start();
        }

        public SSLClient Disconnect()
        {
            if (_client == null) { return this; }
            _client.Close();
            _client = null;
            return this;
        }

        public TcpClient TcpClient => _client;

        private void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                RunLoopStep();

                Thread.Sleep(ReadLoopIntervalMs);
            }

            _rxThread = null;
        }

        private SslStream SslStream;
        private void RunLoopStep()
        {
            if (_client == null) return;
            if (_client.Connected == false) return;

            var c = _client;

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
            {
                Thread.Sleep(10);
                return;
            }
            //List<byte> bytesReceived = new List<byte
            while (c.Available > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                SslStream.Read(nextByte, 0, 1);

                //bytesReceived.AddRange(nextByte);

                //              == \n
                if (nextByte[0] == 10)
                {
                    NotifyDelimiterMessageRx(c, _queuedMsg);
                    _queuedMsg.Clear();
                }
                else
                {
                    _queuedMsg.AddRange(nextByte);
                }
            }

            //if (bytesReceived.Count > 0)
            //{
            //    //NotifyEndTransmissionRx(c, bytesReceived.ToArray());
            //}
        }

        private readonly StringBuilder builder = new();
        private void NotifyDelimiterMessageRx(TcpClient client, List<byte> msg)
        {
            DelimiterDataReceived.Invoke(this, builder
                .Clear()
                .Append(StringEncoder.GetChars(msg.ToArray()))
                .ToString());
        }

        //private void NotifyEndTransmissionRx(TcpClient client, byte[] msg)
        //{
        //    if (DataReceived != null)
        //    {
        //        Message m = new Message(msg, client, StringEncoder, Delimiter, AutoTrimStrings);
        //        DataReceived(this, m);
        //    }
        //}

        public void Write(byte[] data)
        {
            //if (_client == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
            if (!_client.Connected)
            {

                _client.Connect("116.202.155.226", 6697);
                SslStream = new SslStream(_client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                SslStream.AuthenticateAsClient("irc.faforever.com");
            }

            while (!_client.Connected)
            {

            }
            SslStream.Write(data, 0, data.Length);
        }

        public void Write(string data)
        {
            if (data == null) { return; }
            Write(StringEncoder.GetBytes(data));
        }

        public void WriteLine(string data)
        {
            if (data.Length == 0) return;

            if (data[^1] != '\n')
                Write(data + '\n');
            else Write(data);
        }

        //public Message WriteLineAndGetReply(string data, TimeSpan timeout)
        //{
        //    Message mReply = null;
        //    //DataReceived += (s, e) => { mReply = e; };
        //    WriteLine(data);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    while (mReply == null && sw.Elapsed < timeout)
        //    {
        //        Thread.Sleep(10);
        //    }

        //    return mReply;
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                QueueStop = true;
                if (_client != null)
                {
                    try
                    {
                        _client.Close();
                    }
                    catch { }
                    _client = null;
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SimpleTcpClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}