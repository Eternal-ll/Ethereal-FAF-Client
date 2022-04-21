using beta.Models.Server.Enums;

namespace beta.Models
{
    public class UnknownPlayer : IPlayer
    {
        public string login { get; set; }
        public bool IsChatModerator { get; set; }
        public PlayerRelationShip RelationShip { get; set; } = PlayerRelationShip.IRC;
        public int id { get; set; }
        public string clan { get; set; }
    }
}
