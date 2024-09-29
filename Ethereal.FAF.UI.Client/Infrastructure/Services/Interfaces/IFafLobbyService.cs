using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafLobbyService
    {
        public bool Connected { get; }
        /// <summary>
        /// Connect to lobby
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Disconnect from lobby
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DisconnectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="uid">Lobby UID</param>
        /// <returns></returns>
        public Task JoinGameAsync(long uid, string password = null, int port = 0);
        /// <summary>
        /// Restore game session
        /// </summary>
        /// <param name="uid">Game id</param>
        /// <returns></returns>
        public Task RestoreGameSessionAsync(long uid);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task GameEndedAsync();

        #region Party

        /// <summary>
        /// Accept party invite
        /// </summary>
        /// <param name="senderId">Player id</param>
        public void AcceptPartyInvite(long senderId);
        /// <summary>
        /// Invite player to party
        /// </summary>
        /// <param name="recipientId">Player id</param>
        public void InviteToParty(long recipientId);
        /// <summary>
        /// Kick player from party
        /// </summary>
        /// <param name="playerId">Player id</param>
        public void KickFromParty(long playerId);
        /// <summary>
        /// Leave from current party
        /// </summary>
        public void LeaveParty();
        /// <summary>
        /// Set party factions
        /// </summary>
        /// <remarks>
        /// Auto creates party if player is not a member of any party
        /// </remarks>
        /// <param name="factions">Selected factions</param>
        public void SetPartyFactions(params Faction[] factions);
        #endregion

        #region Matchmaking
        public void AskQueues();
        public void MatchReady();
        public void UpdateQueueState(string queue, QueueSearchState state);

        #endregion
    }
}
