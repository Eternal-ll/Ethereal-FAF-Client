using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Background
{
    internal class ImageCachingBackgroundService : BackgroundService
    {
        private readonly IBackgroundQueue _backgroundQueue;
        private readonly ILogger<BackgroundImageCachingQueue> _logger;

        public ImageCachingBackgroundService(
            BackgroundImageCachingQueue backgroundQueue,
            ILogger<BackgroundImageCachingQueue> logger)
        {
            _backgroundQueue = backgroundQueue;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Run(stoppingToken);

        }
        private async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workTask = await _backgroundQueue.DequeueAsync(cancellationToken);
                try
                {
                    await workTask(cancellationToken);
                }
                catch(Exception ex)
                {
                    _logger.LogCritical("Background job failed with exception [{Exception}]", ex.ToString());
                }
            }
        }
    }
}
