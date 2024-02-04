using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer.Outgoing;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public interface IFafJavaIceAdapterCallbacks
    {
        /// <summary>
        /// The game connected to the internal GPGNetServer.
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
        [JsonRpcMethod("onConnectionStateChanged")]
        public Task OnConnectionStateChangedAsync(string newState);
        /// <summary>
        /// The game sent a message to the faf-ice-adapter via the internal GPGNetServer.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>
        [JsonRpcMethod("onGpgNetMessageReceived")]
        public Task OnGpgNetMessageReceivedAsync(string header, List<object> chunks);
        /// <summary>
        /// The PeerRelays gathered a local ICE message for connecting to the remote player. This message must be forwarded to the remote peer and set using the iceMsg command.
        /// </summary>
        /// <param name="localPlayerId"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [JsonRpcMethod("onIceMsg")]
        public Task OnIceMsgAsync(long localPlayerId, long remotePlayerId, object message);
        /// <summary>
        /// See <see href="https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceConnectionState"/>
        /// </summary>
        /// <param name="localPlayerId"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [JsonRpcMethod("onIceConnectionStateChanged")]
        public Task OnIceConnectionStateChangedAsync(long localPlayerId, long remotePlayerId, string state);
        /// <summary>
        /// Informs the client that ICE connectivity to the peer is established or unestablished.
        /// </summary>
        /// <param name="localPlayerId"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="connected"></param>
        /// <returns></returns>
        [JsonRpcMethod("onConnected")]
        public Task OnConnectedAsync(long localPlayerId, long remotePlayerId, bool connected);
    }
    internal class FafJavaIceAdapterCallbacks : IFafJavaIceAdapterCallbacks
    {
        private readonly ILogger<FafJavaIceAdapterCallbacks> _logger;
        private readonly FafLobbyService _fafLobbyService;

        public FafJavaIceAdapterCallbacks(IFafLobbyService fafLobbyService,
            ILogger<FafJavaIceAdapterCallbacks> logger)
        {
            _fafLobbyService = (FafLobbyService)fafLobbyService;
            _logger = logger;
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

        public Task OnGpgNetMessageReceivedAsync(string header, List<object> chunks)
        {
            _logger.LogInformation(
                "Message from game: '{header}' '{chunks}'",
                header,
                JsonSerializer.Serialize(chunks, Services.JsonSerializerDefaults.CyrillicJsonSerializerOptions));
            _fafLobbyService.SendCommandToLobby(new OutgoingArgsCommand(header, chunks.ToArray()));
            return Task.CompletedTask;
        }

        public Task OnIceConnectionStateChangedAsync(long localPlayerId, long remotePlayerId, string state)
        {
            _logger.LogInformation(
                "ICE connection state for peer '{remote}' changed to: {state}",
                remotePlayerId,
                state);
            return Task.CompletedTask;
        }

        public Task OnIceMsgAsync(long localPlayerId, long remotePlayerId, object message)
        {
            var utf8 = JsonSerializer.Serialize(message, Services.JsonSerializerDefaults.CyrillicJsonSerializerOptions);
            _logger.LogInformation(
                "ICE message for connection '{local}/{remote}': {msg}",
                localPlayerId,
                remotePlayerId,
                utf8);
            _fafLobbyService.SendCommandToLobby(new OutgoingArgsCommand("IceMsg", remotePlayerId, 
                JsonSerializer.Deserialize<object>(utf8)));
            return Task.CompletedTask;
        }
    }
}
