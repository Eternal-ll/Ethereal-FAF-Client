using beta.Models.Server;
using beta.Models.Server.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        public SocialMessage SocialMessage { get; set; }

        public void AddRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend);
        public void RemoveRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend);

        public List<PlayerInfoMessage> GetFriends();
        public List<PlayerInfoMessage> GetFoes();
    }
}
