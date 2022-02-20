using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ISessionService SessionService;
        private readonly ICacheService CacheService;
        private readonly List<BitmapImage> Cache = new();

        public AvatarService(
            ISessionService sessionService,
            ICacheService cacheService)
        {
            SessionService = sessionService;
            CacheService = cacheService;
        }

        public BitmapImage GetAvatar(Uri uri) => CacheService.GetImage(uri, Folder.PlayerAvatars);

        public void SetAvatar()
        {
            throw new NotImplementedException();
        }
    }
}
