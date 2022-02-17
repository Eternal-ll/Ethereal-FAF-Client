using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace beta.Models
{
    public enum TcpClientState
    {
        
    }

    public class ManagedTcpClient : IDisposable
    {
        #region Events
        public event EventHandler<string> DataReceived;
        public event EventHandler<TcpClientState> StateChanged; 
        #endregion

        #region Properties

        private readonly Encoding StringEncoder = Encoding.UTF8;
        private readonly List<byte> QueueCache = new();

        private Thread ListenThread;

        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// Delimeter for incoming data. Default is '\n'
        /// </summary>
        public byte Delimiter { get; set; }
        public char CharDelimeter
        {
            get => Convert.ToChar(Delimiter);
            set => Delimiter = Convert.ToByte(value);
        }

        public bool IsListening { get; set; }

        public int ReadLoopIntervalMs { get; set; }

        /// <summary>
        /// Call the thread as you want
        /// </summary>
        public string ThreadName { get; set; } = "TCP Client";
        #endregion

        public ManagedTcpClient()
        {
            ReadLoopIntervalMs = 10;
            Delimiter = 10;
        }

        public ManagedTcpClient Connect(string hostNameOrIpAddress, int port)
        {
            TcpClient = new TcpClient(AddressFamily.InterNetwork);

            try
            {
                TcpClient.Connect(hostNameOrIpAddress, port);
            }
            catch (Exception e)
            {
                throw new Exception(nameof(ManagedTcpClient), e);
            }
            finally
            {
                StartListenThread();
            }
            return this;
        }
        public ManagedTcpClient Disconnect()
        {
            if (TcpClient == null) { return this; }
            TcpClient.Close();
            TcpClient = null;
            return this;
        }

        private void StartListenThread()
        {
            if (ListenThread != null) { return; }

            ListenThread = new Thread(ListenerLoop)
            {
                Name = ThreadName,
                IsBackground = true
            };

            ListenThread.Start();
        }
        private void ListenerLoop(object state)
        {
            //try
            //{
                while (!IsListening)
                {
                    RunLoopStep();

                    Thread.Sleep(ReadLoopIntervalMs);
                }
            //}
            //catch (Exception e)
            //{
            //    throw new Exception(ThreadName, e);
            //}
            //finally
            //{
                ListenThread = null;
            //}
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
                    NotifyDelimiterMessageRx(c, QueueCache);
                    QueueCache.Clear();
                }
                else QueueCache.Add(nextByte[0]);
            }
        }

        private void NotifyDelimiterMessageRx(TcpClient client, List<byte> msg) => DataReceived.Invoke(this, StringEncoder.GetString(msg.ToArray()));
        
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
                    TcpClient.Close();
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
}