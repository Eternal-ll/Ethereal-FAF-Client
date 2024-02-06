using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public interface IFafJavaIceAdapterClient
    {
        /// <summary>
        /// Gracefully shuts down the faf-ice-adapter.
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("quit")]
        public Task QuitAsync();
        /// <summary>
        /// Tell the game to create the lobby and host game on Lobby-State.
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        [JsonRpcMethod("hostGame")]
        public Task HostGameAsync(string mapName);
        /// <summary>
        /// Tell the game to create the Lobby, create a PeerRelay in answer mode and join the remote game.
        /// </summary>
        /// <param name="remotePlayerLogin"></param>
        /// <param name="remotePlayerId"></param>
        /// <returns></returns>
        [JsonRpcMethod("joinGame")]
        public Task JoinGameAsync(string remotePlayerLogin, long remotePlayerId);
        /// <summary>
        /// Create a PeerRelay and tell the game to connect to the remote peer with offer/answer mode.
        /// </summary>
        /// <param name="remotePlayerLogin"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="offer"></param>
        /// <returns></returns>
        [JsonRpcMethod("connectToPeer")]
        public Task ConnectToPeerAsync(string remotePlayerLogin, long remotePlayerId, bool offer);
        /// <summary>
        /// Destroy PeerRelay and tell the game to disconnect from the remote peer.
        /// </summary>
        /// <param name="remotePlayerId"></param>
        /// <returns></returns>
        [JsonRpcMethod("disconnectFromPeer")]
        public Task DisconnectFromPeerAsync(long remotePlayerId);
        /// <summary>
        /// Set the lobby mode the game will use.<br/>
        /// Supported values are "normal" for normal lobby and "auto" for automatch lobby (aka ladder).
        /// </summary>
        /// <param name="lobbyInitMode"></param>
        /// <returns></returns>
        [JsonRpcMethod("setLobbyInitMode")]
        public Task SetLobbyInitModeAsync(string lobbyInitMode);
        /// <summary>
        /// Add the remote ICE message to the PeerRelay to establish a connection.
        /// </summary>
        /// <param name="remotePlayerId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [JsonRpcMethod("iceMsg")]
        public Task IceMsgAsync(long remotePlayerId, string msg);
        /// <summary>
        /// Send an arbitrary message to the game.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>
        [JsonRpcMethod("sendToGpgNet")]
        public Task SendToGpgNetAsync(string header, params string[] chunks);
        /// <summary>
        /// ICE server array for use in webrtc.
        /// Must be called before joinGame/connectToPeer.<br/>
        /// See <see href="https://developer.mozilla.org/en-US/docs/Web/API/RTCIceServer"/>
        /// </summary>
        /// <param name="iceServers"></param>
        /// <returns></returns>
        [JsonRpcMethod("setIceServers")]
        public Task SetIceServersAsync(List<IceServer> iceServers);
        /// <summary>
        /// Polls the current status of the faf-ice-adapter.
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("status")]
        [Obsolete]
        public Task<object> StatusAsync();
    }
}
