using beta.Models.Server;
using System.Text;
using System.Text.Json;

namespace beta.Models.Ice
{
    public static class IceJsonRpcMethods
    {
        //public static string SetIceServers(IceServerData[] servers)
        //{
        //    StringBuilder sb = new();
        //    for (int i = 0; i < servers.Length; i++)
        //    {
        //        sb.Append(JsonSerializer.Serialize(servers[i]));

        //        if (i < servers.Length - 1) sb.Append(',');
        //    }
        //    return $"{{\"method\": \"setIceServers\",\"params\": [[{sb}]], \"jsonrpc\": \"2.0\"}}";
        //}
        public static string SetLobbyInitMode(string mode) => $"{{\"method\": \"setLobbyInitMode\",\"params\": [\"{mode}\"], \"jsonrpc\": \"2.0\"}}";
        /// <summary>
        /// Polls the current status of the faf-ice-adapter.
        /// </summary>
        /// <param name="id">id?</param>
        /// <returns></returns>
        public static string AskStatus(int id) => $"{{\"method\": \"status\", \"params\": [], \"jsonrpc\": \"2.0\", \"id\": {id}}}";
        /// <summary>
        /// Send an arbitrary message to the game.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>
        public static string SendToGpgNet(string header, string[] chunks)
        {
            //StringBuilder sb = new();
            //for (int i = 0; i < servers.Length; i++)
            //{
            //    sb.Append(JsonSerializer.Serialize(servers[i]));

            //    if (i < servers.Length - 1) sb.Append(',');
            //}
            return $"{{\"method\": \"sendToGpgNet\", \"params\": [], \"jsonrpc\": \"2.0\"}}";
        }
        /// <summary>
        /// Gracefully shuts down the faf-ice-adapter.
        /// </summary>
        /// <returns></returns>
        public static string Quit() => $"{{\"method\": \"quit	\", \"params\": [], \"jsonrpc\": \"2.0\"}}";
        /// <summary>
        /// Tell the game to create the lobby and host game on Lobby-State.
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public static string HostGame(string mapName) => $"{{\"method\": \"hostGame\", \"params\": [\"{mapName}\"], \"jsonrpc\": \"2.0\"}}";
        /// <summary>
        /// Create a PeerRelay and tell the game to connect to the remote peer with offer/answer mode.
        /// </summary>
        /// <param name="remotePlayerLogin"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="offer"></param>
        /// <returns></returns>
        public static string JoinGame(string remotePlayerLogin, int remotePlayerId, bool offer = false) =>
            $"{{\"method\": \"joinGame\", \"params\": [], \"jsonrpc\": \"2.0\"}}";
        /// <summary>
        /// Create a PeerRelay and tell the game to connect to the remote peer with offer/answer mode.
        /// </summary>
        /// <param name="remotePlayedId"></param>
        /// <returns></returns>
        public static string ConnectToPerr(int remotePlayedId) => $"{{\"method\": \"connectToPeer\", \"params\": [{remotePlayedId}], \"jsonrpc\": \"2.0\"}}";
        /// <summary>
        /// Destroy PeerRelay and tell the game to disconnect from the remote peer.
        /// </summary>
        /// <param name="remotePlayedId"></param>
        /// <returns></returns>
        public static string DisconnectFromPeer(int remotePlayedId) => $"{{\"method\": \"disconnectFromPeer\", \"params\": [{remotePlayedId}], \"jsonrpc\": \"2.0\"}}";
        public static string UniversalMethod(string method, string args) => $"{{\"method\": \"{method}\", \"params\": {args}, \"jsonrpc\": \"2.0\"}}";
    }
}
