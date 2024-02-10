using FAF.Domain.LobbyServer;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{

    public interface IFafLobbyCallbacks
    {
        [JsonRpcMethod("session")]
        public Task OnSessionAsync(long session);
        [JsonRpcMethod("notice")]
        public Task OnNoticeAsync(string style, string text);
        [JsonRpcMethod("welcome")]
        public Task OnWelcomeAsync(PlayerInfoMessage me, int id, string login);
        [JsonRpcMethod("social")]
        public Task OnSocialAsync(string[] autojoin, string[] channels, int[] friends, int[] foes, int power);
        //[JsonRpcMethod("matchmaker_info")]
        //public Task OnMatchmakerInfoAsync(QueueData[] queues);
        [JsonRpcMethod("ping")]
        public Task OnPingAsync();
        [JsonRpcMethod("pong")]
        public Task OnPongAsync();
        //[JsonRpcMethod("game_launch", UseSingleObjectParameterDeserialization = true)]
        //public Task OnGameLaunchAsync(GameLaunchData model);

        [JsonRpcMethod("player_info", UseSingleObjectParameterDeserialization = true)]
        public Task OnPlayerInfoAsync(PlayerInfoMessage player);
        [JsonRpcMethod("players_info")]
        public Task OnPlayersInfoAsync(PlayerInfoMessage[] players);

        [JsonRpcMethod("game_info", UseSingleObjectParameterDeserialization = true)]
        public Task OnGameInfoAsync(GameInfoMessage game);
        [JsonRpcMethod("games_info")]
        public Task OnGamesInfoAsync(GameInfoMessage[] games);

        #region Parties
        /// <summary>
        /// Party invite
        /// </summary>
        /// <param name="sender">Player id</param>
        /// <returns></returns>
        [JsonRpcMethod("party_invite")]
        public Task OnPartyInviteAsync(long sender);
        /// <summary>
        /// Party update
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        [JsonRpcMethod("party_update")]
        public Task OnPartyUpdateAsync(long owner, PartyMember[] members);
        [JsonRpcMethod("kicked_from_party")]
        public Task OnKickedFromPartyAsync();
        #endregion

        #region Matchmaking
        /// <summary>
        /// Match found
        /// </summary>
        /// <param name="game_id"></param>
        /// <param name="queue_name"></param>
        /// <returns></returns>
        [JsonRpcMethod("match_found")]
        public Task OnMatchFoundAsync(long game_id, string queue_name);
        /// <summary>
        /// Match cancelled
        /// </summary>
        /// <param name="game_id"></param>
        /// <param name="queue_name"></param>
        /// <returns></returns>
        [JsonRpcMethod("match_cancelled")]
        public Task OnMatchCancelledAsync(long game_id, string queue_name);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expires_at"></param>
        /// <param name="players_total"></param>
        /// <param name="players_ready"></param>
        /// <param name="ready"></param>
        /// <remarks>
        /// <see cref="https://github.com/FAForever/server/issues/607"/>
        /// <seealso cref="https://github.com/FAForever/downlords-faf-client/issues/1783"/>
        /// </remarks>
        /// <returns></returns>
        [JsonRpcMethod("match_info")]
        public Task OnMatchInfoAsync(DateTime expires_at, int players_total, int players_ready, bool ready);
        /// <summary>
        /// Queue state
        /// </summary>
        /// <param name="queue_name">Queue name</param>
        /// <param name="state">Queue state</param>
        /// <returns></returns>
        [JsonRpcMethod("search_info")]
        public Task OnSearchInfoAsync(string queue_name, string state);
        #endregion

        /// <summary>
        /// We did something wrong and the lobby will disconnect client
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("invalid")]
        public Task OnInvalidAsync();

        #region Game
        [JsonRpcMethod("JoinGame")]
        public Task OnJoinGameAsync(string target, object[] args);
        [JsonRpcMethod("HostGame")]
        public Task OnHostGameAsync(string target, object[] args);
        [JsonRpcMethod("ConnectToPeer")]
        public Task OnConnectToPeerAsync(string target, object[] args);
        [JsonRpcMethod("DisconnectFromPeer")]
        public Task OnDisconnectFromPeerAsync(string target, object[] args);
        [JsonRpcMethod("IceMsg")]
        public Task OnIceMsgAsync(string target, object[] args);
        #endregion
    }
}
