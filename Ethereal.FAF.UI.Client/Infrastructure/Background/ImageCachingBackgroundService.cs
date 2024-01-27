using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Background
{
    internal class ImageCachingBackgroundService : BackgroundQueueService
    {
        public ImageCachingBackgroundService(
            BackgroundImageCachingQueue queue,
            ILogger<BackgroundImageCachingQueue> logger) : base(queue, logger) { }
    }
}
