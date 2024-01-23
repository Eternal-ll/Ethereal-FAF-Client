using System;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class BackgroundImageCachingQueue : BackgroundQueue,
        IBackgroundImageCacheService
    {
        private readonly IImageCacheService _imageCacheService;

        public BackgroundImageCachingQueue(IImageCacheService imageCacheService)
        {
            _imageCacheService = imageCacheService;
        }

        public void Load(string url, Action<string> success) => Enqueue(async cancel =>
        {
            var cacheFile = await _imageCacheService.EnsureCachedAsync(url, cancel);
            success(cacheFile);
        });
    }
}
