using beta.Models.API;
using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceManager
    {
        public event EventHandler Initialized;

        private readonly LobbyClient LobbyClient;
        private readonly NotificationService NotificationService;
        private readonly IConfiguration Configuration;
        private readonly IFafApiClient FafApiClient;
        private readonly ILogger Logger;

        public CoturnServer[] CoturnServers { get; set; }

        public Process IceServer;
        public IceClient IceClient;

        public int RpcPort { get; private set; }
        public int GpgNetPort { get; private set; }

        public IceManager(ILogger<IceManager> logger, LobbyClient lobbyClient, IConfiguration configuration, NotificationService notificationService, IFafApiClient fafApiClient)
        {
            lobbyClient.IceServersDataReceived += LobbyClient_IceServersDataReceived;
            lobbyClient.IceUniversalDataReceived += LobbyClient_IceUniversalDataReceived;
            Configuration = configuration;
            Logger = logger;
            LobbyClient = lobbyClient;
            NotificationService = notificationService;
            FafApiClient = fafApiClient;
        }

        public async Task<CoturnServer[]> GetCoturnServersAsync(string token)
        {
            if (CoturnServers is not null) return CoturnServers;
            var result = await FafApiClient.GetCoturnServersAsync(token);
            CoturnServers = result.Content.Data;
            return result.Content.Data;
        }

        public void SelectCoturnServers(params int[] ids) =>
            UserSettings.Update("IceAdapter:SelectedCoturnServers", ids);

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
        static string HMACHASH(string str, string key)
        {
            byte[] bkey = Encoding.Default.GetBytes(key);
            using (var hmac = new HMACSHA256(bkey))
            {
                byte[] bstr = Encoding.Default.GetBytes(str);
                var bhash = hmac.ComputeHash(bstr);
                return BitConverter.ToString(bhash).Replace("-", string.Empty).ToLower();
            }
        }
        static string SignHMACSHA1Fomate(string str, string key)
        {
            var encoding = Encoding.GetEncoding("UTF-8");

            var hmacsha1 = new HMACSHA1(encoding.GetBytes(key));
            byte[] hashBytes = hmacsha1.ComputeHash(encoding.GetBytes(str));
            string Sign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return Sign; //de4cbb63e690f2d18fa5e15d400f7608203dd863
        }

        private bool TryGetCoturnServers(long playerId,out string servers)
        {
            servers = null;
            var selected = Configuration.GetSection("IceAdapter:SelectedCoturnServers").Get<int[]>();
            if (selected.Length == 0) return false;
            var ttl = Configuration.GetValue<int>("IceAdapter:TTL");
            servers = JsonSerializer.Serialize(CoturnServers
                .Where(c => selected.Contains(c.Id))
                .Select(c => new
                {
                    urls = new string[]
                    {
                        $"turn:{c.Host}?transport=tcp",
                        $"turn:{c.Host}?transport=udp",
                        $"turn:{c.Host}"
                    },
                    username = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 + ttl}:{playerId}",
                    //credential = 
                }));
            return false;
    //             "urls": [
    //  "turn:coturn-eu-1.supcomhub.org?transport=tcp",
    //  "turn:coturn-eu-1.supcomhub.org?transport=udp",
    //  "stun:coturn-eu-1.supcomhub.org"
    //],
    //"username": "1666207812:302176",
    //"credential": "84/VQrEl/ztCjFeQClu4PHwuPkA=",
    //"credentialType": "token"
        }
        bool HasWebUI;
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
            Thread.Sleep(250);
            IceClient.ConnectAsync();
            IceClient.PassIceServersAsync(ice_servers);
            IceClient.GpgNetMessageReceived += IceClient_GpgNetMessageReceived;
            IceClient.IceMessageReceived += IceClient_IceMessageReceived;
            IceClient.ConnectionToGpgNetServerChanged += IceClient_ConnectionToGpgNetServerChanged;
            Logger.LogTrace("Initialized ICE client on [{}:{}]", host, RpcPort);
            Initialized?.Invoke(this, null);
            if (HasWebUI && Configuration.GetValue<bool>("IceAdapter:UseTelemetryUI", true))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Configuration.GetValue<string>("IceAdapter:Telemetry") + $"?gameId={gameId}&playerId={playerId}",
                    UseShellExecute = true
                });
            }
        }
        public Process GetIceServerProcess(string playerId, string playerLogin, long gameId)
        {
            var ice = Configuration.GetValue<string>("IceAdapter:Executable");
            StringBuilder sb = new();
            sb.Append($"-jar \"{ice}\" ");
            sb.Append($"--id {playerId} ");
            sb.Append($"--login {playerLogin} ");
            HasWebUI = ice.Contains("3.3");
            if (HasWebUI)
            {
                sb.Append($"--game-id {gameId} ");
            }
            sb.Append($"--rpc-port {RpcPort} ");
            sb.Append($"--gpgnet-port {GpgNetPort} ");
            if (Configuration.GetValue("IceAdapter:ForceRelay", false)) sb.Append("--force-relay");
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
