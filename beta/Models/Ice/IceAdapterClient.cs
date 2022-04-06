using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Debugger;
using beta.Models.Ice.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.Models.Ice
{
    /// <summary>
    /// Wrapper for https://github.com/FAForever/java-ice-adapter
    /// </summary>
    internal class IceAdapterClient
    {
        /// <summary>
        /// The game sent a message to the faf-ice-adapter via the internal GPGNetServer.
        /// </summary>
        public event EventHandler<GpgNetMessage>GpgNetMessageReceived;
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

        private readonly IIceService IceService;
        private readonly ISessionService SessionService;
        private readonly ILogger Logger = App.Services.GetService<ILogger<IceAdapterClient>>();
        
        public bool IsConnected { get; set; }
        public int RpcPort { get; set; }
        public int GpgNetPort { get; set; }
        
        private IceAdapterProcess IceAdapterProcess;
        private ManagedTcpClient ManagedTcpClient;
        private int JsonRpcId = 0;

        /// <summary>
        /// Normal or auto
        /// </summary>
        /// <param name="initMode"></param>
        public void SetLobbyInitMode(string initMode)
        {
            var json = IceJsonRpcMethods.SetLobbyInitMode(initMode);
            if (!Send(json))
            {
                DataToSend.Add(json);
            }
        }
        bool IcePassed = false;

        List<string> DataToSend = new();
        public void PassIceServers(string iceServers)
        {
            if (IcePassed) return;

            //{'method': 'setIceServers','params': [[{'urls': ['turn:coturn-eu-1.supcomhub.org?transport=tcp', 'turn:coturn-eu-1.supcomhub.org?transport=udp', 'stun:coturn-eu-1.supcomhub.org'], 'username': '1648751141:302176', 'credential': 'z2QgPN/RyNeSknq3BeRJZzm/gEE=', 'credentialType': 'token'}, {'urls': ['turn:faforever.com?transport=tcp', 'turn:faforever.com?transport=udp', 'stun:faforever.com'], 'username': '1648751141:302176', 'credential': 'z2QgPN/RyNeSknq3BeRJZzm/gEE=', 'credentialType': 'token'}, {'urls': ['turn:faf.mabula.net?transport=tcp', 'turn:faf.mabula.net?transport=udp', 'stun:faf.mabula.net'], 'username': '1648751141:302176', 'credential': 'tBIPwWZVZVFc+a9//5mjhjkhBz0=', 'credentialType': 'token'}]], 'jsonrpc': '2.0'}
            var json = IceJsonRpcMethods.UniversalMethod("setIceServers", $"[{iceServers}]");

            if (!Send(json))
            {
                DataToSend.Add(json);
            }
            IcePassed = true;
        }

        public bool Send(string json)
        {
            if (json.Contains("ice", StringComparison.OrdinalIgnoreCase))
            {

            }

            if (ManagedTcpClient.Write(json))
            {
                AppDebugger.LOGJSONRPC($"Sent:{json.ToJsonFormat()}");
                return true;
            }

            AppDebugger.LOGJSONRPC($"NOT SEN\n{json.ToJsonFormat()}");
            return false;
        }

        private static int[] GetFreePort(int count)
        {
            var ports = new int[count];
            for (int i = 0; i < count; i++)
            {
                TcpListener listener = new(IPAddress.Parse("127.0.0.1"), 0);
                listener.Start();
                ports[i] = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
            }
            return ports;
        }
        public IceAdapterClient(string playerId, string playerLogin)
        {
            var ports = GetFreePort(2);
            RpcPort = ports[0];
            GpgNetPort = ports[1];
            var process = new IceAdapterProcess(playerId, playerLogin, ports[0], ports[1]);

            ManagedTcpClient = new()
            {
                Host = "127.0.0.1",
                Port = ports[0],
                ThreadName = "JSON-RPC TCP Client"
            };

            DataToSend.Add(IceJsonRpcMethods.AskStatus(++JsonRpcId));

            ManagedTcpClient.Connect();
            ManagedTcpClient.StateChanged += ManagedTcpClient_StateChanged;
            ManagedTcpClient.DataReceived += OnJsonRpcDataReceived;
        }

        private bool IsInitialized = false;
        private void OnJsonRpcDataReceived(object sender, string json)
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
                        Send(IceJsonRpcMethods.AskStatus(++JsonRpcId));

                        if (connected) Logger.LogInformation(args);
                        else Logger.LogCritical(args);
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
                        Logger.LogInformation($"From RPC received (onIceMsg): ({args.Length} lenght)");
                        IceMessageReceived?.Invoke(this, args);
                        break;
                    case "onIceConnectionStateChanged":
                        data = args[1..^2].Split(',');
                        var state = data[2][1..];
                        var localPId = data[0];
                        var remotePId = data[1];
                        Logger.LogInformation(args);
                        Send(IceJsonRpcMethods.AskStatus(++JsonRpcId));
                        break;
                    case "onConnected":
                        Logger.LogInformation(args);
                        Send(IceJsonRpcMethods.AskStatus(++JsonRpcId));
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
                Logger.LogInformation($"Received RPC data: (id: {jsonRpc.id}) {jsonRpc.result}");
            }

        }

        public async Task CloseAsync()
        {
            Send(IceJsonRpcMethods.Quit());
            if (IceAdapterProcess is not null)
            {
                await IceAdapterProcess.Process.WaitForExitAsync();
                if (IceAdapterProcess.Process.HasExited)
                {
                    IceAdapterProcess.Process.Close();
                    IceAdapterProcess.Process.Dispose();
                }
            }
        }

        private void ManagedTcpClient_StateChanged(object sender, ManagedTcpClientState e)
        {
            AppDebugger.LOGJSONRPC($"JSON-RPC TCP Client state changed to \"{e}\"");
            if (e == ManagedTcpClientState.Connected)
            {
                IsConnected = true;

                for (int i = 0; i < DataToSend.Count; i++)
                {
                    AppDebugger.LOGJSONRPC($"----Sending from queue------");
                    if (Send(DataToSend[i]))
                    {
                        DataToSend.RemoveAt(i);
                    }
                }
            }
            else
            {
                IsConnected = false;
                JsonRpcId = 0;
            }

            if (e == ManagedTcpClientState.CantConnect)
            {
                ManagedTcpClient.Connect();
                AppDebugger.LOGJSONRPC($"Reconnecting to RPC Server...");

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