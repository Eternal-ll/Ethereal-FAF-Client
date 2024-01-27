using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Background
{
    internal abstract class BackgroundQueueService : BackgroundService
    {
        private readonly IBackgroundQueue _queue;
        private readonly ILogger _logger;

        public BackgroundQueueService(IBackgroundQueue queue, ILogger logger)
        {
            _queue = queue;
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Run(stoppingToken);
        private async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workTask = await _queue.DequeueAsync(cancellationToken);
                try
                {
                    await workTask(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Background job failed with exception [{Exception}]", ex.ToString());
                }
            }
        }
    }
}
