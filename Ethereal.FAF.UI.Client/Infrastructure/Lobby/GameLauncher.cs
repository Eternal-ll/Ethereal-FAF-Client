using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class GameLauncher
    {
        public event EventHandler LeftFromGame;

        private readonly LobbyClient LobbyClient;
        private readonly PatchClient PatchClient;
        private readonly IceManager IceManager;
        private readonly TokenProvider TokenProvider;
        private readonly ILogger Logger;

        private readonly IHttpClientFactory HttpClientFactory;

        public long? LastGameUID;
        public Process ForgedAlliance;

        public GameLauncher(LobbyClient lobbyClient, ILogger<GameLauncher> logger, IceManager iceManager, TokenProvider tokenProvider, PatchClient patchClient, IHttpClientFactory httpClientFactory)
        {
            LobbyClient = lobbyClient;
            IceManager = iceManager;
            Logger = logger;

            lobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            TokenProvider = tokenProvider;
            PatchClient = patchClient;
            HttpClientFactory = httpClientFactory;
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
            var jwt = TokenProvider.JwtSecurityToken;
            if (jwt is null)
            {
                Logger.LogError("Access token is null, cant get player data");
                return;
            }
            if (!jwt.Payload.TryGetValue("ext", out var ext))
            {
                Logger.LogError("User data not found in JWT payload [{}]", jwt.Payload);
                return;
            }
            var login = JsonSerializer.Deserialize<Ext>(ext.ToString()).Username;
            var id = jwt.Subject;
            IceManager.Initialize(id, login);
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
            //bool isLogging = false;
            //if (isLogging)
            //{
            //    arguments.Append($"/log \"C:\\ProgramData\\FAForever\\logs\\game.uid.{e.uid}.log\" ");
            //}
            Logger.LogWarning($"Starting game with args: {arguments}");
            ForgedAlliance = new()
            {
                StartInfo = new()
                {
                    FileName = @"C:\ProgramData\FAForever\bin\ForgedAllianceBAC.exe",
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
            IceManager.IceClient.SendAsync(IceJsonRpcMethods.Quit());
            IceManager.IceClient.DisconnectAsync();
            IceManager.IceClient.Dispose();
            IceManager.IceServer?.Process?.Kill();
            ForgedAlliance.Close();
            ForgedAlliance.Dispose();
            ForgedAlliance = null;
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
        public async Task JoinGame(GameInfoMessage game, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var map = Environment.ExpandEnvironmentVariables(@"C:\Users\%username%\Documents\My Games\Gas Powered Games\Supreme Commander Forged Alliance\Maps\");

            if (!Directory.Exists(map + game.Mapname))
            {
                var client = HttpClientFactory.CreateClient();
                using var fs = new FileStream(game.Mapname + ".zip", FileMode.Create);
                var response = await client.GetAsync($"https://content.faforever.com/{game.MapFilePath}", cancellationToken);
                progress?.Report($"Downloading map [{game.Mapname + ".zip"}]");
                await response.Content.CopyToAsync(fs, cancellationToken);
                fs.Close();
                progress?.Report($"Extracting map [{game.Mapname + ".zip"}]");
                ZipFile.ExtractToDirectory(game.Mapname + ".zip", map, true);
                File.Delete(game.Mapname + ".zip");
            }


            await PatchClient.UpdatePatch(game.FeaturedMod, TokenProvider.TokenBearer.AccessToken, 0, false, cancellationToken, progress);

            if (cancellationToken.IsCancellationRequested) return;
            LobbyClient.SendAsync(ServerCommands.JoinGame(game.Uid.ToString()));
        }
    }
}
