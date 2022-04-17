﻿using beta.Infrastructure.Extensions;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace beta.Models
{
    public enum ManagedTcpClientState : byte
    {
        Disconnected,
        TimedOut,
        CantConnect,
        PendingConnection,
        Connected
    }

    public class ManagedTcpClient : IDisposable
    {
        #region Events
        public event EventHandler<string> DataReceived;
        public event EventHandler<ManagedTcpClientState> StateChanged;
        #endregion

        #region CTOR
        public ManagedTcpClient()
        {

        }

        /// <summary>
        /// Using non SSL port by default
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public ManagedTcpClient(string host = "lobby.faforever.com", int port = 6667)
        {
            Host = host;
            Port = port;
            TcpThread = new(DoConnect);
            TcpThread.Start();
        }
        public ManagedTcpClient(string threadName, string host = "lobby.faforever.com", int port = 6667)
            : this(host, port) => ThreadName = threadName;
        #endregion

        #region Properties

        #region Public
        public bool IsListening { get; set; }
        /// <summary>
        /// 10ms by default
        /// </summary>
        public int ReadLoopIntervalMs { get; set; } = 10;
        public string ThreadName { get; set; } = "TCP Client";
        #endregion

        #region Private

        private readonly Encoding StringEncoder = Encoding.UTF8;
        private readonly List<byte> ByteCache = new();

        public TcpClient TcpClient;
        public int Port;
        public string Host;

        private NetworkStream Stream;
        private Thread TcpThread;
        #endregion

        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Connect to the IRC server
        /// </summary>
        public void Connect()
        {
            OnStateChanged(ManagedTcpClientState.PendingConnection);
            TcpThread = new(DoConnect)
            {
                Name = ThreadName,
                IsBackground = true,
            };
            TcpThread.Start();
        }
        private bool IsDisconnected = false;
        public bool Write(byte[] data)
        {
            if (!IsDisconnected)
            {
                if (TcpClient is null)
                {
                    IsDisconnected = true;
                    OnStateChanged(ManagedTcpClientState.Disconnected);
                    return false;
                    //throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)");
                }
                else if (!TcpClient.Connected)
                {
                    IsDisconnected = true;
                    OnStateChanged(ManagedTcpClientState.Disconnected);
                    return false;
                }
            }
            if (Stream is not null && !IsDisposed)
            {
                IsDisconnected = false;
                Stream.Write(data, 0, data.Length);
                return true;
            }
            return false;
        }

        public bool Write(string data) => Write(StringEncoder.GetBytes(data + '\n'));

        public string WriteLineAndGetReply(string data, ServerCommand command, TimeSpan timeout)
        {
            string reply = string.Empty;

            DataReceived += (s, e) =>
            {
                // parsing first row
                // "command": "....",
                if (Enum.TryParse<ServerCommand>(e.GetRequiredJsonRowValue(), out var repylCommand))
                    if (repylCommand == command)
                        reply = e;
                DataReceived -= (s, e) => { };
            };

            Write(data);

            Stopwatch sw = new();
            sw.Start();

            while (reply.Length == 0 && sw.Elapsed < timeout)
                Thread.Sleep(10);

            return reply;
        }
        public string WriteLineAndGetReply(byte[] data, ServerCommand command, TimeSpan timeout)
        {
            string reply = string.Empty;

            DataReceived += (s, e) =>
            {
                // parsing first row
                // "command": "....",
                if (Enum.TryParse<ServerCommand>(e.GetRequiredJsonRowValue(), out var repylCommand))
                    if (repylCommand == command)
                        reply = e;
                DataReceived -= (s, e) => { };
            };

            Write(data);

            Stopwatch sw = new();
            sw.Start();

            while (reply.Length == 0 && sw.Elapsed < timeout)
                Thread.Sleep(10);

            return reply;
        }
        #endregion

        private void DoConnect()
        {
            OnStateChanged(ManagedTcpClientState.PendingConnection);
            try
            {
                TcpClient = new(Host, Port);

                Stream = TcpClient.GetStream();
                OnStateChanged(ManagedTcpClientState.Connected);

                try
                {
                    while (!IsListening)
                    {
                        RunLoopStep();

                        Thread.Sleep(ReadLoopIntervalMs);
                    }
                }
                catch(SocketException ex)
                {

                }
            }
            catch (SocketException e)
            {
                OnStateChanged(ManagedTcpClientState.CantConnect);
                TcpThread = null;
            }
            finally
            {
                OnStateChanged(ManagedTcpClientState.Disconnected);
                if (TcpClient is not null)
                {
                    //Stream.Dispose();
                    TcpClient.Close();
                }
                TcpThread = null;
            }
        }

        #region Private
        private void StartListenThread()
        {
            if (TcpThread is not null) { return; }

            TcpThread = new Thread(ListenerLoop)
            {
                Name = ThreadName,
                IsBackground = true
            };

            TcpThread.Start();
        }
        private void ListenerLoop(object state)
        {
            while (!IsListening)
            {
                RunLoopStep();

                Thread.Sleep(ReadLoopIntervalMs);
            }
        }

        private void RunLoopStep()
        {
            var c = TcpClient;

            if (c is null) return;
            if (!c.Connected) return;

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
            {
                Thread.Sleep(10);
                return;
            }
            var cacheSB = ByteCache;
            while (bytesAvailable > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                c.Client.Receive(nextByte, 0, 1, SocketFlags.None);

                // \r = 13
                // \n = 10
                if (nextByte[0] == 10)
                {
                    OnDataReceived(StringEncoder.GetString(cacheSB.ToArray()));
                    cacheSB.Clear();
                }
                else cacheSB.Add(nextByte[0]);
            }
        }

        private void OnStateChanged(ManagedTcpClientState state) => StateChanged?.Invoke(this, state);
        private void OnDataReceived(string data) => DataReceived?.Invoke(this, data);

        #endregion


        #endregion

        #region Dispose
        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed) return;
            IsListening = true;
            TcpClient.Close();
            OnStateChanged(ManagedTcpClientState.Disconnected);
            IsDisposed = true;
        }
        #endregion
    }
}