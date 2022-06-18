using beta.Models.Server;
using beta.Models.Server.Enums;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public enum QueueStatus : byte
    {
        None,
        Searching,
        Found,
        Waiting,
        Cancelled,
        TimeOut
    }
    public class QueueStatusEventArgs : EventArgs
    {
        public QueueStatusEventArgs(MatchMakerType matchMakerType, QueueStatus status)
        {
            MatchMakerType = matchMakerType;
            Status = status;
        }

        public MatchMakerType MatchMakerType { get; }
        public QueueStatus Status { get; }
    }
    public interface IQueueService
    {
        public event EventHandler<QueueStatusEventArgs> QueueStatusChanged;

        public event EventHandler<MatchFoundData> MatchFound;
        public event EventHandler<MatchCancelledData> MatchCancelled;
        public void SignUpQueue(MatchMakerType type);
        public void SignOutQueue();
    }
}
