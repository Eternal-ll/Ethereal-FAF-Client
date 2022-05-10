using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models;
using beta.Models.API;
using beta.Models.Enums;
using beta.Models.Ice;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            IReplayServerService replayServerService)
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

            SessionService.GameLaunchDataReceived += OnGameLaunchDataReceived;
            SessionService.IceUniversalDataReceived += SessionService_IceUniversalDataReceived;
            SessionService.Authorized += SessionService_Authorized;

            System.Windows.Application.Current.Exit += (s, e) =>
            {
                IceAdapterClient?.CloseAsync();
            };

            IceService.IceServersReceived += IceService_IceServersReceived;
            replayServerService.ReplayRecorderCreated += ReplayServerService_ReplayRecorderCreated;
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

        private void FillPlayerArgs(StringBuilder args, RatingType ratingType,
            // player info
            string country = "", string clan = "",
            // player rating
            int mean = 1500, int deviation = 500, int games = 0)
        {
            var me = PlayersService.Self;
            if (me is not null)
            {
                if (me.ratings.TryGetValue(ratingType.ToString(), out var rating))
                {
                    mean = (int)rating.rating[0];
                    deviation = (int)rating.rating[1];
                    games = rating.number_of_games;
                }
                country = me.country;
                clan = me.clan ?? string.Empty;
            }
            if (country.Length > 0)
            {
                args.Append("/country ");
                args.Append(country);
                args.Append(' ');
            }
            if (clan.Length > 0)
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
                .ContinueWith(async t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.LogError(t.Exception.Message);
                        await InitializeIce();
                    }
                });
        }

        private void OnGameLaunchDataReceived(object sender, GameLaunchData e)
        {
            Task.Run(() => RunGame(e));
        }

        private async Task RunGame(GameLaunchData e)
        {
            IceAdapterClient ice = null;
            try
            {
                ice = new(Settings.Default.PlayerId.ToString(), Settings.Default.PlayerNick);
                IceAdapterClient = ice;
                await InitializeIce();
            }
            catch(Exception ex)
            {
                SessionService.Send(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
                App.Current.Dispatcher.Invoke(() => NotificationService.ShowExceptionAsync(ex));
                return;
            }
            GameUID = e.uid;

            ice.GpgNetMessageReceived += IceAdapterClient_GpgNetMessageReceived;
            ice.IceMessageReceived += IceAdapterClient_IceMessageReceived;
            ice.ConnectionToGpgNetServerChanged += IceAdapterClient_ConnectionToGpgNetServerChanged;
            // these commands will be ququed and passed after established connect
            var initMode = e.init_mode;
            // 0 - normal
            // 1 - auto
            ice.SetLobbyInitMode("normal");
            ice.PassIceServers(IceService.IceServers);

            var me = PlayersService.Self;

            // game args
            StringBuilder args = new();
            // hide embedded game bug report
            args.Append("/nobugreport ");
            args.Append($"/init init_{e.mod.ToString().ToLower()}.lua ");
            // port from Ice-Adapter status message ["gpgnet"]["local_port"]
            args.Append($"/gpgnet 127.0.0.1:{ice.GpgNetPort} ");
            // append player data
            FillPlayerArgs(args, e.rating_type);
            // append replay stream
            bool isSavingReplay = true;
            if (isSavingReplay)
            {
                var replayPort = ReplayServerService.StartReplayServer();
                args.Append($"/savereplay \"gpgnet://localhost:{replayPort}/{e.uid}/{me.login}.SCFAreplay\"");
            }
            // append game logger
            bool isLogging = false;
            if (isLogging)
            {
                args.Append($"/ log \"C:\\ProgramData\\FAForever\\logs\\game.uid.{e.uid}.log\"");
            }

            //var info = $"{{\"uid\": {e.uid}, \"recorder\": {me.login}, \"featured_mod\": \"{e.mod}\", \"launched_at\": \"time.time\"}}";
            Logger.LogWarning($"Starting game with args: {args}");
            ForgedAlliance = new()
            {
                StartInfo = new()
                {
                    FileName = @"C:\ProgramData\FAForever\bin\ForgedAlliance.exe",
                    Arguments = args.ToString(),
                    UseShellExecute = true
                }
            };
            if (!ForgedAlliance.Start())
            {
                Logger.LogError("Cant start game");
                return;
            }
            IceAdapterClient = ice;
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

        public async Task JoinGame(GameInfoMessage game)
        {
            Logger.LogInformation($"Joining to the game '{game.title}' hosted by '{game.host}' on '{game.mapname}'");

            if (string.IsNullOrWhiteSpace(Settings.Default.PathToGame))
            {
                var model = new SelectPathToGameViewModel();
                var result = await NotificationService.ShowDialog(model);
                if (result is ContentDialogResult.None)
                {
                    return;
                }
                Settings.Default.PathToGame = model.Path;
            }

            if (ForgedAlliance is not null)
            {
                await NotificationService.ShowPopupAsync("Game already is running");
                return;
            }

            if (game.mapname.StartsWith("neroxis"))
            {
                await NotificationService.ShowPopupAsync("Unsupported map");
                return;
            }
            if (game.sim_mods.Count > 0)
            {
                await NotificationService.ShowPopupAsync("Mods not supported");
                return;
            }

            //if (game.FeaturedMod != FeaturedMod.FAF)
            //{
            //    await NotificationService.ShowPopupAsync("Unsupported game mode");
            //    return;
            //}

            string password = string.Empty;

            if (game.password_protected)
            {
                PassPasswordViewModel model = new();
                var result = await NotificationService.ShowDialog(model);
                if (result is ContentDialogResult.None)
                {
                    Logger.LogInformation($"User refused to pass password to game {game.uid} by {game.host}");
                    return;
                }
                password = model.Password;
            }


            LastGame = game;

            // Check mods?
            // ...

            // Check map
            if (!ConfirmMap(game.mapname))
            {
                Logger.LogWarning("Map {1} required to download", game.mapname);

                ContentDialogResult result;

                if (!Settings.Default.AlwaysDownloadMap)
                {
                    result = await NotificationService.ShowDialog("Map required to download:\n" + game.mapname, "Download", "Always download", "Cancel");

                    if (result == ContentDialogResult.None)
                    {
                        Logger.LogWarning($"Refused to download map {game.mapname}");
                        return;
                    }

                    if (result == ContentDialogResult.Secondary)
                    {
                        Logger.LogWarning("Set to always download map");
                        // Always download
                        Settings.Default.AlwaysDownloadMap = true;
                    }
                }
                else
                {
                    Logger.LogWarning("Auto downloading map");
                }

                // Run task for downloading
                //await Task.Run(() => MapsService.Download(new($"https://content.faforever.com/maps/{game.mapname}.zip")));
                var model = await MapsService.DownloadAndExtractAsync(new($"https://content.faforever.com/maps/{game.mapname}.zip"));

                if (!model.IsCompleted)
                {
                    return;
                }

                if (!ConfirmMap(game.mapname))
                {
                    await NotificationService.ShowPopupAsync("Something went wrong on downloading map, try again");
                    return;
                }

                //result = await NotificationService.ShowDialog(model, close: "Hide", secondary: "Cancel");

                    //if (result == ContentDialogResult.Secondary)
                    //{

                    //    Logger.LogWarning($"Cancelled downloading of map {game.mapname}");
                    //    model.Cancel();
                    //    model.Dispose();
                    //    return;
                    //}

                    // because it is .zip file, we cant just wait till model itself complete to download.
                    // we have to wait the service to complete unzip process;
                    //MapsService.DownloadCompleted += OnRequiredMapDownloadCompleted;
            }
            Logger.LogInformation("Map confirmed!");

            // Check current patch
            // ConfirmPatch takes about 1800ms to process
            if (!await ConfirmPatch(game.FeaturedMod)) return;
            Logger.LogInformation("Patch confirmed");

            string command;
            
            command = ServerCommands.JoinGame(game.uid.ToString(), password: password);

            await SessionService.SendAsync(command);
        }

        private async Task<bool> ConfirmPatch(FeaturedMod mod)
        {
            var dataToDownload = await ConfirmPatchFiles(mod);
            if (dataToDownload.Length == 0) return true;

            if ((ForgedAlliance is not null && !ForgedAlliance.HasExited) || (ReplayForgedAlliance is not null && !ReplayForgedAlliance.HasExited))
            {
                await NotificationService.ShowPopupAsync("You cant update patch when game is launched");
                return false;
            }
            // we have patch files to download

            Logger.LogWarning($"Patch {mod} required to download");

            ContentDialogResult result;

            if (!Settings.Default.AlwaysDownloadPatch)
            {
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
                Logger.LogWarning("Patch download didnt complete");
                return false;
            }

            var files = await ConfirmPatchFiles(mod);
            if (files.Length == 0) return true;

            var wrong = string.Empty;
            for (int i = 0; i < files.Length; i++)
            {
                wrong += files[i].Name;
                if (i < files.Length - 1)
                {
                    wrong += ", ";
                }
            }
            await NotificationService.ShowPopupAsync($"Something went wrong. Patch files didn`t match MD5:\n{wrong}");
            return false;
        }

        private bool ConfirmMap(string name) => MapsService.CheckLocalMap(name) switch
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

        private async void OnDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ((DownloadViewModel)sender).Completed -= OnDownloadCompleted;

            if (!e.Cancelled) await JoinGame(LastGame);
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

        public void JoinGame(GameInfoMessage game, string password)
        {
            /* SEND
            "command": "game_join",
            "uid": _,
            "password": _,
            */

            /* REPLY / WRONG
            "command": "notice",
            "style": "info",
            "text": "Bad password (it's case sensitive)."
             */
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

        public async Task HostGame(string title, FeaturedMod mod, string mapName, double? minRating, double? maxRating, GameVisibility visibility = GameVisibility.Friends, bool isRatingResctEnforced = false, string password = null, bool isRehost = false)
        {
            if (ForgedAlliance is not null)
            {
                await NotificationService.ShowPopupAsync("Game is running");
                return;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                await NotificationService.ShowPopupAsync("Title not selected");
                return;
            }

            if (string.IsNullOrWhiteSpace(mapName))
            {
                await NotificationService.ShowPopupAsync("Map not selected");
                return;
            }

            if (!await ConfirmPatch(mod)) return;

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
            if (!ConfirmMap(mapName))
            {
                Logger.LogWarning("Map {1} required to download", mapName);

                ContentDialogResult result;

                if (!Settings.Default.AlwaysDownloadMap)
                {
                    result = await NotificationService.ShowDialog("Map required to download:\n" + mapName, "Download", "Always download", "Cancel");

                    if (result == ContentDialogResult.None)
                    {
                        Logger.LogWarning($"Refused to download map {mapName}");
                        return;
                    }

                    if (result == ContentDialogResult.Secondary)
                    {
                        Logger.LogWarning("Set to always download map");
                        // Always download
                        Settings.Default.AlwaysDownloadMap = true;
                    }
                }
                else
                {
                    Logger.LogWarning("Auto downloading map");
                }

                // Run task for downloading
                //await Task.Run(() => MapsService.Download(new($"https://content.faforever.com/maps/{game.mapname}.zip")));
                var model = await MapsService.DownloadAndExtractAsync(new($"https://content.faforever.com/maps/{mapName}.zip"));

                if (!model.IsCompleted)
                {
                    return;
                }

                if (!ConfirmMap(mapName))
                {
                    await NotificationService.ShowPopupAsync("Something went wrong on downloading map, try again");
                    return;
                }

                //result = await NotificationService.ShowDialog(model, close: "Hide", secondary: "Cancel");

                //if (result == ContentDialogResult.Secondary)
                //{

                //    Logger.LogWarning($"Cancelled downloading of map {game.mapname}");
                //    model.Cancel();
                //    model.Dispose();
                //    return;
                //}

                // because it is .zip file, we cant just wait till model itself complete to download.
                // we have to wait the service to complete unzip process;
                //MapsService.DownloadCompleted += OnRequiredMapDownloadCompleted;
            }
            Logger.LogInformation("Map confirmed!");

            // Check current patch
            if (!await ConfirmPatch(mod)) return;
            Logger.LogInformation("Patch confirmed");

            try
            {
                SessionService.Send(command);
            }
            catch(Exception ex)
            {
                NotificationService.ShowExceptionAsync(ex);
            }
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
            if (!ConfirmMap(mapName))
            {
                var model = await MapsService.DownloadAndExtractAsync(new($"https://content.faforever.com/maps/{mapName}.zip"));

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
