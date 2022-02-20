using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;

namespace beta.Infrastructure.Services
{
    public class SocialService : ISocialService
    {
        /*
        ## Social
        The social components of the lobby server are relatively limited, as the
        primary social element, chat, is handled by a separate server.The social
        features handled by the lobby server are therefore limited to:

        - Syncronizing online player state
        - Enforcing global bans
        - Modifying a list of friends and a list of foes
        - Modifying the currently selected avatar
        */

        #region Properties

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;

        public SocialMessage SocialMessage { get; set; }

        #endregion

        #region Ctor
        public SocialService(
            ISessionService sessionService,
            IPlayersService playersService)
        {
            SessionService = sessionService;
            PlayersService = playersService;
        }
        #endregion

        #region Methods
        public void AddFriend(int id) => AddRelationShip(id);
        public void AddFoe(int id) => AddRelationShip(id, PlayerRelationShip.Foe);

        public void RemoveFriend(int id) => RemoveRelationShip(id);
        public void RemoveFoe(int id) => RemoveRelationShip(id, PlayerRelationShip.Foe);

        public void AddRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend) => SendCommand("social_add", id, relation);
        public void RemoveRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend) => SendCommand("social_remove", id, relation);

        private void SendCommand(string command, int id, PlayerRelationShip relation)
        {
            string json = $"{{ \"command\":\"{command}\", \"{relation.ToString().ToLower()}\": {id} }}";

            var player = PlayersService.GetPlayer(id);
            if (player != null)
            {
                if (command == "social_remove")
                    relation = PlayerRelationShip.None;
                
                player.RelationShip = relation;
            }

            SessionService.Send(json);
        }
        #endregion
    }
}
