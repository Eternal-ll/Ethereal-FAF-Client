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
        /// <summary>
        /// Connection status with remote player
        /// </summary>
        public event EventHandler<ConnectionState> ConnectionStateChanged;

        /// <summary>
        /// Normal or auto
        /// </summary>
        /// <param name="initMode"></param>
        public void SetLobbyInitMode(string initMode)
        {
            var json = IceJsonRpcMethods.SetLobbyInitMode(initMode);
            SendAsync(json);
        }

        readonly List<string> Queue = new();
        readonly List<byte> Cache = new();
        bool IcePassed = false;
        public bool IsStop;

        public void PassIceServersAsync(string iceServers)
        {
            if (IcePassed) return;
            var json = IceJsonRpcMethods.UniversalMethod("setIceServers", $"[{iceServers}]");
            SendAsync(json);
            IcePassed = true;
        }

        public IceClient(string host, int port) : base(host, port)
        {
        }
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (Cache.Count == 0 && buffer[0] == '{' && buffer[^1] == '\n')
            {
                ProcessData(Encoding.UTF8.GetString(buffer, 0, (int)size));
                return;
            }
            for (int i = (int)offset; i < (int)size; i++)
            {
                if (buffer[i] == '\n')
                {
                    ProcessData(Encoding.UTF8.GetString(Cache.ToArray(), 0, Cache.Count));
                    Cache.Clear();
                    continue;
                }
                Cache.Add(buffer[i]);
            }
        }
        public override long Send(string text) => base.Send(text[^1] != '\n' ? text + '\n' : text);
        public override bool SendAsync(string text)
        {
            Console.WriteLine(text);
            var sent = base.SendAsync(text[^1] == '\n' ? text : text + '\n'); ;
            if (!sent)
            {
                Queue.Add(text);
            }
            return sent;
        }
        protected override void OnConnected()
        {
            foreach (var item in Queue)
            {
                Send(item);
            }
        }
        protected override void OnDisconnected()
        {
            if (IsStop) return;
            Thread.Sleep(50);
            if (IsConnecting) return;
            ConnectAsync();
        }

        private bool IsInitialized = false;
        private void ProcessData(string json)
        {
            if (json.StartsWith("{\"me"))
            {
                var rpc = JsonSerializer.Deserialize<RpcRequest>(json);
                switch (rpc.Method)
                {
                    case "onConnectionStateChanged":
                        var connected = rpc.Params[0].ToString() == "Connected";
                        ConnectionToGpgNetServerChanged?.Invoke(this, connected);
                        break;
                    case "onGpgNetMessageReceived":
                        var command = rpc.Params[0].ToString();
                        var param = rpc.Params[1].ToString();
                        GpgNetMessageReceived?.Invoke(this, new(command, param));
                        break;  
                    case "onIceMsg":
                        var ice = $"[{rpc.Params[1]},\"{rpc.Params[2].ToString().Replace("\"","\\\"")}\"]";
                        IceMessageReceived?.Invoke(this, ice);
                        break;
                    case "onIceConnectionStateChanged":
                        var localPlayerId = int.Parse(rpc.Params[0].ToString());
                        var remotePlayerId = int.Parse(rpc.Params[1].ToString());
                        var state = rpc.Params[2].ToString();
                        Console.WriteLine($"Connection state of [{localPlayerId}] to [{remotePlayerId}] is [{state}]");
                        ConnectionStateChanged?.Invoke(this, new ConnectionState(localPlayerId, remotePlayerId, state));
                        break;
                    case "onConnected":
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
    }
    public class ConnectionState
    {
        public int LocalPlayerId { get; set; }
        public int RemotePlayerId { get; set; }
        public string State { get; set; }
        public ConnectionState(int localPlayerId, int remotePlayerId, string state)
        {
            LocalPlayerId = localPlayerId;
            RemotePlayerId = remotePlayerId;
            State = state;
        }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Connected { get; private set; }
        public TimeSpan ConnectedIn { get; private set; }
        public void UpdateState(string state)
        {
            State = state;
            if (state == "connected")
            {
                Connected = DateTime.Now;
                ConnectedIn = Connected - Created;
            }
        }
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