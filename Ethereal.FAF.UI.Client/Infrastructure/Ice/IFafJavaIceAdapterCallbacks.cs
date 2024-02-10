using StreamJsonRpc;
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
        public Task OnGpgNetMessageReceivedAsync(string header, object[] chunks);
        /// <summary>
        /// The PeerRelays gathered a local ICE message for connecting to the remote player. This message must be forwarded to the remote peer and set using the iceMsg command.
        /// </summary>
        /// <param name="localPlayerId"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [JsonRpcMethod("onIceMsg")]
        public Task OnIceMsgAsync(long localPlayerId, long remotePlayerId, string message);
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
}
