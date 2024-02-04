using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Models.Progress;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafGameLauncher : IGameLauncher
    {
        private readonly ISnackbarService _snackbarService;
        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly IFafLobbyService _fafLobbyService;
        private readonly IFafGamesService _fafGamesService;
        private readonly IMapsService _mapsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsManager _settingsManager;
        private readonly IGameNetworkAdapter _gameNetworkAdapter;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        private Process ForgedAlliance;

        public FafGameLauncher(ISnackbarService snackbarService, IFafLobbyEventsService fafLobbyEventsService, IFafLobbyService fafLobbyService, IFafGamesService fafGamesService, ILogger<FafGameLauncher> logger, IMapsService mapsService, IServiceProvider serviceProvider, ISettingsManager settingsManager, IGameNetworkAdapter gameNetworkAdapter, IUserService userService)
        {
            _snackbarService = snackbarService;
            _fafLobbyEventsService = fafLobbyEventsService;

            _fafLobbyEventsService.GameLaunchDataReceived += _fafLobbyEventsService_GameLaunchDataReceived;
            _fafLobbyService = fafLobbyService;
            _fafGamesService = fafGamesService;
            _logger = logger;
            _mapsService = mapsService;
            _serviceProvider = serviceProvider;
            _settingsManager = settingsManager;
            _gameNetworkAdapter = gameNetworkAdapter;
            _userService = userService;
        }

        private void ShowSnackbar(string message)
        {
            _snackbarService.Show("GameLauncher", message);
        }

        public async Task JoinGameAsync(Game game, string password = null)
        {
            if (game.State != GameState.Open)
            {
                ShowSnackbar("Game state is incorrect. Game must be open to join.");
                return;
            }
            if (game.GameType != GameType.Custom)
            {
                ShowSnackbar("Unsupported game type. Only custom games supported");
                return;
            }
            if (game.FeaturedMod.ToString().ToLower() != "faf")
            {
                ShowSnackbar("Unsupported featured mod. Only faf supported");
                return;
            }
            if (game.SimMods.Any())
            {
                ShowSnackbar("Games with SIM mods currently unsupported");
                return;
            }
            var progress = new Progress<ProgressReport>(x =>
            {
                return;
                _logger.LogInformation(
                    "Progress: {Progress:0.00}%, message: {message}",
                    x.IsIndeterminate ? -1 : x.Progress,
                    x.Message);
            });
            (var mapsService, var patchClient, var gameNetworkAdapter) = GetServices();
            await mapsService.EnsureMapExist(game.Map.RawName, progress);
            await gameNetworkAdapter.PrepareAsync(progress);
            await patchClient.EnsurePatchExist(game.FeaturedMod.ToString(), _settingsManager.Settings.FAForeverLocation, progress: progress);
            await _fafLobbyService.JoinGameAsync(game.Uid, password);
        }

        private void _fafLobbyEventsService_GameLaunchDataReceived(object sender, GameLaunchData e)
            => Task.Run(async () => await HandleGameLaunchData(e))
            .ContinueWith(async x =>
            {
                if (x.IsFaulted) await HandleOnException(null);
            }, TaskScheduler.Default)
            .SafeFireAndForget(x => _logger.LogError(x.Message));
        private async Task HandleGameLaunchData(GameLaunchData data)
        {
            var game = _fafGamesService.GetGame(data.GameUid);
            if (game == null)
            {
                ShowSnackbar("Game not found");
                return;
            }
            if (game.State != GameState.Open)
            {
                ShowSnackbar("Game state is incorrect");
                return;
            }
            var progress = new Progress<ProgressReport>(x =>
            {
                return;
                _logger.LogInformation(
                    "Progress: {Progress:0.00}%, message: {message}",
                    x.IsIndeterminate ? -1 : x.Progress,
                    x.Message);
            });
            (var mapsService, _, var gameNetworkAdapter) = GetServices();
            await mapsService.EnsureMapExist(game.Map.RawName, progress);
            var gpgnetPort = await gameNetworkAdapter.Run(data.GameUid, data.InitMode.ToString().ToLower(), progress);

            var initFile = $"init_{data.FeaturedMod.ToString().ToLower()}.lua";
            if (!File.Exists(Path.Combine(_settingsManager.Settings.FAForeverLocation, "bin", initFile)))
            {
                initFile = "init.lua";
            }

            var args = new StringBuilder();
            args.Append($"/nobugreport /init {initFile} /gpgnet 127.0.0.1:{gpgnetPort} ");

            var country = _userService.GetCountry();
            var clan = _userService.GetClan();
            var rating = _userService.GetRating(data.RatingType.ToString().ToLower());

            if (!string.IsNullOrEmpty(country)) args.Append($"/country {country} ");
            if (!string.IsNullOrEmpty(clan)) args.Append($"/clan {clan} ");

            if (rating != null)
            {
                args.Append($"/mean {(int)rating.rating[0]} ");
                args.Append($"/deviation {(int)rating.rating[1]} ");
                args.Append($"/numgames {rating.number_of_games} ");
            }

            //if (e.GameType is GameType.MatchMaker)
            //{
            //    // matchmaker
            //    arguments.Append($"/{e.faction.ToString().ToLower()} ");
            //    arguments.Append($"/players {e.expected_players} ");
            //    arguments.Append($"/team {e.team} ");
            //    arguments.Append($"/startspot {e.map_position} ");
            //}
            // append replay stream
            //bool isSavingReplay = true;
            //if (isSavingReplay)
            //{
            //    var replayPort = ReplayServerService.StartReplayServer();
            //    arguments.Append($"/savereplay \"gpgnet://localhost:{replayPort}/{e.uid}/{me.login}.SCFAreplay\" ");
            //}
            //var logs = Configuration.GetValue<string>("Paths:GameSession").Replace("{uid}", e.uid.ToString());
            //arguments.Append($"/log \"{logs}\" ");

            ForgedAlliance = new()
            {
                StartInfo = new()
                {
                    FileName = Path.Combine(_settingsManager.Settings.FAForeverLocation, "bin", "ForgedAlliance.exe"),
                    Arguments = args.ToString(),
                    UseShellExecute = true
                }
            };
            var started = ForgedAlliance.Start();
            if (!started)
            {
                throw new InvalidOperationException("Failed to start ForgedAlliance.exe");
            }
            Task.Run(() => ForgedAlliance.WaitForExitAsync())
                .ContinueWith(HandleOnException, TaskScheduler.Default)
                .SafeFireAndForget(x => _logger.LogError(x.Message));
        }
        private async Task HandleOnException(Task task)
        {
            if (task?.IsFaulted == true)
            {
                _logger.LogCritical(task.Exception?.InnerException?.ToString());
            }
            await _fafLobbyService.GameEndedAsync();
            await _gameNetworkAdapter.Stop();
            if (ForgedAlliance != null)
            {
                ForgedAlliance.Close();
                ForgedAlliance.Dispose();
            }
        }
        private (IMapsService, IPatchClient, IGameNetworkAdapter) GetServices()
        {
            return (_mapsService, _serviceProvider.GetService<IPatchClient>(), _gameNetworkAdapter);
        }
    }
}
