using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System;

namespace beta.Infrastructure.Services
{
    internal class QueueService : IQueueService
    {
        public event EventHandler StatusChanged;
        public event EventHandler<QueueStatusEventArgs> QueueStatusChanged;
        public event EventHandler<MatchFoundData> MatchFound;
        public event EventHandler<MatchCancelledData> MatchCancelled;

        private readonly ISessionService SessionService;

        public QueueService(ISessionService sessionService)
        {
            SessionService = sessionService;
        }

        public MatchMakerType CurrentQueue { get; set; }
        public bool IsInQueue { get; set; }

        public void SignOutQueue()
        {
            if (!IsInQueue) return;
        }

        public void SignUpQueue(MatchMakerType type)
        {
            if (IsInQueue) return;
            SessionService.Send(ServerCommands.JoinToMatchMakingQueue(type.ToString()));

        }
    }
}
