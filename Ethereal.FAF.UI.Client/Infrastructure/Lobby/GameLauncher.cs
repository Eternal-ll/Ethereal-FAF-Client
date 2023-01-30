using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public enum GameLauncherState
    {
        Idle,
        Joining,
        Launching,
        Running,
        //Updating
    }
    public sealed class GameLauncher
    {
        public event EventHandler<GameLauncherState> StateChanged;
        public event EventHandler<Progress<string>> GameLaunching;

        //private readonly LobbyClient LobbyClient;
        //private readonly PatchClient PatchClient;
        private readonly MapsService MapsService;
        private readonly MapGenerator MapGenerator;
        private readonly NotificationService NotificationService;
        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;
        private readonly PatchWatcher PatchWatcher;

        private Process Process;

        public GameLauncherState State { get; set; }

        public GameLauncher(
            IConfiguration configuration,
            ILogger<GameLauncher> logger,
            //IceManager iceManager,
            //PatchClient patchClient,
            MapsService mapsService,
            MapGenerator mapGenerator,
            NotificationService notificationService,
            PatchWatcher patchWatcher)
        {
            Configuration = configuration;
            //LobbyClient = lobbyClient;
            //IceManager = iceManager;
            Logger = logger;

            //lobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            //lobbyClient.MatchCancelled += LobbyClient_MatchCancelled;
            //PatchClient = patchClient;
            MapsService = mapsService;
            MapGenerator = mapGenerator;
            NotificationService = notificationService;
            PatchWatcher = patchWatcher;
        }
        private long LastGameUid;
        private GameLaunchData LastGameLaunchData;

        public void LobbyClient_MatchCancelled(object sender, MatchCancelled e)
        {
            if (Process is null) return;
            Process.Kill();
            //IceManager.NotifyAboutBadConnections();
        }
        private bool Joining;
        public void LobbyClient_GameLaunchDataReceived(GameLaunchData e, ServerManager serverManager)
        {
            Task.Run(() => RunGame(e, serverManager))
                .ContinueWith(t =>
                {
                    Joining = false;
                    serverManager.GetLobbyClient().GameEnded();
                    if (t.IsFaulted)
                    {
                        Logger.LogError(t.Exception.ToString());
                        NotificationService.Notify("Game", $"Launch failed: {t.Exception}", ignoreOs: true);
                        Process?.Kill();
                        Process?.Dispose();
                        Process?.Close();
                        Process = null;
                    }
                    var iceManager = serverManager.GetIceManager();
                    if (iceManager.IceClient?.IsConnected is true)
                    {
                        iceManager.IceClient.IsStop = true;
                        iceManager.IceClient.Send(IceJsonRpcMethods.Quit());
                        iceManager.IceClient.Disconnect();
                    }
                    iceManager.IceClient?.Dispose();
                    iceManager.IceClient = null;
                    if (iceManager.IceServer is not null)
                    {
                        //iceManager.IceServer.WaitForExit();
                        iceManager.IceServer.Dispose();
                        iceManager.IceServer.Close();
                        iceManager.IceServer = null;
                    }
                    State = GameLauncherState.Idle;
                });
        }
        private void FillPlayerArgs(StringBuilder args, RatingType ratingType, Player self,
            // player info
            string country = "", string clan = null,
            // player rating
            int mean = 1500, int deviation = 500, int games = 0)
        {
            var me = self;
            if (me is not null)
            {
                //if (me.Ratings.TryGetValue(ratingType, out var rating))
                //{
                //    mean = (int)rating.rating[0];
                //    deviation = (int)rating.rating[1];
                //    games = rating.number_of_games;
                //}
                var rating = ratingType switch
                {
                    RatingType.global => me.Ratings.Global,
                    RatingType.ladder_1v1 => me.Ratings.Ladder1V1,
                    RatingType.tmm_2v2 => me.Ratings.Tmm2V2,
                    RatingType.tmm_4v4_full_share => me.Ratings.Tmm4V4FullShare
                };
                if (rating is null) rating = new Rating()
                {
                    rating = new double[] { 1500, 500 }
                };
                mean = 1900;//(int)rating.rating[0];
                deviation = 50;// (int)rating.rating[1];
                games = rating.number_of_games;
                country = me.Country;
                clan = me.Clan;
            }
            if (country.Length > 0)
            {
                args.Append($"/country {country} ");
            }
            if (clan?.Length > 0)
            {
                args.Append($"/clan {clan} ");
            }
            args.Append($"/mean {mean} ");
            args.Append($"/deviation {deviation} ");
            args.Append($"/numgames {games} ");
        }
        private async Task RunGame(GameLaunchData e, ServerManager serverManager)
        {
            var self = serverManager.Self;
            var iceManager = serverManager.GetIceManager();
            var server = serverManager.Server;

            LastGameLaunchData = e;
            LastGameUid = e.GameUid;

            //if (e.game_type is GameType.Custom && test == 1)
            //{
            //    _ = Task.Run(async () =>
            //    {
            //        await Task.Delay(6000);
            //        if (!IceManager.AllConnected)
            //        {
            //            IsRestart = true;
            //            Process.Kill();
            //            test++;
            //        }
            //        else
            //        {

            //        }
            //    });
            //}

            var progressSource = new Progress<string>();
            //GameLaunching?.Invoke(this, progressSource);
            var progress = (IProgress<string>)progressSource;

            //OnStateChanged(GameLauncherState.Launching);
            iceManager.Initialize(self.Id, self.Login, e.GameUid, e.GameType is GameType.MatchMaker ? "auto" : "normal");
            //ice.PassIceServers(IceService.IceServers);
            //var me = PlayersService.Self;
            // game args
            StringBuilder arguments = new();
            arguments.Append(string.Join(' ', e.args));
            arguments.Append(' ');
            // hide embedded game bug report
            arguments.Append("/nobugreport ");
            arguments.Append($"/init init_{e.FeaturedMod.ToString().ToLower()}.lua ");
            // port from Ice-Adapter status message ["gpgnet"]["local_port"]
            arguments.Append($"/gpgnet 127.0.0.1:{iceManager.GpgNetPort} ");

            FillPlayerArgs(arguments, e.RatingType, self);
            if (e.GameType is GameType.MatchMaker)
            {
                // matchmaker
                arguments.Append($"/{e.faction.ToString().ToLower()} ");
                arguments.Append($"/players {e.expected_players} ");
                arguments.Append($"/team {e.team} ");
                arguments.Append($"/startspot {e.map_position} ");
            }

            // append replay stream
            //bool isSavingReplay = true;
            //if (isSavingReplay)
            //{
            //    var replayPort = ReplayServerService.StartReplayServer();
            //    arguments.Append($"/savereplay \"gpgnet://localhost:{replayPort}/{e.uid}/{me.login}.SCFAreplay\" ");
            //}
            //var logs = Configuration.GetValue<string>("Paths:GameSession").Replace("{uid}", e.uid.ToString());
            //arguments.Append($"/log \"{logs}\" ");

            if (!string.IsNullOrWhiteSpace(e.mapname))
            {
                await MapsService.EnsureMapExistAsync(e.mapname, serverManager.GetContentClient());
            }
            Logger.LogTrace("Starting game with next arguments [{args}]", arguments);
            Process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetForgedAllianceExecutable(),
                    Arguments = arguments.ToString(),
                    UseShellExecute = true
                }
            };
            NotificationService.Notify("Game", "Launching game", ignoreOs: true);
            if (!Process.Start())
            {
                Logger.LogError("Cant start game");
                throw new Exception("Can`t start \"Supreme Commander: Forged Alliance\"");
            }
            //OnStateChanged(GameLauncherState.Running);
            await Process.WaitForExitAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public void WatchGame(Game game) => WatchGame(game.Uid, game.Players.First().Id, game.FeaturedMod);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="playerId"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public void WatchGame(long gameId, long playerId, FeaturedMod mod)
        {
            StringBuilder sb = new();
            sb.Append("/nobugreport ");
            sb.Append($"/init init_{mod.ToString().ToLower()}.lua ");
            // TODO - replace hard code from config
            sb.Append($"/replay gpgnet://lobby.faforever.com/{gameId}/{playerId}.SCFAreplay ");
            sb.Append($"/replayid {gameId} ");
            if (Configuration.GetValue<bool>("Game:Replays:IsLogsEnabled"))
            {
                var log = string.Format(Configuration.GetValue<string>("Game:Replays:Logs"), gameId, playerId);
                sb.Append($"/log \"{log}\"");
            }
            Logger.LogInformation("Starting replay watching with next arguments [{args}]", sb.ToString());
            //await patchClient.UpdatePatch(mod, 0, false);
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetForgedAllianceExecutable(),
                    Arguments = sb.ToString(),
                    UseShellExecute = true
                }
            };
            process.Start();
        }
        private void OnStateChanged(GameLauncherState state)
        {
            State = state;
            StateChanged?.Invoke(this, state);
        }
    }
}
