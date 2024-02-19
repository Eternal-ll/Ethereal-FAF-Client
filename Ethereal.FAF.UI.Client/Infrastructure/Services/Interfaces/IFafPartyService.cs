using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafPartyService
    {
        /// <summary>
        /// Event when player is kicked out from party
        /// </summary>
        public event EventHandler OnKick;
        /// <summary>
        /// Event of invitation from another player
        /// </summary>
        public event EventHandler<long> OnInvite;
        /// <summary>
        /// Event of update of current party
        /// </summary>
        public event EventHandler<(long Owner, PartyMember[] Members)> OnUpdate;
        /// <summary>
        /// Set party factions
        /// </summary>
        /// <param name="factions">Selected factions</param>
        public void SetPartyFactions(params Faction[] factions);
        /// <summary>
        /// Get allowed factions
        /// </summary>
        /// <returns>Array of allowed <see cref="Faction[]"/></returns>
        public Faction[] GetFactions();
        /// <summary>
        /// Leave from party
        /// </summary>
        public void LeaveParty();
        /// <summary>
        /// Invite player to party
        /// </summary>
        /// <param name="recipientId">Player id</param>
        public void InviteToParty(long recipientId);
        /// <summary>
        /// Accept invite to party
        /// </summary>
        /// <param name="senderId">Player id</param>
        public void AcceptInvite(long senderId);
        /// <summary>
        /// Kick player from party
        /// </summary>
        /// <param name="playerId">Player id</param>
        public void KickFromParty(long playerId);
    }
}
