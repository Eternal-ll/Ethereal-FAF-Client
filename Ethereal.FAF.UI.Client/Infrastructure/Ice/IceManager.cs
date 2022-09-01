using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceManager
    {
        public event EventHandler Initialized;

        private readonly ILogger Logger;
        private readonly LobbyClient LobbyClient;

        private readonly string JavaRuntimeFile;
        private readonly string IceClientJar;
        private readonly string IceClientLogging;

        public Process IceServer;
        public IceClient IceClient;

        public int RpcPort { get; private set; }
        public int GpgNetPort { get; private set; }

        public IceManager(ILogger logger, LobbyClient lobbyClient, string javaRuntimeFile, string iceClientJar, string iceClientLogging)
        {
            Logger = logger;
            LobbyClient = lobbyClient;
            JavaRuntimeFile = javaRuntimeFile;
            IceClientJar = iceClientJar;
            IceClientLogging = iceClientLogging;
            lobbyClient.IceServersDataReceived += LobbyClient_IceServersDataReceived;
            lobbyClient.IceUniversalDataReceived += LobbyClient_IceUniversalDataReceived;
        }

        private string ice_servers;
        private void LobbyClient_IceServersDataReceived(object sender, IceServersData e)
        {
            ice_servers = e.ice_servers;
        }

        public void InitializeNewPorts()
        {
            var ports = GetFreePort(2);
            RpcPort = ports[0];
            GpgNetPort = ports[1];
            Logger.LogTrace("RPC port: [{}]", RpcPort);
            Logger.LogTrace("GPG NET port: [{}]", GpgNetPort);
        }

        public void Initialize(string playerId, string playerLogin)
        {
            InitializeNewPorts();
            Logger.LogTrace("Initializing Ice process with player id [{}], player login [{}], RPC port [{}], GPG NET port [{}]",
                playerId, playerLogin, RpcPort, GpgNetPort);
            IceServer?.Close();
            IceServer = GetIceServerProcess(playerId, playerLogin);
            IceServer.Start();
            Logger.LogTrace("Ice server initialized");
            var host = "127.0.0.1";
            Logger.LogTrace("Initializing ICE client");
            IceClient = new(host, RpcPort);
            IceClient.ConnectAsync();
            IceClient.PassIceServers(ice_servers);
            IceClient.GpgNetMessageReceived += IceClient_GpgNetMessageReceived;
            IceClient.IceMessageReceived += IceClient_IceMessageReceived;
            IceClient.ConnectionToGpgNetServerChanged += IceClient_ConnectionToGpgNetServerChanged;
            Logger.LogTrace("Initialized ICE client on [{}:{}]", host, RpcPort);
            Initialized?.Invoke(this, null);
        }
        public Process GetIceServerProcess(string playerId, string playerLogin)
        {
            var rpc = RpcPort;
            var gpgnet = GpgNetPort;
            var jar = IceClientJar;
            var java = JavaRuntimeFile;
            var logging = IceClientLogging;
            return GetIceServerProcess(playerId, playerLogin, rpc, gpgnet, jar, java, logging);
        }
        private static Process GetIceServerProcess(string playerId, string playerLogin, int rpcPort, int gpgnetPort, string jar, string java, string logging)
        {
            logging = null;
            // load settings to show ice window
            // ...
            // delay window
            // ...
            var args = $"--id {playerId} --login {playerLogin} --rpc-port {rpcPort} --gpgnet-port {gpgnetPort} --log-level debug";
            // --info-window
            // --delay-ui time
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = java,
                    Arguments = $"-jar \"{jar}\" {args}",
                    UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //CreateNoWindow = true,
                }
            };
            return process;
        }

        private void IceClient_ConnectionToGpgNetServerChanged(object sender, bool e)
        {
            Logger.LogInformation("Connected to GPG Net server [{}]", e);
        }

        private void IceClient_IceMessageReceived(object sender, string e)
        {
            Logger.LogInformation($"Sending Ice message to lobby-server: ({e.Length} lenght)");
            LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("IceMsg", e));
        }

        private void IceClient_GpgNetMessageReceived(object sender, GpgNetMessage e)
        {
            var t = ServerCommands.UniversalGameCommand(e.Command, e.Args);
            Logger.LogInformation($"Sending GPGNetCommand to lobby: {t}");
            LobbyClient.SendAsync(t);
        }

        private void LobbyClient_IceUniversalDataReceived(object sender, IceUniversalData e)
        {
            if (IceClient is null)
            {
                Logger.LogWarning($"Received data related to Ice-Adapter, but client is closed.n {@e}");
                return;
            }
            switch (e.Command)
            {
                case ServerCommand.JoinGame:
                    //'{"method": "joinGame", "params": ["BorisBritva94", 244697], "jsonrpc": "2.0"}'
                    //{"method": "joinGame", "params": ["Stason4ik",437080], "jsonrpc": "2.0"}
                    Logger.LogInformation($"From Server (joinGame): {e.args}");
                    var bg = IceJsonRpcMethods.UniversalMethod("joinGame", e.args);
                    IceClient.SendAsync(bg);
                    break;
                case ServerCommand.HostGame:
                    Logger.LogInformation($"From Server (hostGame): {e.args}");
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("hostGame", e.args));
                    break;
                case ServerCommand.ConnectToPeer:
                    //'{"method": "connectToPeer", "params": ["Zem", 407626, true], "jsonrpc": "2.0"}'
                    Logger.LogInformation($"From Server (connectToPeer): {e.args}");
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("connectToPeer", e.args));
                    break;
                case ServerCommand.DisconnectFromPeer:
                    Logger.LogInformation($"From Server (disconnectFromPeer): {e.args}");
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("disconnectFromPeer", e.args));
                    break;
                case ServerCommand.IceMsg:
                    //{"method": "iceMsg", "params": [352305, "{\"srcId\":352305,\"destId\":302176,\"password\":\"1sfe30qjsjin448t5h9sp2kvbf\",\"ufrag\":\"c7q8a1fvib8mnl\",\"candidates\":[{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":17779,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"203\",\"relAddr\":\"109.60.186.116\",\"relPort\":6989}]}"], "jsonrpc": "2.0"}
                    //{ 'command': 'IceMsg', 'args': [352305, '{"srcId":352305,"destId":302176,"password":"5a8mlkvo3lbe5p9a3rq5s4fptm","ufrag":"40s9b1fvi927mb","candidates":[{"foundation":"1","protocol":"udp","priority":2130706431,"ip":"192.168.0.102","port":6657,"type":"HOST_CANDIDATE","generation":0,"id":"32","relPort":0},{"foundation":"2","protocol":"udp","priority":2130706431,"ip":"fe80:0:0:0:e818:5a94:b9f3:f2cf","port":6657,"type":"HOST_CANDIDATE","generation":0,"id":"33","relPort":0},{"foundation":"3","protocol":"udp","priority":1677724415,"ip":"109.60.186.116","port":6657,"type":"SERVER_REFLEXIVE_CANDIDATE","generation":0,"id":"34","relAddr":"192.168.0.102","relPort":6657},{"foundation":"4","protocol":"udp","priority":2815,"ip":"116.202.155.226","port":11410,"type":"RELAYED_CANDIDATE","generation":0,"id":"35","relAddr":"109.60.186.116","relPort":6657}]}'], 'target': 'game'}
                    //{ "method": "iceMsg", "params": [352305, "{\"srcId":352305,"destId":302176,"password":"5a8mlkvo3lbe5p9a3rq5s4fptm","ufrag\":\"40s9b1fvi927mb\",\"candidates\":[{\"foundation\":\"1\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"192.168.0.102\",\"port\":6657,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"32\",\"relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:e818:5a94:b9f3:f2cf\",\"port\":6657,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"33\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"109.60.186.116\",\"port\":6657,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"34\",\"relAddr\":\"192.168.0.102\",\"relPort\":6657},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":11410,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"35\",\"relAddr\":\"109.60.186.116\",\"relPort\":6657}]}"], "jsonrpc": "2.0"}

                    //{ "method": "iceMsg", "params": [196240, "{"srcId":196240,"destId":302176,"password":"5494ri6f0jtcdtni76do6epjna","ufrag":"85j7t1fvi5vmud","candidates":[{"foundation":"1","protocol":"udp","priority":2130706431,"ip":"192.168.0.101","port":7017,"type":"HOST_CANDIDATE","generation":0,"id":"108","relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:6131:d870:92df:48f5\",\"port\":7017,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"109\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"5.153.134.169\",\"port\":7017,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"110\",\"relAddr\":\"192.168.0.101\",\"relPort\":7017},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":14307,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"111\",\"relAddr\":\"5.153.134.169\",\"relPort\":7017}]}"], "jsonrpc": "2.0"}
                    //{ "method": "iceMsg", "params": [437080,"{\"srcId\":437080,\"destId\":302176,\"password\":\"614au1653q1sb8buh9anm92lmr\",\"ufrag\":\"an6oc1fvi79im5\",\"candidates\":[{\"foundation\":\"1\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"192.168.1.182\",\"port\":6328,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"0\",\"relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:481c:9bf6:1dce:6a7a\",\"port\":6328,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"1\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"37.144.129.42\",\"port\":60164,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"2\",\"relAddr\":\"192.168.1.182\",\"relPort\":6328},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":10642,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"3\",\"relAddr\":\"37.144.129.42\",\"relPort\":60164}]}"], "jsonrpc": "2.0"}

                    Logger.LogInformation($"From Server (iceMsg): ({e.args.Length} lenght)");
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("iceMsg", $"{e.args}"));
                    break;
                default:
                    Logger.LogWarning($"From Server ({e.Command}): {e.args}");
                    //IceClient.Send(IceJsonRpcMethods.UniversalMethod("sendToGpgNet", $"[{e.Command.ToString()}, {e.args}]"));
                    break;
            }
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
    }
}
