using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ISessionService SessionService;
        private readonly ICacheService CacheService;

        private readonly Dictionary<Uri, PlayerAvatar> Cache = new();

        public AvatarService(
            ISessionService sessionService,
            ICacheService cacheService)
        {
            SessionService = sessionService;
            CacheService = cacheService;
        }

        public async Task UpdatePlayerAvatarAsync(PlayerInfoMessage player, PlayerAvatar avatar)
        {
            if (avatar is null)
            {
                player.Avatar = null;
                return;
            }

            if (!Equals(player.Avatar, avatar))
            {
                player.Avatar = avatar;
            }
        }

        public void SetAvatar()
        {
            /*
            "command": "avatar",
            "action": "select",
            "avatar": "https://content.faforever.com/faf/avatars/qai2.png"
            */

            throw new NotImplementedException();
        }

        public void UpdateAvaiableAvatars()
        {
            /*
            "command": "avatar",
            "action": "list_avatar"
            */

            throw new NotImplementedException();
        }

        public BitmapImage[] GetAvailableAvatars()
        {
            /*
            "command": "avatar",
            "avatarlist": [
            { "url": "https://content.faforever.com/faf/avatars/qai2.png","tooltip": "QAI"},
            { "url": "https://content.faforever.com/faf/avatars/UEF.png", "tooltip": "UEF"}
            ]
            */

            throw new NotImplementedException();
        }
    }
}
