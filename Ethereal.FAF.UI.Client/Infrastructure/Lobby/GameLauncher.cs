using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        public void LobbyClient_GameLaunchDataReceived(GameLaunchData e, Player self, IceManager iceManager, Server server,
            LobbyClient lobbyClient)
        {
            Task.Run(() => RunGame(e, self, iceManager, server))
                .ContinueWith(t =>
                {
                    Joining = false;
                    lobbyClient.GameEnded();
                    if (t.IsFaulted)
                    {
                        Logger.LogError(t.Exception.ToString());
                        NotificationService.Notify("Game", $"Launch failed: {t.Exception}", ignoreOs: true);
                        Process?.Kill();
                        Process?.Dispose();
                        Process?.Close();
                        Process = null;
                    }
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
        private async Task RunGame(GameLaunchData e, Player self, IceManager iceManager, Server server)
        {
            LastGameLaunchData = e;
            LastGameUid = e.uid;

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
            iceManager.Initialize(self.Id, self.Login, e.uid, e.game_type is GameType.MatchMaker ? "auto" : "normal");
            //ice.PassIceServers(IceService.IceServers);
            //var me = PlayersService.Self;
            // game args
            StringBuilder arguments = new();
            arguments.Append(string.Join(' ', e.args));
            arguments.Append(' ');
            // hide embedded game bug report
            arguments.Append("/nobugreport ");
            arguments.Append($"/init init_{e.mod.ToString().ToLower()}.lua ");
            // port from Ice-Adapter status message ["gpgnet"]["local_port"]
            arguments.Append($"/gpgnet 127.0.0.1:{iceManager.GpgNetPort} ");

            FillPlayerArgs(arguments, e.rating_type, self);
            if (e.game_type is GameType.MatchMaker)
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
                var maps = Configuration.GetMapsLocation();
                if (MapGenerator.IsGeneratedMap(e.mapname))
                {
                    progress?.Report("Generating map");
                    NotificationService.Notify("Game", "Generating map", ignoreOs: true);
                    await MapGenerator.GenerateMap(e.mapname, maps, default, progress);
                }
                else
                {
                    if (MapsService.IsExist(e.mapname))
                    {
                        await MapsService.DownloadAsync(e.mapname, server.Content.ToString(), $"maps/{e.mapname}.zip", progress, default);
                    }
                }
            }
            Logger.LogTrace("Starting game with next arguments [{args}]", arguments);
            Process = new()
            {
                StartInfo = new()
                {
                    FileName = Path.Combine(Configuration.GetForgedAlliancePatchLocation(), ForgedAllianceHelper.BinFolder, ForgedAllianceHelper.Executable),
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
        public async Task JoinGame(Game game, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var maps = Configuration.GetMapsLocation();
                if (!MapsService.IsExist(game.Mapname))
                {
                    if (MapGenerator.IsGeneratedMap(game.Mapname))
                    {
                        progress?.Report("Generating map");
                        await MapGenerator.GenerateMap(game.Mapname, maps, cancellationToken, progress);
                        game.SmallMapPreview = Path.Combine(maps, game.Mapname, game.Mapname + "_preview.png");
                    }
                    else
                    {
                        await MapsService.DownloadAsync(game.Mapname,
                            game.ServerManager.GetServer().Content.ToString(),
                            game.MapFilePath, progress, cancellationToken);
                    }
                }
                await game.ServerManager.GetPatchClient().UpdatePatch(game.FeaturedMod, 0, false, cancellationToken, progress);
                progress.Report("Joining game");
                game.ServerManager.GetLobbyClient().JoinGame(game.Uid);
            }
            catch
            {
            }
        }
        public async Task WatchGame(long gameId, long playerId, string mapname, FeaturedMod mod, PatchClient patchClient, Server server)
        {
            var maps = Configuration.GetMapsLocation();
            if (!MapsService.IsExist(mapname))
            {
                if (MapGenerator.IsGeneratedMap(mapname))
                {
                    await MapGenerator.GenerateMap(mapname, maps);
                }
                else
                {
                    await MapsService.DownloadAsync(mapname, server.Content.ToString());
                }
            }
            StringBuilder sb = new();
            sb.Append("/nobugreport ");
            sb.Append($"/init init_{mod.ToString().ToLower()}.lua ");
            sb.Append($"/replay gpgnet://lobby.faforever.com/{gameId}/{playerId}.SCFAreplay ");
            sb.Append($"/replayid {gameId} ");
            if (Configuration.GetValue<bool>("Game:Replays:IsLogsEnabled"))
            {
                var log = string.Format(Configuration.GetValue<string>("Game:Replays:Logs"), gameId, playerId);
                sb.Append($"/log \"{log}\"");
            }
            await patchClient.UpdatePatch(mod, 0, false);
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Path.Combine(Configuration.GetForgedAlliancePatchLocation(), ForgedAllianceHelper.BinFolder, ForgedAllianceHelper.Executable),
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
