using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    internal class FafJavaIceAdapterCallbacks : IFafJavaIceAdapterCallbacks
    {
        private readonly ILogger<FafJavaIceAdapterCallbacks> _logger;
        private readonly IFafLobbyActionClient _fafLobbyActionClient;

        public FafJavaIceAdapterCallbacks(
            ILogger<FafJavaIceAdapterCallbacks> logger,
            IFafLobbyActionClient fafLobbyActionClient)
        {
            _logger = logger;
            _fafLobbyActionClient = fafLobbyActionClient;
        }

        public Task OnConnectedAsync(long localPlayerId, long remotePlayerId, bool connected)
        {
            if (connected)
            {
                _logger.LogInformation(
                    "Connection between '{local}' and '{remote}' has been established",
                    localPlayerId,
                    remotePlayerId);
            }
            else
            {
                _logger.LogInformation(
                    "Connection between '{local}' and '{remote}' has been lost",
                    localPlayerId,
                    remotePlayerId);
            }
            return Task.CompletedTask;
        }

        public Task OnConnectionStateChangedAsync(string newState)
        {
            _logger.LogInformation(
                "ICE adapter connection state changed to: {state}",
                newState);
            return Task.CompletedTask;
        }

        public async Task OnGpgNetMessageReceivedAsync(string header, object[] chunks)
        {
            _logger.LogInformation(
                "Message from game: '{header}' '{chunks}'",
                header,
                JsonSerializer.Serialize(chunks, Services.JsonSerializerDefaults.CyrillicJsonSerializerOptions));
            await _fafLobbyActionClient.SendTargetActionAsync(header, "game", chunks);
        }

        public Task OnIceConnectionStateChangedAsync(long localPlayerId, long remotePlayerId, string state)
        {
            _logger.LogInformation(
                "ICE connection state for peer '{remote}' changed to: {state}",
                remotePlayerId,
                state);
            return Task.CompletedTask;
        }

        public async Task OnIceMsgAsync(long localPlayerId, long remotePlayerId, string message)
        {
            _logger.LogInformation(
                "ICE message for connection '{local}/{remote}': {msg}",
                localPlayerId,
                remotePlayerId,
                message);
            await _fafLobbyActionClient.SendTargetActionAsync("IceMsg", "game", [remotePlayerId, message]);
        }
    }
}
