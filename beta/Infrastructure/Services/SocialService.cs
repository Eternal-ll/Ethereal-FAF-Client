using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System.Collections.Generic;

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
        public void AddFriend(int id) => SendCommand(ServerCommands.AddFriend(id.ToString()), id, PlayerRelationShip.Friend);
        public void AddFoe(int id) => SendCommand(ServerCommands.AddFoe(id.ToString()), id, PlayerRelationShip.Foe);
        public void RemoveFriend(int id) => SendCommand(ServerCommands.RemoveFriend(id.ToString()), id, PlayerRelationShip.Friend, true);
        public void RemoveFoe(int id) => SendCommand(ServerCommands.RemoveFoe(id.ToString()), id, PlayerRelationShip.Foe, true);

        private void SendCommand(string command, int id, PlayerRelationShip relation, bool isRemoving = false)
        {
            var player = PlayersService.GetPlayer(id);
            if (player is not null)
            {
                if (isRemoving)
                    relation = PlayerRelationShip.None;
                
                player.RelationShip = relation;
            }

            SessionService.Send(command);
        }

        public List<PlayerInfoMessage> GetFriends()
        {
            var friendsIds = SocialMessage.friends;
            List<PlayerInfoMessage> friends = new();
            for (int i = 0; i < friendsIds.Count; i++)
            {
                if (PlayersService.TryGetPlayer(friendsIds[i], out var player))
                {
                    friends.Add(player);
                }
            }
            return friends;
        }

        public List<PlayerInfoMessage> GetFoes()
        {
            var foesIds = SocialMessage.friends;
            List<PlayerInfoMessage> foes = new();
            for (int i = 0; i < foesIds.Count; i++)
            {
                if (PlayersService.TryGetPlayer(foesIds[i], out var player))
                {
                    foes.Add(player);
                }
            }
            return foes;
        }
        #endregion
    }
}
