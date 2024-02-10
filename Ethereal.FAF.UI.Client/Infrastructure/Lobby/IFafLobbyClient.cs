using StreamJsonRpc;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// Client for FAForever server <see href="https://github.com/FAForever/server"/>
    /// </summary>
    public interface IFafLobbyClient
    {
        #region Authorization
        /// <summary>
        /// Ask valid session for authorization (can be delayed)
        /// </summary>
        /// <param name="user_agent">Client name</param>
        /// <param name="version">Version of client</param>
        /// <returns><see cref="IFafLobbyCallbacks.OnSessionAsync(long)"/></returns>
        [JsonRpcMethod("ask_session")]
        public void AskSession(string user_agent, string version);
        /// <summary>
        /// Authorize in lobby
        /// </summary>
        /// <param name="token">OAuth2 token</param>
        /// <param name="unique_id">FA Forever unique id <see href="https://github.com/FAForever/uid"/></param>
        /// <param name="session">Valid session fetched from <see cref="AskSessionAsync(string, string)"/></param>
        /// <returns></returns>
        [JsonRpcMethod("auth")]
        public void Auth(string token, string unique_id, long session);
        #endregion

        #region Social
        /// <summary>
        /// Add friend
        /// </summary>
        /// <param name="friend">Played id</param>
        /// <returns></returns>
        [JsonRpcMethod("social_add")]
        public void AddFriend(int friend);
        /// <summary>
        /// Remove friend
        /// </summary>
        /// <param name="friend">Played id</param>
        /// <returns></returns>
        [JsonRpcMethod("social_remove")]
        public void RemoveFriend(int friend);
        /// <summary>
        /// Add foe
        /// </summary>
        /// <param name="foe">Played id</param>
        /// <returns></returns>
        [JsonRpcMethod("social_add")]
        public void AddFoe(int foe);
        /// <summary>
        /// Remove foe
        /// </summary>
        /// <param name="foe">Played id</param>
        /// <returns></returns>
        [JsonRpcMethod("social_remove")]
        public void RemoveFoe(string foe);
        #endregion

        #region Avatar
        /// <summary>
        /// Action on avatar
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="avatar">Avatar url (only for <paramref name="action"/> == 'select')</param>
        /// <returns></returns>
        [JsonRpcMethod("avatar")]
        public void AvatarAction(string action, string avatar = null);
        /// <summary>
        /// Send a list of available avatars
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("avatar")]
        public void ListAvatar(string action = "list_avatar");
        /// <summary>
        /// Select a valid avatar for the playe
        /// </summary>
        /// <param name="avatar">Avatar url</param>
        /// <returns></returns>
        [JsonRpcMethod("avatar")]
        public void SelectAvatar(string avatar, string action = "select");
        #endregion

        #region Parties
        /// <summary>
        /// Invite this player to a party
        /// </summary>
        /// <param name="recipient_id">Player id</param>
        /// <returns></returns>
        [JsonRpcMethod("invite_to_party")]
        public void InviteToParty(int recipient_id);
        /// <summary>
        /// Accept the party invite from the given player
        /// </summary>
        /// <param name="sender_id">Player id</param>
        /// <returns></returns>
        [JsonRpcMethod("accept_party_invite")]
        public void AcceptPartyInvite(int sender_id);
        /// <summary>
        /// Kick a player from a party you own
        /// </summary>
        /// <param name="kicked_player_id">Player id</param>
        /// <returns></returns>
        [JsonRpcMethod("kick_player_from_party")]
        public void KickPlayerFromParty(int kicked_player_id);
        /// <summary>
        /// Leave the party you are currently in
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("leave_party")]
        public void LeaveParty();
        /// <summary>
        /// Set party factions
        /// </summary>
        /// <param name="factions">Selected factions</param>
        /// <returns></returns>
        [JsonRpcMethod("set_party_factions")]
        public void SetPartyFactions(params string[] factions);
        #endregion

        #region Matchmaking
        /// <summary>
        /// Ask matchmaker info
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("matchmaker_info")]
        public void AskMatchmakerInfo();
        /// <summary>
        /// Update queue state
        /// </summary>
        /// <param name="queue_name">Queue name</param>
        /// <param name="state">Queue state</param>
        /// <returns></returns>
        [JsonRpcMethod("game_matchmaking")]
        public void UpdateMatchmakingQueueState(string queue_name, string state, params string[] factions);
        /// <summary>
        /// Join matchmaking queue
        /// </summary>
        /// <param name="queue_name">Queue name</param>
        /// <param name="factions">Selected factions</param>
        /// <returns></returns>
        public void StartMatchmakingQueue(string queue_name, params string[] factions)
            => UpdateMatchmakingQueueState(queue_name, "start", factions);
        /// <summary>
        /// Leave matchmaking queue
        /// </summary>
        /// <param name="queue_name">Queue name</param>
        /// <param name="factions">Selected factions</param>
        /// <returns></returns>
        public void StopMatchmakingQueue(string queue_name)
            => UpdateMatchmakingQueueState(queue_name, "stop");
        /// <summary>
        /// Ready for matchmaking match
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("match_ready")]
        public void MatchReady();
        #endregion

        #region Game
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="uid">Game id</param>
        /// <param name="password">Password to join</param>
        /// <param name="gamePort"></param>
        /// <returns></returns>
        [JsonRpcMethod("game_join")]
        public void GameJoin(long uid, string password, int gamePort = 0);
        /// <summary>
        /// Host game
        /// </summary>
        /// <param name="title"></param>
        /// <param name="mod">Game mod like faf/fafdevelop/fafbeta</param>
        /// <param name="visibility">Public or Friends</param>
        /// <param name="mapname">Map name</param>
        /// <param name="password">Secure game with password</param>
        /// <param name="is_rehost">Is game rehosting</param>
        /// <returns></returns>
        [JsonRpcMethod("game_host")]
        public void GameHost(string title, string mod, string mapname, string visibility = "public", string password = null, bool is_rehost = false);
        /// <summary>
        /// Restore game session
        /// </summary>
        /// <param name="game_id">Game id</param>
        /// <returns></returns>
        [JsonRpcMethod("restore_game_session")]
        public void RestoreGameSession(long game_id);
        #endregion


        [JsonRpcMethod("GameState")]
        public void GameState(string target, params object[] args);
        [JsonRpcMethod("IceMsg")]
        public void IceMsg(string target, params object[] args);

        public void GameEnded() => GameState("game", "Ended");

        /// <summary>
        /// Ping
        /// </summary>
        /// <returns><see cref="IFafLobbyCallbacks.OnPongAsync"/></returns>
        [JsonRpcMethod("ping")]
        public void Ping();
        /// <summary>
        /// Pong on <see cref="IFafLobbyCallbacks.OnPingAsync"/>
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("pong")]
        public void Pong();
    }
}
