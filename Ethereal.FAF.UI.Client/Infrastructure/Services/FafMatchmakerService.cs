using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    [Singleton(typeof(IFafMatchmakerService), typeof(FafMatchmakerService))]
    public class FafMatchmakerService : IFafMatchmakerService
    {
        private readonly IFafLobbyService _lobbyService;
        private readonly IFafLobbyEventsService _lobbyEventsService;

        public event EventHandler<QueueData[]> OnQueues;
        public event EventHandler<SearchInfo> OnSearch;
        public event EventHandler<MatchFound> OnMatchFound;
        public event EventHandler<MatchCancelled> OnMatchCancelled;
        public event EventHandler<MatchConfirmation> OnMatchConfirmation;

        public FafMatchmakerService(
            IFafLobbyService lobbyService,
            IFafLobbyEventsService lobbyEventsService)
        {
            lobbyEventsService.SearchInfoReceived += LobbyEventsService_SearchInfoReceived;
            lobbyEventsService.MatchCancelled += LobbyEventsService_MatchCancelled;
            lobbyEventsService.MatchConfirmation += LobbyEventsService_MatchConfirmation;
            lobbyEventsService.MatchMakingDataReceived += LobbyEventsService_MatchMakingDataReceived;

            _lobbyService = lobbyService;
            _lobbyEventsService = lobbyEventsService;
        }

        private void LobbyEventsService_MatchMakingDataReceived(object sender, MatchmakingData e)
            => OnQueues?.Invoke(this, e.Queues);

        private void LobbyEventsService_MatchConfirmation(object sender, MatchConfirmation e)
            => OnMatchConfirmation?.Invoke(this, e);

        private void LobbyEventsService_MatchCancelled(object sender, MatchCancelled e)
            => OnMatchCancelled?.Invoke(this, e);

        private void LobbyEventsService_SearchInfoReceived(object sender, SearchInfo e)
            => OnSearch?.Invoke(this, e);

        public void MatchReady() => _lobbyService.MatchReady();
        public void RefreshQueues() => _lobbyService.AskQueues();
        public void UpdateQueueState(string queueName, QueueSearchState state)
            => _lobbyService.UpdateQueueState(queueName, state);
    }
}
