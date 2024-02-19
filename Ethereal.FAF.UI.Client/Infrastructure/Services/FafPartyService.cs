using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    [Singleton(typeof(IFafPartyService), typeof(FafPartyService))]
    internal class FafPartyService : IFafPartyService
    {
        private readonly IFafLobbyEventsService _eventsService;
        private readonly IFafLobbyService _lobbyService;
        private readonly static Faction[] _factions = new Faction[]
        {
            Faction.UEF, Faction.AEON, Faction.CYBRAN, Faction.SERAPHIM
        };

        public event EventHandler OnKick;
        public event EventHandler<long> OnInvite;
        public event EventHandler<(long Owner, PartyMember[] Members)> OnUpdate;

        public FafPartyService(IFafLobbyEventsService eventsService, IFafLobbyService lobbyService)
        {
            eventsService.KickedFromParty += OnKickFromParty;
            eventsService.PartyInvite += OnPartyInvite;
            eventsService.PartyUpdated += OnPartyUpdated;
            _eventsService = eventsService;
            _lobbyService = lobbyService;
        }

        private void OnPartyUpdated(object sender, PartyUpdate e)
            => OnUpdate?.Invoke(this, (e.OwnerId, e.Members));

        private void OnPartyInvite(object sender, PartyInvite e)
            => OnInvite?.Invoke(this, e.SenderId);

        private void OnKickFromParty(object sender, EventArgs e)
            => OnKick?.Invoke(this, e);
        public void AcceptInvite(long senderId) => _lobbyService.AcceptPartyInvite(senderId);
        public void InviteToParty(long recipientId) => _lobbyService.InviteToParty(recipientId);
        public void LeaveParty() => _lobbyService.LeaveParty();
        public void SetPartyFactions(params Faction[] factions)
        {
            foreach (var faction in factions)
            {
                if (!_factions.Contains(faction))
                    throw new ArgumentOutOfRangeException(
                        paramName: nameof(factions),
                        actualValue: faction.ToString(),
                        message: "Passed Faction not allowed to use in this method");
            }
            _lobbyService.SetPartyFactions(factions);
        }
        public void KickFromParty(long playerId) => _lobbyService.KickFromParty(playerId);
        public Faction[] GetFactions() => _factions;
    }
}
