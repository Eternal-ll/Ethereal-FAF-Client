using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceManager
    {
        public event EventHandler Initialized;

        private readonly LobbyClient LobbyClient;
        private readonly NotificationService NotificationService;
        private readonly IConfiguration Configuration;
        private readonly ILogger Logger;


        public Process IceServer;
        public IceClient IceClient;

        public int RpcPort { get; private set; }
        public int GpgNetPort { get; private set; }

        public IceManager(ILogger logger, LobbyClient lobbyClient, IConfiguration configuration, NotificationService notificationService)
        {
            Configuration = configuration;
            Logger = logger;
            LobbyClient = lobbyClient;
            lobbyClient.IceServersDataReceived += LobbyClient_IceServersDataReceived;
            lobbyClient.IceUniversalDataReceived += LobbyClient_IceUniversalDataReceived;
            NotificationService = notificationService;
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

        public void Initialize(string playerId, string playerLogin, long gameId)
        {
            InitializeNewPorts();
            Logger.LogTrace("Initializing Ice process with player id [{}], player login [{}], RPC port [{}], GPG NET port [{}]",
                playerId, playerLogin, RpcPort, GpgNetPort);
            IceServer?.Close();
            IceServer = GetIceServerProcess(playerId, playerLogin, gameId);
            IceServer.Start();
            Logger.LogTrace("Ice server initialized");
            var host = "127.0.0.1";
            Logger.LogTrace("Initializing ICE client");
            IceClient = new(host, RpcPort);
            IceClient.Connect();
            IceClient.PassIceServers(ice_servers);
            IceClient.GpgNetMessageReceived += IceClient_GpgNetMessageReceived;
            IceClient.IceMessageReceived += IceClient_IceMessageReceived;
            IceClient.ConnectionToGpgNetServerChanged += IceClient_ConnectionToGpgNetServerChanged;
            Logger.LogTrace("Initialized ICE client on [{}:{}]", host, RpcPort);
            Initialized?.Invoke(this, null);
        }
        public Process GetIceServerProcess(string playerId, string playerLogin, long gameId)
        {
            StringBuilder sb = new();
            sb.Append($"-jar \"{Configuration.GetValue<string>("IceAdapter:Executable")}\" ");
            sb.Append($"--id {playerId} ");
            sb.Append($"--login {playerLogin} ");
            sb.Append($"--game-id {gameId} ");
            sb.Append($"--rpc-port {RpcPort} ");
            sb.Append($"--gpgnet-port {GpgNetPort} ");
            //sb.Append($"--log-level {Configuration.GetValue<string>("IceAdapter:LogLevel")} ");
            //if (Configuration.GetValue<bool>("IceAdapter:IsDebugEnabled")) sb.Append("--debug-window ");
            //if (Configuration.GetValue<bool>("IceAdapter:IsInfoEnabled")) sb.Append("--info-window ");
            //sb.Append($"--delay-ui {Configuration.GetValue<long>("IceAdapter:DelayUI")} ");
            //sb.Append(Configuration.GetValue<string>("IceAdapter:Args"));
            var logs = Configuration.GetValue<string>("IceAdapter:Logs");
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetValue<string>("Paths:JavaRuntime"),
                    Arguments = sb.ToString(),
                    UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //CreateNoWindow = true,
                }
            };
            if (!string.IsNullOrWhiteSpace(logs))
                process.StartInfo.Environment.Add("LOG_DIR", logs);
            return process;
        }
        public string GetIceHelpMessage()
        {
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetValue<string>("Paths:JavaRuntime"),
                    Arguments = $"-jar \"{Configuration.GetValue<string>("IceAdapter:Executable")}\" --help",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.Kill();
            return output;
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
            if (e.Command == "GameFull")
            {
                NotificationService.Notify("Game", "Game is full");
                return;
            }
            if (e.Command != "Chat") return;
            NotificationService.Notify("Game chat", e.Args);
        }

        private void LobbyClient_IceUniversalDataReceived(object sender, IceUniversalData e)
        {
            if (IceClient is null)
            {
                Logger.LogWarning($"Received data related to Ice-Adapter, but client is closed. {@e}");
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
