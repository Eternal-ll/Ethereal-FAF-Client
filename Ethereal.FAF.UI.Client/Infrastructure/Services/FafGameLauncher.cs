using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Linq;
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

        public FafGameLauncher(ISnackbarService snackbarService, IFafLobbyEventsService fafLobbyEventsService, IFafLobbyService fafLobbyService, IFafGamesService fafGamesService)
        {
            _snackbarService = snackbarService;
            _fafLobbyEventsService = fafLobbyEventsService;

            _fafLobbyEventsService.GameLaunchDataReceived += _fafLobbyEventsService_GameLaunchDataReceived;
            _fafLobbyService = fafLobbyService;
            _fafGamesService = fafGamesService;
        }

        private void ShowSnackbar(string message)
        {
            _snackbarService.Show("GameLauncher", message);
        }

        public async Task JoinGameAsync(Game game)
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
            if (game.SimMods.Any())
            {
                ShowSnackbar("Games with SIM mods currently unsupported");
                return;
            }
            if (game.IsMapgen)
            {
                ShowSnackbar("Generated maps currently unsupported");
                return;
            }
            await _fafLobbyService.JoinGameAsync(game.Uid);
        }

        private void _fafLobbyEventsService_GameLaunchDataReceived(object sender, GameLaunchData e)
            => HandleGameLaunchData(e)
            .ContinueWith(HandleOnException, TaskContinuationOptions.OnlyOnFaulted)
            .SafeFireAndForget();
        private async Task HandleGameLaunchData(GameLaunchData data)
        {
            throw new Exception();
        }
        private Task HandleOnException(Task task)
        {
            return _fafLobbyService.GameEndedAsync();
        }
    }
}
