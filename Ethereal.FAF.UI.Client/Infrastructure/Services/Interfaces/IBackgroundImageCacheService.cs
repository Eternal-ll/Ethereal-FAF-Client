using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IBackgroundImageCacheService
    {
        public void Load(string url, Action<string> success);
    }
}
