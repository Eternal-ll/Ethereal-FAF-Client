using Ethereal.FAF.UI.Client.Infrastructure.Ice.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using TcpClient = NetCoreServer.TcpClient;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    /// <summary>
    /// Wrapper for https://github.com/FAForever/java-ice-adapter
    /// </summary>
    public class IceClient : TcpClient
    {
        /// <summary>
        /// The game sent a message to the faf-ice-adapter via the internal GPGNetServer.
        /// </summary>
        public event EventHandler<GpgNetMessage> GpgNetMessageReceived;
        /// <summary>
        /// The game connected to the internal GPGNetServer.
        /// </summary>
        public event EventHandler<bool> ConnectionToGpgNetServerChanged;
        /// <summary>
        /// The PeerRelays gathered a local ICE message for connecting to the remote player.
        /// This message must be forwarded to the remote peer and set using the iceMsg command.
        /// </summary>
        //public event EventHandler<IceMessage> IceMessageReceived;
        public event EventHandler<string> IceMessageReceived;
        /// <summary>
        /// Informs the client that ICE connectivity to the peer is established or unestablished.
        /// </summary>
        public event EventHandler<IcePeerConnectionStateData> IcePeerConnectionStateChanged;
        /// <summary>
        /// See https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceConnectionState
        /// </summary>
        public event EventHandler<IceConnectionStateData> IceConnectionStateChanged;
        /// <summary>
        /// Current status of the faf-ice-adapter.
        /// </summary>
        public event EventHandler<IceStatusData> IceStatusReceived;
        private int JsonRpcId = 0;

        /// <summary>
        /// Normal or auto
        /// </summary>
        /// <param name="initMode"></param>
        public void SetLobbyInitMode(string initMode)
        {
            var json = IceJsonRpcMethods.SetLobbyInitMode(initMode);
            SendAsync(json);
        }
        List<string> Queue = new();
        
        bool IcePassed = false;
        public void PassIceServers(string iceServers)
        {
            if (IcePassed) return;
            var json = IceJsonRpcMethods.UniversalMethod("setIceServers", $"[{iceServers}]");
            SendAsync(json);
            IcePassed = true;
        }

        public IceClient(string host, int port) : base(host, port)
        {
        }

        string cache;
        private bool TryParseLines(ref string data, out string message)
        {
            message = string.Empty;
            if (data is null) return false;
            var index = data.IndexOf('\n');
            if (index != -1)
            {
                message += cache;
                message += data[..(index + 1)];
                data = data[(index + 1)..];
                cache = null;
            }
            else
            {
                cache += data;
            }
            return message.Length != 0;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var data = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            while (TryParseLines(ref data, out var message))
            {
                ProcessData(message);
            }
        }
        public override bool SendAsync(string text)
        {
            Console.WriteLine(text);
            var sent = base.SendAsync(text[^1] == '\n' ? text : text + '\n'); ;
            if (!sent)
            {
                Queue.Add(text);
                Console.WriteLine("NOT SENT TO ICE");
            }
            return sent;
        }
        protected override void OnConnected()
        {
            Console.WriteLine("Connected to RPC Server");
            SendAsync(IceJsonRpcMethods.AskStatus(++JsonRpcId));
            foreach (var item in Queue)
            {
                Console.WriteLine("FROM QUEUE");
                SendAsync(item);
            }
        }
        protected override void OnDisconnected()
        {
            Thread.Sleep(1000);
            ConnectAsync();
        }

        private bool IsInitialized = false;
        private void ProcessData(string json)
        {
            if (json.StartsWith("{\"me"))
            {
                var raw = json[(json.IndexOf(':') + 2)..^18];
                var methodEndIndex = raw.IndexOf('\"');
                var method = raw[..methodEndIndex];

                var args = raw[(methodEndIndex + 11)..];
                switch (method)
                {
                    case "onConnectionStateChanged":
                        var connected = args == "[\"Connected\"]";
                        ConnectionToGpgNetServerChanged?.Invoke(this, connected);
                        SendAsync(IceJsonRpcMethods.AskStatus(++JsonRpcId));
                        break;
                    case "onGpgNetMessageReceived":
                        var data = args.Split(',');
                        var command = data[0][2..^1];
                        var param = string.Join(',', data[1..]);
                        param = param[..^1];
                        GpgNetMessageReceived?.Invoke(this, new(command, param));
                        break;
                    case "onIceMsg":
                        args = '[' + args[(args.IndexOf(',') + 1)..];
                        //Logger.LogInformation($"From RPC received (onIceMsg): ({args.Length} lenght)");
                        IceMessageReceived?.Invoke(this, args);
                        break;
                    case "onIceConnectionStateChanged":
                        data = args[1..^2].Split(',');
                        var state = data[2][1..];
                        var localPId = data[0];
                        var remotePId = data[1];
                        //Logger.LogInformation(args);
                        SendAsync(IceJsonRpcMethods.AskStatus(++JsonRpcId));
                        break;
                    case "onConnected":
                        //Logger.LogInformation(args);
                        SendAsync(IceJsonRpcMethods.AskStatus(++JsonRpcId));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //{"id":null,"error":{"code":-32700,"message":"Parse Error"},"jsonrpc":"2.0"}
                var jsonRpc = JsonSerializer.Deserialize<JsonRpcData>(json);

                if (jsonRpc.result is not null & jsonRpc.result.Contains("version"))
                {
                    if (!IsInitialized)
                    {
                        IsInitialized = true;
                    }
                    var status = JsonSerializer.Deserialize<IceStatusData>(jsonRpc.result);
                    IceStatusReceived?.Invoke(this, status);
                }
                else
                {

                }
            }

        }

        //bool pendingConnection = false;
        //private void ManagedTcpClient_StateChanged(object sender, ManagedTcpClientState e)
        //{
        //    AppDebugger.LOGJSONRPC($"JSON-RPC TCP Client state changed to \"{e}\"");
        //    if (e == ManagedTcpClientState.Connected)
        //    {
        //        IsConnected = true;

        //        for (int i = 0; i < DataToSend.Count; i++)
        //        {
        //            AppDebugger.LOGJSONRPC($"----Sending from queue------");
        //            if (DataToSend.Count == 0) continue;
        //            if (Send(DataToSend[i]))
        //            {
        //            }
        //        }
        //        DataToSend.Clear();
        //    }
        //    else
        //    {
        //        IsConnected = false;
        //        JsonRpcId = 0;
        //    }
        //    if (e == ManagedTcpClientState.CantConnect)
        //    {
        //        if (pendingConnection) return;
        //        ManagedTcpClient.Connect();
        //        AppDebugger.LOGJSONRPC($"Reconnecting to RPC Server...");
        //        pendingConnection = true;
        //        return;
        //    }
        //    pendingConnection = false;
        //}
    }
}


