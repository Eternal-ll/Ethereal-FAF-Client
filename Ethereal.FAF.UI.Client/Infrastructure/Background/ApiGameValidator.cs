using beta.Models.API.Base;
using Ethereal.FAF.API.Client.Models.Game;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Background
{
    internal class ApiGameValidator : BackgroundService
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly ILogger Logger;
        private readonly GamesViewModel GamesViewModel;

        public ApiGameValidator(IHttpClientFactory httpClientFactory, GamesViewModel gamesViewModel, ILogger<ApiGameValidator> logger)
        {
            HttpClientFactory = httpClientFactory;
            GamesViewModel = gamesViewModel;
            Logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            ValidateGames(stoppingToken);

        private async Task ValidateGames(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var gamesView = GamesViewModel.GamesView;
                if (gamesView is null || !GamesViewModel.IsLive) 
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }
                var games = gamesView
                    .Cast<Game>()
                    .Where(g => g.State is GameState.Playing && g.ApiGameValidatyState is ApiGameValidatyState.UNKNOWN)
                    .ToArray();
                if (games is null || !games.Any())
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                using var client = HttpClientFactory.CreateClient();
                var url = $"https://api.faforever.com/data/game?filter=(id=in=({string.Join(',', games.Select(g => g.Uid))}))";
                await client.GetFromJsonAsync<ApiUniversalResult<ApiGame[]>>(url, stoppingToken)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            return;
                        }

                        foreach (var api in t.Result.Data)
                        {
                            var game = games.FirstOrDefault(g => g.Uid == api.Id);
                            if (game is null) continue;
                            game.ApiGameValidatyState = api.Validity;
                            game.VictoryCondition = api.VictoryCondition;
                        }
                    }, stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
