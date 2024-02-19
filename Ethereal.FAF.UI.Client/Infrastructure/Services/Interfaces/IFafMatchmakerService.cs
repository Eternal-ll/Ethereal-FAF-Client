using FAF.Domain.LobbyServer;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafMatchmakerService
    {
        public event EventHandler<QueueData[]> OnQueues;
        public event EventHandler<SearchInfo> OnSearch;
        public event EventHandler<MatchFound> OnMatchFound;
        public event EventHandler<MatchCancelled> OnMatchCancelled;
        public event EventHandler<MatchConfirmation> OnMatchConfirmation;
        public void UpdateQueueState(string queueName, QueueSearchState state);
        public void RefreshQueues();
        public void MatchReady();
    }
}
