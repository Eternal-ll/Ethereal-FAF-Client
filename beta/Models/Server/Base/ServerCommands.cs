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
        public static string JoinGame(string uid, string password, string gamePort) =>
            $"{{\"command\":\"game_join\", \"uid\": {uid}, \"gameport\":{gamePort}, \"password\": {password}}}";

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
        public static string AddFriend(string id) => $"{{\"command\": \"social_add\", \"friend\": {id}}}";
        /// <summary>
        /// Remove from friends
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string RemoveFriend(string id) => $"{{\"command\": \"social_remove\", \"friend\": {id}}}";
        /// <summary>
        /// Add to foes
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string AddFoe(string id) => $"{{\"command\": \"social_add\", \"friend\": {id}}}";
        /// <summary>
        /// Remove from foes
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string RemoveFoe(string id) => $"{{\"command\": \"social_remove\", \"friend\": {id}}}";

        /// <summary>
        /// Requests MatchMaker info
        /// </summary>
        /// <returns></returns>
        public static string RequestMatchMakerInfo() => "{\"command\": \"matchmaker_info\"}";

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
        public static string RestoreGameSession(string uid) => $"{{'command': 'restore_game_session', 'game_id': {uid}}}";

        /// <summary>
        /// Join to MatchMaking queue
        /// </summary>
        /// <param name="queue">Queue name</param>
        /// <returns></returns>
        public static string JoinToMatchMakingQueue(string queue) => $"{{'command': 'game_matchmaking', 'queue_name': 'queue', 'state': 'start'}}";

        // #"{{'command': 'set_party_factions', 'factions': }}";
        // TODO
        public static string SetPartyFactions() => throw new NotImplementedException();

        /// <summary>
        /// Accept invite to MatchMaker party
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string AcceptPartyInvite(string id) => $"{{'command':'accept_party_invite', 'sender_id': {id}}}";
        /// <summary>
        /// Kick player from MatchMaker party
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public static string KickPlayerFromParty(string id) => $"{{'command':'kick_player_from_party', 'kicked_player_id': {id}}}";
        /// <summary>
        /// Leave from MatchMaker party
        /// </summary>
        /// <returns></returns>
        public static string LeaveFromParty() => "{'command': 'leave_party'}";

    }
}
