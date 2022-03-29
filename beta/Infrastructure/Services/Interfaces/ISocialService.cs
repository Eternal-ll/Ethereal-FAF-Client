using beta.Models.Server;
using beta.Models.Server.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        public SocialMessage SocialMessage { get; set; }

        public void AddFriend(int id);
        public void AddFoe(int id);
        public void RemoveFriend(int id);
        public void RemoveFoe(int id);

        public List<PlayerInfoMessage> GetFriends();
        public List<PlayerInfoMessage> GetFoes();
    }
}
