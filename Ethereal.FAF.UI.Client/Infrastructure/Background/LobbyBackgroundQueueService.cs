using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Background
{
    /// <summary>
    /// Background queue service for handling incoming messages from FAF lobby
    /// </summary>
    internal class LobbyBackgroundQueueService : BackgroundQueueService
    {
        public LobbyBackgroundQueueService(
            LobbyBackgroundQueue queue,
            ILogger<LobbyBackgroundQueueService> logger) : base(queue, logger) { }
    }
}
