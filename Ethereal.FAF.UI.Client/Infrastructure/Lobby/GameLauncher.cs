using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class GameLauncher
    {
        private readonly LobbyClient LobbyClient;
        private readonly PatchClient PatchClient;
        private readonly IceManager IceManager;
        private readonly TokenProvider TokenProvider;
        private readonly ILogger Logger;

        public GameLauncher(LobbyClient lobbyClient, ILogger<GameLauncher> logger, IceManager iceManager, TokenProvider tokenProvider, PatchClient patchClient)
        {
            LobbyClient = lobbyClient;
            IceManager = iceManager;
            Logger = logger;

            lobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            TokenProvider = tokenProvider;
            PatchClient = patchClient;
        }
        public static string gameId;
        public Process ForgedAlliance;

        private void LobbyClient_GameLaunchDataReceived(object sender, GameLaunchData e)
        {
            Task.Run(() => RunGame(e))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
                });
        }
        private async Task RunGame(GameLaunchData e)
        {
            gameId = e.uid.ToString();
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
            // append player data
            arguments.Append(" /mean 1500 /deviation 500 /country RU");
            //FillPlayerArgs(arguments, e.rating_type);
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

            //await ice.CloseAsync();
            //ice.GpgNetMessageReceived -= IceAdapterClient_GpgNetMessageReceived;
            //ice.IceMessageReceived -= IceAdapterClient_IceMessageReceived;
            //ice.ConnectionToGpgNetServerChanged -= IceAdapterClient_ConnectionToGpgNetServerChanged;
            //ice = null;

            LobbyClient.SendAsync(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
        }
        public async Task JoinGame(GameInfoMessage game)
        {
            await PatchClient.UpdatePatch(game.FeaturedMod, TokenProvider.TokenBearer.AccessToken);
        }
    }
}
