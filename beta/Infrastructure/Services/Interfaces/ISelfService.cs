using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Self service instance
    /// </summary>
    public interface ISelfService
    {
        public event EventHandler<PlayerInfoMessage> SelfUpdated;
        public PlayerInfoMessage Self { get; }
    }
}
