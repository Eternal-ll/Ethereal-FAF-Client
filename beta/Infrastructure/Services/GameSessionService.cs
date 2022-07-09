using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models;
using beta.Models.API;
using beta.Models.API.Base;
using beta.Models.Enums;
using beta.Models.Ice;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class GameSessionService : IGameSessionService
    {
        public event EventHandler<GameInfoMessage> GameFilled;

        private readonly ISessionService SessionService;
        private readonly IGamesService GamesService;
        private readonly IDownloadService DownloadService;
        private readonly IMapsService MapsService;
        private readonly IIceService IceService;
        private readonly IPlayersService PlayersService;
        private readonly INotificationService NotificationService;
        private readonly IReplayServerService ReplayServerService;
        private readonly ILogger Logger;

        private readonly IConfiguration Configuration;
        private GameInfoMessage LastGame;
        private IceAdapterClient IceAdapterClient;
        private readonly List<string> IceMessagesQueue = new();
        private readonly ApiFeaturedModFileData[] PreviousFeaturedModData;
        private bool IsLocalFilesChanged;
        private Process ForgedAlliance;
        private Process ReplayForgedAlliance;


        private long GameUID = 0;

        public bool GameIsRunning => ForgedAlliance is not null;

        public GameSessionState State => throw new NotImplementedException();

        public GameSessionService(
            IDownloadService downloadService,
            IMapsService mapsService,
            ISessionService sessionService,
            IGamesService gamesService,
            ILogger<GameSessionService> logger,
            IIceService iceService,
            IPlayersService playersService,
            INotificationService notificationService,
            IReplayServerService replayServerService,
            IConfiguration configuration)
        {
            DownloadService = downloadService;
            MapsService = mapsService;
            Logger = logger;
            SessionService = sessionService;
            IceService = iceService;
            GamesService = gamesService;
            PlayersService = playersService;
            NotificationService = notificationService;
            ReplayServerService = replayServerService;
            Configuration = configuration;

            SessionService.GameLaunchDataReceived += OnGameLaunchDataReceived;
            SessionService.IceUniversalDataReceived += SessionService_IceUniversalDataReceived;
            SessionService.Authorized += SessionService_Authorized;
            sessionService.MatchCancelledDataReceived += SessionService_MatchCancelledDataReceived;

            System.Windows.Application.Current.Exit += (s, e) =>
            {
                IceAdapterClient?.CloseAsync();
            };

            IceService.IceServersReceived += IceService_IceServersReceived;
            replayServerService.ReplayRecorderCreated += ReplayServerService_ReplayRecorderCreated;
        }

        private void SessionService_MatchCancelledDataReceived(object sender, MatchCancelledData e)
        {
            if (GameIsRunning)
            {
                ForgedAlliance.Close();
            }
        }

        private void ReplayServerService_ReplayRecorderCreated(object sender, ReplayRecorder e)
        {
            var game = GamesService.GetGame(GameUID);
            if (game is null)
            {
                throw new ArgumentNullException(nameof(game));
            }
            e.Initialize(ForgedAlliance, game, PlayersService.Self.login);
        }

        private void SessionService_Authorized(object sender, bool e)
        {
            if (e)
            {
                var ices = IceMessagesQueue;
                if (IceAdapterClient is not null && IceAdapterClient.IsConnected)
                {
                    SessionService.Send(ServerCommands.RestoreGameSession(LastGame.uid.ToString()));
                    for (int i = 0; i < ices.Count; i++)
                    {
                        SessionService.Send(ServerCommands.UniversalGameCommand("IceMsg", ices[i]));
                    }
                }
                ices.Clear();
            }
        }

        private void SessionService_IceUniversalDataReceived(object sender, IceUniversalData e)
        {
            if (IceAdapterClient is null)
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
                    IceAdapterClient.Send(bg);
                    break;
                case ServerCommand.HostGame:
                    Logger.LogInformation($"From Server (hostGame): {e.args}");
                    IceAdapterClient.Send(IceJsonRpcMethods.UniversalMethod("hostGame", e.args));
                    break;
                case ServerCommand.ConnectToPeer:
                    //'{"method": "connectToPeer", "params": ["Zem", 407626, true], "jsonrpc": "2.0"}'
                    Logger.LogInformation($"From Server (connectToPeer): {e.args}");
                    IceAdapterClient.Send(IceJsonRpcMethods.UniversalMethod("connectToPeer", e.args));
                    break;
                case ServerCommand.DisconnectFromPeer:
                    Logger.LogInformation($"From Server (disconnectFromPeer): {e.args}");
                    IceAdapterClient.Send(IceJsonRpcMethods.UniversalMethod("disconnectFromPeer", e.args));
                    break;
                case ServerCommand.IceMsg:
                    //{"method": "iceMsg", "params": [352305, "{\"srcId\":352305,\"destId\":302176,\"password\":\"1sfe30qjsjin448t5h9sp2kvbf\",\"ufrag\":\"c7q8a1fvib8mnl\",\"candidates\":[{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":17779,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"203\",\"relAddr\":\"109.60.186.116\",\"relPort\":6989}]}"], "jsonrpc": "2.0"}
                    //{ 'command': 'IceMsg', 'args': [352305, '{"srcId":352305,"destId":302176,"password":"5a8mlkvo3lbe5p9a3rq5s4fptm","ufrag":"40s9b1fvi927mb","candidates":[{"foundation":"1","protocol":"udp","priority":2130706431,"ip":"192.168.0.102","port":6657,"type":"HOST_CANDIDATE","generation":0,"id":"32","relPort":0},{"foundation":"2","protocol":"udp","priority":2130706431,"ip":"fe80:0:0:0:e818:5a94:b9f3:f2cf","port":6657,"type":"HOST_CANDIDATE","generation":0,"id":"33","relPort":0},{"foundation":"3","protocol":"udp","priority":1677724415,"ip":"109.60.186.116","port":6657,"type":"SERVER_REFLEXIVE_CANDIDATE","generation":0,"id":"34","relAddr":"192.168.0.102","relPort":6657},{"foundation":"4","protocol":"udp","priority":2815,"ip":"116.202.155.226","port":11410,"type":"RELAYED_CANDIDATE","generation":0,"id":"35","relAddr":"109.60.186.116","relPort":6657}]}'], 'target': 'game'}
                    //{ "method": "iceMsg", "params": [352305, "{\"srcId":352305,"destId":302176,"password":"5a8mlkvo3lbe5p9a3rq5s4fptm","ufrag\":\"40s9b1fvi927mb\",\"candidates\":[{\"foundation\":\"1\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"192.168.0.102\",\"port\":6657,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"32\",\"relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:e818:5a94:b9f3:f2cf\",\"port\":6657,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"33\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"109.60.186.116\",\"port\":6657,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"34\",\"relAddr\":\"192.168.0.102\",\"relPort\":6657},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":11410,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"35\",\"relAddr\":\"109.60.186.116\",\"relPort\":6657}]}"], "jsonrpc": "2.0"}

                    //{ "method": "iceMsg", "params": [196240, "{"srcId":196240,"destId":302176,"password":"5494ri6f0jtcdtni76do6epjna","ufrag":"85j7t1fvi5vmud","candidates":[{"foundation":"1","protocol":"udp","priority":2130706431,"ip":"192.168.0.101","port":7017,"type":"HOST_CANDIDATE","generation":0,"id":"108","relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:6131:d870:92df:48f5\",\"port\":7017,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"109\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"5.153.134.169\",\"port\":7017,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"110\",\"relAddr\":\"192.168.0.101\",\"relPort\":7017},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":14307,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"111\",\"relAddr\":\"5.153.134.169\",\"relPort\":7017}]}"], "jsonrpc": "2.0"}
                    //{ "method": "iceMsg", "params": [437080,"{\"srcId\":437080,\"destId\":302176,\"password\":\"614au1653q1sb8buh9anm92lmr\",\"ufrag\":\"an6oc1fvi79im5\",\"candidates\":[{\"foundation\":\"1\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"192.168.1.182\",\"port\":6328,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"0\",\"relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:481c:9bf6:1dce:6a7a\",\"port\":6328,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"1\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"37.144.129.42\",\"port\":60164,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"2\",\"relAddr\":\"192.168.1.182\",\"relPort\":6328},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":10642,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"3\",\"relAddr\":\"37.144.129.42\",\"relPort\":60164}]}"], "jsonrpc": "2.0"}

                    Logger.LogInformation($"From Server (iceMsg): ({e.args.Length} lenght)");
                    IceAdapterClient.Send(IceJsonRpcMethods.UniversalMethod("iceMsg", $"{e.args}"));
                    break;
                default:
                    Logger.LogWarning($"From Server ({e.Command}): {e.args}");
                    //IceAdapterClient.Send(IceJsonRpcMethods.UniversalMethod("sendToGpgNet", $"[{e.Command.ToString()}, {e.args}]"));
                    break;
            }
        }

        private void IceService_IceServersReceived(object sender, string e)
        {
            if (IceAdapterClient is not null)
            {
                IceAdapterClient.PassIceServers(e);
            }
        }

        private void FillPlayerArgs(StringBuilder args, string ratingType,
            // player info
            string country = "", string clan = null,
            // player rating
            int mean = 1500, int deviation = 500, int games = 0)
        {
            var me = PlayersService.Self;
            if (me is not null)
            {
                if (me.ratings.TryGetValue(ratingType, out var rating))
                {
                    mean = (int)rating.rating[0];
                    deviation = (int)rating.rating[1];
                    games = rating.number_of_games;
                }
                country = me.country;
                clan = me.clan;
            }
            if (country.Length > 0)
            {
                args.Append("/country ");
                args.Append(country);
                args.Append(' ');
            }
            if (clan?.Length > 0)
            {
                args.Append("/clan ");
                args.Append(clan);
                args.Append(' ');
            }
            args.Append("/mean ");
            args.Append(mean);
            args.Append(' ');
            args.Append("/deviation ");
            args.Append(deviation);
            args.Append(' ');
            args.Append("/numgames ");
            args.Append(games);
            args.Append(' ');
        }

        private async Task InitializeIce()
        {
            await Task.Run(async () => await IceAdapterClient.Initialize())
                .ContinueWith(async task =>
                {
                    if (task.IsFaulted)
                    {
                        Logger.LogError(task.Exception.Message);
                        await InitializeIce();
                    }
                });
        }
        public bool IsLaunching { get; set; }
        private void OnGameLaunchDataReceived(object sender, GameLaunchData e) => 
            Task.Run(async () =>
            {
                IsLaunching = true;
                if (LastGame is null)
                {
                    // that means that we received matchmaker game launch data and we have to confirm path and mods
                }
                await RunGame(e);
            })
                .ContinueWith(task =>
                {
                    IsLaunching = false;
                    if (task.IsFaulted)
                    {
                        SessionService.Send(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
                        App.Current.Dispatcher.Invoke(() => NotificationService.ShowExceptionAsync(task.Exception));
                    }
                });

        private string GetInitMode(int mode) => mode switch
        {
            0 => "normal",
            1 => "auto",
            _ => throw new NotImplementedException($"Unknown mode: {mode}")
        };

        private async Task RunGame(GameLaunchData e)
        {
            IceAdapterClient ice = new(Settings.Default.PlayerId.ToString(), Settings.Default.PlayerNick);
            IceAdapterClient = ice;
            await InitializeIce();

            GameUID = e.uid;

            ice.GpgNetMessageReceived += IceAdapterClient_GpgNetMessageReceived;
            ice.IceMessageReceived += IceAdapterClient_IceMessageReceived;
            ice.ConnectionToGpgNetServerChanged += IceAdapterClient_ConnectionToGpgNetServerChanged;
            // these commands will be ququed and passed after established connect

            ice.SetLobbyInitMode(e.init_mode
                .ToString()
                .ToLower());

            ice.PassIceServers(IceService.IceServers);

            var me = PlayersService.Self;

            // game args
            StringBuilder arguments = new();
            // hide embedded game bug report
            arguments.Append("/nobugreport ");
            arguments.Append($"/init init_{e.mod.ToString().ToLower()}.lua ");
            // port from Ice-Adapter status message ["gpgnet"]["local_port"]
            arguments.Append($"/gpgnet 127.0.0.1:{ice.GpgNetPort} ");
            // append player data
            FillPlayerArgs(arguments, e.rating_type);

            if (e.init_mode == GameInitMode.Auto)
            {
                // matchmaker
                arguments.Append($"/players {e.expected_players} ");
                arguments.Append($"/team {e.team} ");
                arguments.Append($"/startspot {e.map_position} ");

            }

            // append replay stream
            bool isSavingReplay = true;
            if (isSavingReplay)
            {
                var replayPort = ReplayServerService.StartReplayServer();
                arguments.Append($"/savereplay \"gpgnet://localhost:{replayPort}/{e.uid}/{me.login}.SCFAreplay\" ");
            }
            // append game logger
            bool isLogging = false;
            if (isLogging)
            {
                arguments.Append($"/log \"C:\\ProgramData\\FAForever\\logs\\game.uid.{e.uid}.log\" ");
            }

            Logger.LogWarning($"Starting game with args: {arguments}");
            ForgedAlliance = new()
            {
                StartInfo = new()
                {
                    FileName = @"C:\ProgramData\FAForever\bin\ForgedAlliance.exe",
                    Arguments = arguments.ToString(),
                    UseShellExecute = true
                }
            };
            if (!ForgedAlliance.Start())
            {
                Logger.LogError("Cant start game");
                throw new Exception("Can`t start \"Supreme Commander: Forged Alliance\"");
            }
            await ForgedAlliance.WaitForExitAsync();
            ForgedAlliance.Close();
            ForgedAlliance.Dispose();
            ForgedAlliance = null;

            await ice.CloseAsync();
            ice.GpgNetMessageReceived -= IceAdapterClient_GpgNetMessageReceived;
            ice.IceMessageReceived -= IceAdapterClient_IceMessageReceived;
            ice.ConnectionToGpgNetServerChanged -= IceAdapterClient_ConnectionToGpgNetServerChanged;
            ice = null;

            SessionService.Send(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
            LastGame = null;
        }

        private void IceAdapterClient_ConnectionToGpgNetServerChanged(object sender, bool e)
        {
            if (!e)
            {

            }
        }

        private void IceAdapterClient_IceMessageReceived(object sender, string e)
        {
            if (SessionService.IsAuthorized)
            {
                Logger.LogInformation($"Sending Ice message to lobby-server: ({e.Length} lenght)");
                SessionService.Send(ServerCommands.UniversalGameCommand("IceMsg", e));
            }
            else
            {
                //if (e.Contains("\"candidate\"", StringComparison.OrdinalIgnoreCase))
                //{
                //    IceMessagesQueue.Clear();
                //}
                IceMessagesQueue.Add(e);
            }
        }

        private void IceAdapterClient_GpgNetMessageReceived(object sender, GpgNetMessage e)
        {
            var t = ServerCommands.UniversalGameCommand(e.Command, e.Args);
            Logger.LogInformation($"Sending GPGNetCommand to lobby-server: {t}");
            SessionService.Send(t);
        }

        public async Task ConfirmMap(string mapName, bool askToDownload = false)
        {
            if (!ConfirmLocalMapState(mapName))
            {
                if (!Settings.Default.AlwaysDownloadMap)
                {
                    switch (await App.Current.Dispatcher.InvokeAsync(()=>new ContentDialog()
                    {
                        Content = $"Download map \"{mapName}\"",
                        CloseButtonText = "No",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "Always"
                    }.ShowAsync()).Result)
                    {
                        case ContentDialogResult.Primary:
                            break;
                        case ContentDialogResult.Secondary:
                            Settings.Default.AlwaysDownloadMap = true;
                            break;
                        case ContentDialogResult.None:
                        default:
                            Logger.LogWarning($"Refused to download map {mapName}");
                            return;
                    }
                }
                else
                {
                    Logger.LogWarning("Auto downloading map");
                }

                // Run task for downloading
                //await Task.Run(() => MapsService.Download(new($"https://content.faforever.com/maps/{mapName}.zip")));
                var model = await MapsService.DownloadAndExtractAsync(new($"https://content.faforever.com/maps/{mapName}.zip"));

                if (!model.IsCompleted && Settings.Default.AlwaysDownloadMap)
                {
                    throw new Exception($"Map \"{mapName}\" downloading is not completed");
                }
            }
            else
            {
                Logger.LogInformation($"Map \"{mapName}\" confirmed!");
            }
        }

        public async Task<bool> ConfirmGamePath()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.PathToGame)) return true;

            var model = new SelectPathToGameViewModel();
            var result = await App.Current.Dispatcher.InvokeAsync(() => NotificationService.ShowDialog(model));
            result.Wait();
            if (result.Result == ContentDialogResult.None)
            {
                return false;
            }
            Settings.Default.PathToGame = model.Path;
            return true;
        }

        public async Task JoinGame(GameInfoMessage game, string password = null)
        {
            Logger.LogInformation($"Joining to the game '{game.title}' hosted by '{game.host}' on '{game.mapname}'");

            if (!await ConfirmGamePath())
            {
                return;
            }

            if (ForgedAlliance is not null)
            {
                throw new Exception("Game \"Supreme Commander: Forged Alliance\" is already running");
            }

            if (game.mapname.StartsWith("neroxis"))
            {
                throw new NotImplementedException("Neroxis map generator is not supported");
            }

            if (game.sim_mods.Count > 0)
            {
                throw new NotImplementedException("SIM mods is not supported");
            }

            if (game.password_protected)
            {
                if (password is null)
                {
                    throw new ArgumentNullException("Given password is null. Lobby is password protected. You must insert password to join to lobby");
                }
            }

            LastGame = game;

            // Check mods?
            // ...

            // Check map
            await ConfirmMap(game.mapname);

            // Check current patch
            // ConfirmPatch takes about 1800ms to process
            if (!await ConfirmPatch(game.FeaturedMod)) return;
            Logger.LogInformation("Patch confirmed");

            string command = ServerCommands.JoinGame(game.uid.ToString(), password: password);
            await SessionService.SendAsync(command);
        }

        private async Task<bool> ConfirmPatch(FeaturedMod mod)
        {
            var dataToDownload = await ConfirmPatchFiles(mod);
            if (dataToDownload.Length == 0) return true;
            // we have patch files to download

            if (ForgedAlliance is not null && !ForgedAlliance.HasExited)
            {
                throw new Exception("You cant update patch while game is running");
            }
            if (ReplayForgedAlliance is not null && !ReplayForgedAlliance.HasExited)
            {
                throw new Exception("You cant update patch while replay is running");
            }

            Logger.LogWarning($"Patch {mod} required to download");

            if (!Settings.Default.AlwaysDownloadPatch)
            {
                //MessageBox.Show("", "", MessageBoxButton.YesNo);
                ContentDialogResult result;
                result = await NotificationService.ShowDialog("Patch required to download:\n" + mod, "Download", "Always download", "Cancel");

                if (result == ContentDialogResult.None)
                {
                    Logger.LogWarning($"Refused to download patch {mod}");
                    return false;
                }

                if (result == ContentDialogResult.Secondary)
                {
                    Logger.LogWarning("Set to always download patch");
                    // Always download
                    Settings.Default.AlwaysDownloadPatch = true;
                }
            }
            else
            {
                Logger.LogWarning("Auto downloading patch");
            }

            var download = DownloadPatchFiles(dataToDownload);

            NotificationService.ShowDownloadDialog(download, "Cancel");

            await download.DownloadAllAsync();

            if (!download.IsCompleted)
            {
                throw new Exception("Patch download is not completed");
            }

            var files = await ConfirmPatchFiles(mod);
            if (files.Length == 0) return true;
            throw new Exception($"Something went wrong. Patch files didn`t match MD5:\n{string.Join(',', files.Select(f => f.Name))}");
        }

        private bool ConfirmLocalMapState(string name) => MapsService.CheckLocalMap(name) switch
        {
            LocalMapState.NotExist => false,
            LocalMapState.Older => false,
            LocalMapState.Newest => false,
            LocalMapState.Same => true,
            LocalMapState.Unknown => false,
            _ => throw new NotImplementedException(),
        };

        private async Task<ApiFeaturedModFileData[]> ConfirmPatchFiles(FeaturedMod featuredMod)
        {
            Logger.LogInformation($"Confirming patch for {featuredMod} game mod");
            WebRequest webRequest = WebRequest.Create($"https://api.faforever.com/featuredMods/{(int)featuredMod}/files/latest");

            using WebResponse response = await webRequest.GetResponseAsync();
            
            var result = await JsonSerializer.DeserializeAsync<ApiUniversalResult<ApiFeaturedModFileData[]>>(response.GetResponseStream());

            var localPath = App.GetPathToFolder(Folder.ProgramData);
            Logger.LogInformation($"Using {Folder.ProgramData}");

            Logger.LogInformation($"Local path: {localPath}");

            // Check MD5 of existed files. Clearing list of required files to download

            List<ApiFeaturedModFileData> data = new();

            Logger.LogInformation($"Files to check: {result.Data.Length}\nChecking files...");
            for (int i = 0; i < result.Data.Length; i++)
            {
                var item = result.Data[i];
                var file = localPath + item.Group + "\\";

                // TODO Move this checks of Bin / Gamedata
                if (!Directory.Exists(file))
                    Directory.CreateDirectory(file);

                file += item.Name;

                if (!ConfirmFile(file, item.MD5))
                {
                    data.Add(item);
                    Logger.LogWarning($"{i + 1}/{result.Data.Length} MD5 not confirmed {item.Name} ");
                }
                else
                {
                    Logger.LogInformation($"{i + 1}/{result.Data.Length} MD5 confirmed {item.Name}");
                }
            }

            if (data.Count > 0)
            {
                Logger.LogWarning($"Files to download {data.Count}/{result.Data.Length}");
            }

            return data.ToArray();
        }

        private DownloadViewModel DownloadPatchFiles(ApiFeaturedModFileData[] data)
        {
            var localPath = App.GetPathToFolder(Folder.ProgramData);
            var models = new DownloadItem[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];

                models[i] = new(localPath + item.Group + "\\", item.Name, item.Url.AbsoluteUri);
            }

            return DownloadService.GetDownload(models);
        }

        /// <summary>
        /// Returns true if exists and has same MD5
        /// </summary>
        /// <param name="file"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        private bool ConfirmFile(string file, string md5)
        {
            if (!File.Exists(file))
            {
                Logger.LogWarning($"File not exist {file}");
                return false;
            }
            var fileMD5 = Tools.CalculateMD5FromFile(file);
            if (fileMD5 != md5)
            {
                Logger.LogWarning($"MD5 didn`t match\n{file}\n{fileMD5} != {md5}");
                return false;
            }
            return true;
        }

        private bool CopyOriginalBin()
        {
            Logger.LogInformation("Copying original game files from Bin folder...");
            var pathToGame = Settings.Default.PathToGame;

            if (pathToGame is null)
            {
                Logger.LogError("No path to game folder");
                throw new ArgumentNullException(pathToGame);
            }

            if (!Directory.Exists(pathToGame))
            {
                Logger.LogError($"No game folder on {pathToGame}");
                throw new Exception("No game folder");
            }

            var pathToBin = pathToGame + "\\bin";

            if (!Directory.Exists(pathToBin))
            {
                throw new Exception("No bin folder");
            }

            var localPath = App.GetPathToFolder(Folder.ProgramData) + "bin";

            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            Logger.LogInformation($"Target folder to copy: {localPath}");

            var files = Directory.GetFiles(pathToBin);
            Logger.LogInformation($"Files to copy: {files.Length}\nCopying files from Bin folder");
            for (int i = 0; i < files.Length; i++)
            {
                Logger.LogInformation($"Copying {files[i]}");
                File.Copy(files[i], files[i].Replace(pathToBin, localPath), true);
            }
            Logger.LogInformation("Copying completed");

            return true;
        }

        public void RestoreGame(int uid)
        {
            /* SEND
            "command": "restore_game_session",
            "game_id": 123
            */

            /* REPLY / WRONG
            "command": "notice",
            "style": "info",
            "text": "The game you were connected to does no longer exist"
            */
        }

        private void OnGameFilled(GameInfoMessage e) => GameFilled?.Invoke(this, e);

        public async Task Close()
        {
            if (ForgedAlliance is not null && !ForgedAlliance.HasExited)
            {
                ForgedAlliance.Close();
            }
        }

        public async Task ResetPatch()
        {
            await Task.Run(() => CopyOriginalBin());
            NotificationService.Notify("Patch reset completed");
        }

        public async Task HostGame(string title, FeaturedMod mod, string mapName, double? minRating, double? maxRating, GameVisibility visibility = GameVisibility.Public, bool isRatingResctEnforced = false, string password = null, bool isRehost = false)
        {
            if(!await ConfirmGamePath())
            {
                return;
            }

            if (ForgedAlliance is not null)
            {
                throw new Exception("Game \"Supreme Commander: Forged Alliance\" is already running");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "Title is empty");
            }

            if (string.IsNullOrWhiteSpace(mapName))
            {
                throw new ArgumentNullException(nameof(mapName), "Map name is empty");
            }

            //"title":"Welcome",
            //"featured_mod":"faf",
            //"visibility":"public",
            //"mapname":"dualgap_adaptive.v0012",
            //"rating_min":null,
            //"rating_max":null,
            //"password_protected":false,
            //"enforce_rating_range":false,

            StringBuilder sb = new();
            sb.Append("{\"command\":\"game_host\",");
            sb.Append($"\"title\":\"{title}\",");
            sb.Append($"\"mod\":\"{mod.ToString().ToLower()}\",");
            sb.Append($"\"mapname\":\"{mapName}\",");
            sb.Append($"\"visibility\":\"{visibility.ToString().ToLower()}\",");

            if (isRatingResctEnforced && (minRating.HasValue || maxRating.HasValue))
            {
                sb.Append($"\"enforce_rating_range\":{isRatingResctEnforced},");
                if (minRating.HasValue)
                {
                    sb.Append($"\"rating_min\":{minRating.Value},");
                }
                if (maxRating.HasValue)
                {
                    sb.Append($"\"rating_max\":{maxRating.Value},");
                }
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                sb.Append($"\"password\":\"{password}\",");
            }
            sb[^1] = '}';
            var command = sb.ToString();

            // Check map
            await ConfirmMap(mapName, true);

            // Check current patch
            if (!await ConfirmPatch(mod)) return;
            Logger.LogInformation("Patch confirmed");
            
            SessionService.Send(command);
        }
        public async Task WatchGame(long replayId, string mapName, int playerId, FeaturedMod featuredMod, bool isLive = true)
        {
            if (featuredMod != FeaturedMod.FAF) return;
            // game args
            StringBuilder args = new();
            // hide embedded game bug report
            args.Append("/nobugreport ");
            args.Append($"/init init_{featuredMod.ToString().ToLower()}.lua ");
            args.Append($"/replay gpgnet://lobby.faforever.com/{replayId}/{playerId}.SCFAreplay ");
            args.Append($"/replayid {replayId} ");
            args.Append("/log \"C:\\ProgramData\\FAForever\\logs\\replay.log\"");
            //'"C:\\ProgramData\\FAForever\\bin\\ForgedAlliance.exe" /replay gpgnet://lobby.faforever.com/16997391/369689.SCFAreplay /init init_faf.lua /nobugreport /log "C:\\ProgramData\\FAForever\\logs\\replay.log" /replayid 16997391'
            if (!ConfirmLocalMapState(mapName))
            {
                var model = await MapsService.DownloadAndExtractAsync(new($"https://content.faforever.com/maps/{mapName}.zip"), false);

                if (!model.IsCompleted)
                {
                    return;
                }
            }

            if (!await ConfirmPatch(featuredMod)) return;

            ReplayForgedAlliance = new()
            {
                StartInfo = new()
                {
                    FileName = @"C:\ProgramData\FAForever\bin\ForgedAlliance.exe",
                    Arguments = args.ToString(),
                    UseShellExecute = true
                }
            };
            if (!ReplayForgedAlliance.Start())
            {
                Logger.LogError("Cant start game");
                return;
            }
            await ReplayForgedAlliance.WaitForExitAsync();
            ReplayForgedAlliance.Close();
            ReplayForgedAlliance.Dispose();
            ReplayForgedAlliance = null;
        }
    }
}
