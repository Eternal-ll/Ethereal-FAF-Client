using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISelfService
    {
        public event EventHandler<PlayerInfoMessage> SelfUpdated;
        public PlayerInfoMessage Self { get; }
    }
}
