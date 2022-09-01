﻿using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class GameLauncher
    {
        public event EventHandler LeftFromGame;

        private readonly LobbyClient LobbyClient;
        private readonly PatchClient PatchClient;
        private readonly MapsService MapsService;
        private readonly MapGenerator MapGenerator;
        private readonly IceManager IceManager;
        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;

        private readonly IHttpClientFactory HttpClientFactory;

        public long? LastGameUID;
        public Process Process;

        public GameLauncher(IConfiguration configuration, LobbyClient lobbyClient, ILogger<GameLauncher> logger, IceManager iceManager, PatchClient patchClient, IHttpClientFactory httpClientFactory, MapsService mapsService, MapGenerator mapGenerator)
        {
            Configuration = configuration;
            LobbyClient = lobbyClient;
            IceManager = iceManager;
            Logger = logger;

            lobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            PatchClient = patchClient;
            HttpClientFactory = httpClientFactory; MapsService = mapsService;
            MapGenerator = mapGenerator;
        }

        private void LobbyClient_GameLaunchDataReceived(object sender, GameLaunchData e)
        {
            Task.Run(() => RunGame(e))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
                });
        }
        private void FillPlayerArgs(StringBuilder args, RatingType ratingType,
            // player info
            string country = "", string clan = null,
            // player rating
            int mean = 1500, int deviation = 500, int games = 0)
        {
            var me = LobbyClient.Self;
            if (me is not null)
            {
                //if (me.Ratings.TryGetValue(ratingType, out var rating))
                //{
                //    mean = (int)rating.rating[0];
                //    deviation = (int)rating.rating[1];
                //    games = rating.number_of_games;
                //}
                var rating = me.Ratings.Global;
                mean = (int)rating.rating[0];
                deviation = (int)rating.rating[1];
                games = rating.number_of_games;
                country = me.Country;
                clan = me.Clan;
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
        private async Task RunGame(GameLaunchData e)
        {
            LastGameUID = e.uid;
            LobbyClient.LastGameUID = LastGameUID;
            var me = LobbyClient.Self;
            IceManager.Initialize(me.Id.ToString(), me.Login);
            IceManager.IceClient.SetLobbyInitMode(e.init_mode
                .ToString()
                .ToLower());

            //ice.PassIceServers(IceService.IceServers);

            //var me = PlayersService.Self;
            // game args
            StringBuilder arguments = new();
            // hide embedded game bug report
            arguments.Append("/nobugreport ");
            arguments.Append($"/init init_{e.mod.ToString().ToLower()}.lua ");
            // port from Ice-Adapter status message ["gpgnet"]["local_port"]
            arguments.Append($"/gpgnet 127.0.0.1:{IceManager.GpgNetPort} ");

            FillPlayerArgs(arguments, e.rating_type);
            if (e.init_mode == GameInitMode.Auto)
            {
                // matchmaker
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
            // append game logger
            bool isLogging = false;
            if (isLogging)
            {
                var logs = Configuration.GetValue<string>("Paths:GameLogs");
                if (string.IsNullOrWhiteSpace(logs))
                {

                }
                else
                {
                    logs = string.Format(logs, e.uid);
                    arguments.Append($"/log \"{logs}\" ");
                }
            }
            Logger.LogWarning($"Starting game with args: {arguments}");
            Process = new()
            {
                StartInfo = new()
                {
                    FileName = Configuration.GetValue<string>("Paths:Patch") + "bin/ForgedAlliance.exe",
                    Arguments = arguments.ToString(),
                    UseShellExecute = true
                }
            };
            try
            {
                if (!Process.Start())
                {
                    Logger.LogError("Cant start game");
                    throw new Exception("Can`t start \"Supreme Commander: Forged Alliance\"");
                }
                await Process.WaitForExitAsync();
            }
            catch
            {

            }
            IceManager.IceClient.SendAsync(IceJsonRpcMethods.Quit());
            IceManager.IceClient.DisconnectAsync();
            IceManager.IceClient.Dispose();
            IceManager.IceServer.Close();
            //IceManager.IceServer?.Kill();
            Process.Kill();
            Process.Dispose();
            Process = null;
            LeftFromGame?.Invoke(this, null);
            //await ice.CloseAsync();
            //ice.GpgNetMessageReceived -= IceAdapterClient_GpgNetMessageReceived;
            //ice.IceMessageReceived -= IceAdapterClient_IceMessageReceived;
            //ice.ConnectionToGpgNetServerChanged -= IceAdapterClient_ConnectionToGpgNetServerChanged;
            //ice = null;

            LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
            LastGameUID = null;
            LobbyClient.LastGameUID = null;
        }
        public async Task JoinGame(GameInfoMessage game, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            if (!MapsService.IsExist(game.Mapname))
            {
                if (game.Mapname.StartsWith("neroxis_map_generator_"))
                {
                    await MapGenerator.GenerateMap(game.Mapname, MapsService.MapsFolder, cancellationToken, progress);

                    game.SmallMapPreview = MapsService.MapsFolder + game.Mapname + '/' + game.Mapname + "_preview.png";
                    game.OnPropertyChanged(nameof(game.SmallMapPreview));
                }
                else
                {
                    await MapsService.DownloadAsync(game.Mapname, game.MapFilePath, progress, cancellationToken);
                }
            }

            await PatchClient.UpdatePatch(game.FeaturedMod, 0, false, cancellationToken, progress);

            if (cancellationToken.IsCancellationRequested) return;
            LobbyClient.SendAsync(ServerCommands.JoinGame(game.Uid.ToString()));
        }
    }
}
