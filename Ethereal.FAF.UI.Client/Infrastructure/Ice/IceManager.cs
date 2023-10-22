using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.API.Client.Models.Attributes;
using Ethereal.FAF.API.Client.Models.Base;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceManager
    {
        public event EventHandler Initialized;

        private readonly NotificationService NotificationService;
        private readonly IConfiguration Configuration;
        private readonly ILogger Logger;

        private readonly LobbyClient LobbyClient;
        private readonly IFafApiClient FafApiClient;

        public IceCoturnServer[] CoturnServers { get; set; }

        public Process IceServer;
        public IceClient IceClient;

        public int RpcPort { get; private set; }
        public int GpgNetPort { get; private set; }
        private long LastGameId;

        public List<ConnectionState> ConnectionStates { get; set; }
        public bool AllConnected => ConnectionStates.Where(c => c.State == "connected").Count() == ConnectionStates.Count;

        public IceManager(ILogger<IceManager> logger, IConfiguration configuration, NotificationService notificationService, LobbyClient lobbyClient, IFafApiClient fafApiClient)
        {
            Configuration = configuration;
            Logger = logger;
            NotificationService = notificationService;
            LobbyClient = lobbyClient;
            FafApiClient = fafApiClient;

            lobbyClient.IceUniversalDataReceived += LobbyClient_IceUniversalDataReceived;
			CoturnServers = Array.Empty<IceCoturnServer>();

		}

        public void SelectCoturnServers(params int[] ids) =>
            UserSettings.Update("IceAdapter:SelectedCoturnServers", ids);


        public void InitializeNewPorts()
        {
            Logger.LogInformation("Searching free ports...");
            var ports = GetFreePort(2);
            RpcPort = ports[0];
            GpgNetPort = ports[1];
            Logger.LogInformation("RPC server port: [{rpc}]", RpcPort);
            Logger.LogInformation("GPGNetServer port: [{gpg}]", GpgNetPort);
        }
        private IceCoturnServer[] GetSelectedCoturnServers()
		{
			var selected = Configuration.GetSection("IceAdapter:SelectedCoturnServers").Get<int[]>();
            return CoturnServers.Where(x => selected.Contains(x.Id)).ToArray();
		}
        public void Initialize(long playerId, string playerLogin, long gameId, string initMode)
        {
            LastGameId = gameId;
            InitializeNewPorts();
            IceServer?.Kill();
            IceServer = GetIceServerProcess(playerId, playerLogin, gameId);
            IceServer.Start();
            IceClient = new("127.0.0.1", RpcPort);
            IceClient.SetIceCoturnServers(GetSelectedCoturnServers());
            IceClient.SetInitMode(initMode);
            IceClient.ConnectAsync();
            var t = 0;
            while (!IceClient.IsConnected)
            {
                if (t is 50)
                {
                    throw new Exception("Failed to connect to Ice adapter");
                }
                Thread.Sleep(100);
                IceClient.Connect();
                t++;
            }            

            IceClient.GpgNetMessageReceived += IceClient_GpgNetMessageReceived;
            IceClient.IceMessageReceived += IceClient_IceMessageReceived;
            IceClient.ConnectionToGpgNetServerChanged += IceClient_ConnectionToGpgNetServerChanged;
            IceClient.ConnectionStateChanged += IceClient_ConnectionStateChanged;
            ConnectionStates?.Clear();
            ConnectionStates = null;
            ConnectionStates = new();
            Initialized?.Invoke(this, null);
            if (Configuration.IceAdapterHasWebUiSupport() && Configuration.IceAdapterUseTelemetryUI())
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Configuration.GetIceAdapterTelemtryUrl() + $"?gameId={gameId}&playerId={playerId}",
                    UseShellExecute = true
                });
            }
        }

        private void IceServer_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.LogWarning(e.Data);
        }

        public Process GetIceServerProcess(long playerId, string playerLogin, long gameId)
        {
            //sb.Append($"--log-level {Configuration.GetValue<string>("IceAdapter:LogLevel")} ");
            //if (Configuration.GetValue<bool>("IceAdapter:IsDebugEnabled")) sb.Append("--debug-window ");
            //if (Configuration.GetValue<bool>("IceAdapter:IsInfoEnabled")) sb.Append("--info-window ");
            //sb.Append($"--delay-ui {Configuration.GetValue<long>("IceAdapter:DelayUI")} ");
            //sb.Append(Configuration.GetValue<string>("IceAdapter:Args"));
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetJavaRuntimeExecutable(),
                    Arguments = IceAdapterArguments.Generate(Configuration.GetIceAdapterExecutable())
                        .WithPlayerId(playerId)
                        .WithPlayerLogin(playerLogin)
                        .WithRpcPort(RpcPort)
                        .WithGPGNetPort(GpgNetPort)
                        .WithGameId(gameId, Configuration.IceAdapterHasWebUiSupport())
                        .WithForcedRelay(Configuration.IceAdapterForceRelay())
                        .ToString(),
                    CreateNoWindow = true
                }
            };
            Logger.LogInformation("Prepared FAF Ice Adapter process with arguments: [{args}]", process.StartInfo.Arguments);
            var logs = Configuration.GetValue<string>("IceAdapter:Logs");
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
                    FileName = Configuration.GetJavaRuntimeExecutable(),
                    Arguments = $"-jar \"{Configuration.GetIceAdapterExecutable()}\" --help"
                }
            };
            try
            {
				process.Start();
				string output = process.StandardOutput.ReadToEnd();
				process.Kill();
				return output;
			}
            catch
            {
                return null;
            }
        }
        DateTime start;
        DateTime end;
        private void IceClient_ConnectionStateChanged(object sender, ConnectionState e)
        {
            if (ConnectionStates.Count == 0) start = DateTime.Now;
            //gathering
            //awaitingCandidates
            //checking
            //connected
            //disconnected
            var con = ConnectionStates.FirstOrDefault(c => c.RemotePlayerId == e.RemotePlayerId);
            if (con is not null)
            {
                con.UpdateState(e.State);
            }
            else
            {
                ConnectionStates.Add(e);
            }


            if (ConnectionStates.Where(c => c.State == "connected").Count() == ConnectionStates.Count)
            {
                end = DateTime.Now;


                LogConnectionStatuses();
                Logger.LogInformation("Connected to all players. From [{start}] to [{end}] in [{seconds}] seconds",
                    start, end, (end - start).TotalSeconds);
                //NotificationService.Notify($"Connected to {ConnectionStates.Count} players",
                //    $"Connected in {(end - start).TotalSeconds} seconds", ignoreOs: false);

                start = DateTime.Now;
            }
        }

        public void LogConnectionStatuses()
        {
            //Logger.LogTrace("Connection statuses from game [{gameId}] and player [{country}][{player}]:",
            //    LastGameId, LobbyClient.Self.Country, LobbyClient.Self.Id);
            //var sorted = ConnectionStates
            //    .OrderBy(c => c.Created)
            //    .ThenBy(c => c.Connected)
            //    .ToArray();
            
            //for (int i = 1; i <= sorted.Length; i++)
            //{
            //    var c = sorted[i - 1];
            //    if (c.State == "connected")
            //    {
            //        Logger.LogTrace("[{RemotePlayerId:#######}] - [{State}] from [{Created:T}] to [{Connected:T}] connected in [{TotalSeconds}] seconds",
            //            c.RemotePlayerId, c.State, c.Created, c.Connected, c.ConnectedIn.TotalSeconds);
            //    }
            //    else
            //    {
            //        Logger.LogTrace("[{RemotePlayerId:#######}] - [{State}]",
            //            c.RemotePlayerId, c.State);
            //    }
            //}
        }
        public void NotifyAboutBadConnections()
        {
            var bad = ConnectionStates.Where(c => c.State != "connected");
            NotificationService.Notify("You were not connected to these players",
                string.Join('\n', bad.Select(c => $"{c.RemotePlayerId} - {c.State}")),
                ignoreOs: false);
        }

        private void IceClient_ConnectionToGpgNetServerChanged(object sender, bool e)
        {
            Logger.LogInformation("Connected to GPG Net server [{state}]", e);
        }

        private void IceClient_IceMessageReceived(object sender, string e)
        {
            LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("IceMsg", e));
        }

        private void IceClient_GpgNetMessageReceived(object sender, GpgNetMessage e)
        {
            LobbyClient.SendAsync(ServerCommands.UniversalGameCommand(e.Command, e.Args));
            if (e.Command == "GameFull")
            {
                NotificationService.Notify("Game is full", "", ignoreOs: false);
                return;
            }
            if (e.Command != "Chat") return;
            NotificationService.Notify("Game chat", e.Args);
        }

        private void LobbyClient_IceUniversalDataReceived(object sender, IceUniversalData e)
        {
            if (IceClient is null)
            {
                Logger.LogWarning("IceClient is null, message ignored");
                return;
            }
            switch (e.Command)
            {
                case ServerCommand.JoinGame:
                    var bg = IceJsonRpcMethods.UniversalMethod("joinGame", e.args);
                    IceClient.SendAsync(bg);
                    break;
                case ServerCommand.HostGame:
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("hostGame", e.args));
                    break;
                case ServerCommand.ConnectToPeer:
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("connectToPeer", e.args));
                    break;
                case ServerCommand.DisconnectFromPeer:
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("disconnectFromPeer", e.args));
                    break;
                case ServerCommand.IceMsg:
                    IceClient.SendAsync(IceJsonRpcMethods.UniversalMethod("iceMsg", $"{e.args}"));
                    break;
                default:
                    Logger.LogWarning("Unknown command GPGNet server [{Command}] args [{args}]", e.Command, e.args);
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