/*

1. IceAdapterProcess()
1.1. Port = DetermineFreePort()
1.2. faf-ice-adapter.exe --id {playerId} --login {playerLogin} --rpc-port {Port} --gpgnet-port 0 --log-level debug

2. Launch JSON-RPC TCP Client and connect to JSON-RPC Server on localhost:{Port}
2.1. Loop connections until succefully connected

3. PassIceServers()

4. {"result":
        "{"version":"SNAPSHOT",
          "ice_servers_size":0,
          "lobby_port":54135,
          "init_mode":"normal",
          "options":{"player_id":302176,"player_login":"Eternal-","rpc_port":55251,"gpgnet_port":32721},
          "gpgpnet":{"local_port":32721,"connected":false,"game_state":"","task_string":"-"},
          "relays":[]}",
    "id":1,
    "jsonrpc":"2.0"}


-------------- WARNING! UNKNOWN COMMAND: JoinGame ----------------
{
    "command":"JoinGame",
    "args":["Ninrai", 66531],
    "target":"game"
}
-------------- WARNING! UNKNOWN COMMAND: ConnectToPeer ----------------
{
    "command":"ConnectToPeer",
    "args":["INGI", 272379, true],
    "target":"game"
}
-------------- WARNING! UNKNOWN COMMAND: IceMsg ----------------
{
    "command":"IceMsg",
    "args":
        [66531,
        "{
            \"srcId\":66531,
            \"destId\":302176,
            \"password\":\"4t7anb6pile191q4kc5cvn5t27\",
            \"ufrag\":\"56bhp1fvhtcaq7\",
            \"candidates\":[{
                \"foundation\":\"1\",
                \"protocol\":\"udp\",
                \"priority\":2130706431,
                \"ip\":\"141.89.46.155\",
                \"port\":6495,
                \"type\":\"HOST_CANDIDATE\",
                \"generation\":0,
                \"id\":\"24\",
                \"relPort\":0
            },
            {
                \"foundation\":\"2\",
                \"protocol\":\"udp\",
                \"priority\":2130706431,
                \"ip\":\"fe80:0:0:0:ba6c:5434:6779:dd5a\",
                \"port\":6495,
                \"type\":\"HOST_CANDIDATE\",
                \"generation\":0,
                \"id\":\"25\",
                \"relPort\":0
            },
            {
                \"foundation\":\"3\",
                \"protocol\":\"udp\",
                \"priority\":2113937151,
                \"ip\":\"fe80:0:0:0:d4f4:afa6:3e52:39a3\",
                \"port\":6495,
                \"type\":\"HOST_CANDIDATE\",
                \"generation\":0,
                \"id\":\"26\",
                \"relPort\":0
            },
            {
                \"foundation\":\"5\",
                \"protocol\":\"udp\",
                \"priority\":2113937151,
                \"ip\":\"fe80:0:0:0:d042:b6da:42ca:ffb3\",
                \"port\":6495,
                \"type\":\"HOST_CANDIDATE\",
                \"generation\":0,
                \"id\":\"27\",
                \"relPort\":0
            },
            {
                \"foundation\":\"4\",
                \"protocol\":\"udp\",
                \"priority\":2113932031,
                \"ip\":\"192.168.178.30\",
                \"port\":6495,
                \"type\":\"HOST_CANDIDATE\",
                \"generation\":0,
                \"id\":\"28\",
                \"relPort\":0
            },
            {
                \"foundation\":\"7\",
                \"protocol\":\"udp\",
                \"priority\":2815,
                \"ip\":\"116.202.155.226\",
                \"port\":10188,
                \"type\":\"RELAYED_CANDIDATE\",
                \"generation\":0,
                \"id\":\"29\",
                \"relAddr\":\"141.89.46.155\",
                \"relPort\":6495
            }]
        }"],
    "target":"game"
}

*/