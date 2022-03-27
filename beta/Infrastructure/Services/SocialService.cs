﻿using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
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
        public void AddFriend(int id) => AddRelationShip(id);
        public void AddFoe(int id) => AddRelationShip(id, PlayerRelationShip.Foe);

        public void RemoveFriend(int id) => RemoveRelationShip(id);
        public void RemoveFoe(int id) => RemoveRelationShip(id, PlayerRelationShip.Foe);

        public void AddRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend)
        {
            SendCommand("social_add", id, relation);
        }
        public void RemoveRelationShip(int id, PlayerRelationShip relation = PlayerRelationShip.Friend)
        {
            SendCommand("social_remove", id, relation);
        }

        private void SendCommand(string command, int id, PlayerRelationShip relation)
        {
            /*
            
            "command": "social_add",
            "friend": 2

            "command": "social_remove",
            "friend": 1

            "command": "social_add",
            "foe": 2

            "command": "social_remove",
            "foe": 1

            */

            string json = $"{{ \"command\":\"{command}\", \"{relation.ToString().ToLower()}\": {id} }}";

            var player = PlayersService.GetPlayer(id);
            if (player is not null)
            {
                if (command == "social_remove")
                    relation = PlayerRelationShip.None;
                
                player.RelationShip = relation;
            }

            SessionService.Send(json);
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
