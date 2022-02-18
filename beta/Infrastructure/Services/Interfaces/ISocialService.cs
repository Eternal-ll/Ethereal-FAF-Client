using beta.Models.Server;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        public SocialMessage SocialMessage { get; set; }
        public int[] FriendsIds { get; set; }
        public int[] FoesIds { get; set; }

        // fast
        public PlayerInfoMessage GetPlayer(int id, PlayerRelationShip relation = PlayerRelationShip.Friend);
        // slow
        public PlayerInfoMessage GetPlayer(string login, PlayerRelationShip relation = PlayerRelationShip.Friend);

        public void AddPlayer(int id, PlayerRelationShip relation = PlayerRelationShip.Friend);
        public void RemovePlayer(int id, PlayerRelationShip relation = PlayerRelationShip.Friend);
    }
}
