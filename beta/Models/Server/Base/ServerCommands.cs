using System;

namespace beta.Models.Server.Base
{
    /// <summary>
    /// JSON outgoing commands
    /// </summary>
    public static class ServerCommands
    {
        /// <summary>
        /// JSON for authentication on lobby-server
        /// </summary>
        /// <param name="accessToken">Access token from OAuth2</param>
        /// <param name="uid">Generated UID from faf-uid.exe</param>
        /// <param name="session">Session id from lobby-server</param>
        /// <returns></returns>
        public static string PassAuthentication(string accessToken, string uid, string session) =>
            $"{{\"command\":\"auth\", \"token\": \"{accessToken}\", \"unique_id\": \"{uid}\", \"session\": {session}}}";
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="uid">Game uid</param>
        /// <param name="gamePort">Port for Ice?</param>
        /// <returns></returns>
        public static string JoinGame(string uid, string gamePort = "0") => 
            $"{{\"command\":\"game_join\", \"uid\": {uid}, \"gameport\":{gamePort}}}";
        /// <summary>
        /// Join to game with password
        /// </summary>
        /// <param name="uid">Game uid</param>
        /// <param name="password">Password from game</param>
        /// <param name="gamePort">Port for Ice?</param>
        /// <returns></returns>
        public static string JoinGame(string uid, string password, string gamePort = "0") =>
            $"{{\"command\":\"game_join\", \"uid\": {uid}, \"gameport\":{gamePort} {(!string.IsNullOrWhiteSpace(password) ? $"\"password\":{password}" : "")}}}";

        /// <summary>
        /// JSON command for hosting game
        /// </summary>
        /// <param name="title"></param>
        /// <param name="gameMod">Game mod like faf/fafdevelop/fafbeta</param>
        /// <param name="visibility">Public or Friends</param>
        /// <param name="mapName">Map name</param>
        /// <param name="password">Secure game with password</param>
        /// <param name="isRehost">Is game rehosting</param>
        /// <returns></returns>
        public static string HostGame(string title, string gameMod, string mapName, string visibility = "public", string password = null, bool isRehost = false) =>
            $"{{\"command\":\"game_host\", \"title\": \"{title}\", \"mod\":\"{gameMod}\", \"visibility\": \"{visibility}\", \"mapname\":\"{mapName}\", \"password\":" +
            password is null ? "null," : $"\"{password}\", \"is_rehost\":{isRehost} }}";

        /// <summary>
        /// Add to friends
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string AddFriend(int id) => $"{{\"command\": \"social_add\", \"friend\": {id}}}";
        /// <summary>
        /// Remove from friends
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string RemoveFriend(int id) => $"{{\"command\": \"social_remove\", \"friend\": {id}}}";
        /// <summary>
        /// Add to foes
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string AddFoe(int id) => $"{{\"command\": \"social_add\", \"friend\": {id}}}";
        /// <summary>
        /// Remove from foes
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string RemoveFoe(int id) => $"{{\"command\": \"social_remove\", \"friend\": {id}}}";

        /// <summary>
        /// Requests MatchMaker info
        /// </summary>
        /// <returns></returns>
        public static string RequestMatchMakerInfo => "{\"command\": \"matchmaker_info\"}";

        /// <summary>
        /// Request ICE servers for Ice Adapter
        /// </summary>
        public static string RequestIceServers => "{\"command\": \"ice_servers\"}";

        /// <summary>
        /// Invite player to party
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string InviteToParty(string id) => $"{{\"command\": \"invite_to_party\", \"recipient_id\": {id}}}";
        /// <summary>
        /// Restore game session
        /// </summary>
        /// <param name="uid">Game uid</param>
        /// <returns></returns>
        public static string RestoreGameSession(string uid) => $"{{\"command\": \"restore_game_session\", \"game_id\": {uid}}}";

        /// <summary>
        /// Join to MatchMaking queue
        /// </summary>
        /// <param name="queue">Queue name</param>
        /// <returns></returns>
        public static string JoinToMatchMakingQueue(string queue) => $"{{\"command\": \"game_matchmaking\", \"queue_name\": \"{queue}\", \"state\": \"start\"}}";

        // #"{{\"command\": \"set_party_factions\", \"factions\": }}";
        // TODO
        public static string SetPartyFactions() => throw new NotImplementedException();

        /// <summary>
        /// Accept invite to MatchMaker party
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string AcceptPartyInvite(string id) => $"{{\"command\":\"accept_party_invite\", \"sender_id\": {id}}}";
        /// <summary>
        /// Kick player from MatchMaker party
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string KickPlayerFromParty(string id) => $"{{\"command\":\"kick_player_from_party\", \"kicked_player_id\": {id}}}";
        /// <summary>
        /// Leave from MatchMaker party
        /// </summary>
        /// <returns></returns>
        public static string LeaveFromParty => "{\"command\": \"leave_party\"}";

        public static string Ping => "{\"command\":\"ping\"}";
        public static string Pong => "{\"command\":\"pong\"}";


        // GAME GPGNET Commands
        public static string UniversalGameCommand(string command, string args) => $"{{\"command\": \"{command}\", \"target\": \"game\", \"args\": {args}}}";

        // ICE

        //{
        //  "command": "IceMsg",
        //  "target": "game",
        //  "args": [66531, "{\"srcId\":302176,\"destId\":66531,\"password\":\"5aolj0n984or5vn1907ccn3bp8\",\"ufrag\":\"fsaa71fvi2m4nq\",\"candidates\":[{\"foundation\":\"1\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"192.168.26.1\",\"port\":6231,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"0\",\"relPort\":0},{\"foundation\":\"2\",\"protocol\":\"udp\",\"priority\":2130706431,\"ip\":\"fe80:0:0:0:9806:9143:b3b6:bfae\",\"port\":6231,\"type\":\"HOST_CANDIDATE\",\"generation\":0,\"id\":\"1\",\"relPort\":0},{\"foundation\":\"3\",\"protocol\":\"udp\",\"priority\":1677724415,\"ip\":\"85.26.234.215\",\"port\":60772,\"type\":\"SERVER_REFLEXIVE_CANDIDATE\",\"generation\":0,\"id\":\"2\",\"relAddr\":\"192.168.26.1\",\"relPort\":6231},{\"foundation\":\"4\",\"protocol\":\"udp\",\"priority\":2815,\"ip\":\"116.202.155.226\",\"port\":15152,\"type\":\"RELAYED_CANDIDATE\",\"generation\":0,\"id\":\"3\",\"relAddr\":\"85.26.234.215\",\"relPort\":60772}]}"]
        //  }
        //public static string IceMessage(string remoteId, string args) => $"{{'command}}";

    }
}
