using beta.Models.Server.Enums;

namespace beta.Models
{
    public interface IPlayer
    {
        public string login { get; set; }
        public bool IsChatModerator { get; set; }
        public PlayerRelationShip RelationShip { get; set; }
    }
}
