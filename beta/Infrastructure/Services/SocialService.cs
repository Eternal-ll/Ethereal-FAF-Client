using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System;
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

        public event EventHandler<int> AddedFriend;
        public event EventHandler<int> AddedFoe;
        public event EventHandler<int> RemovedFriend;
        public event EventHandler<int> RemovedFoe;
        public event EventHandler<KeyValuePair<int, PlayerRelationShip>> PlayerdRelationshipChanged;

        private readonly ISessionService SessionService;

        public SocialData SocialMessage { get; set; }

        public List<int> Friends { get; private set; }

        public List<int> Foes { get; private set; }

        public SocialService(ISessionService sessionService)
        {
            SessionService = sessionService;

            sessionService.SocialDataReceived += SessionService_SocialDataReceived;
        }

        private void SessionService_SocialDataReceived(object sender, SocialData e)
        {
            Friends = new(e.friends);
            Foes = new(e.foes);
        }

        #region Methods
        public void AddFriend(int id) => SendCommand(ServerCommands.AddFriend(id), id, PlayerRelationShip.Friend);
        public void AddFoe(int id) => SendCommand(ServerCommands.AddFoe(id), id, PlayerRelationShip.Foe);
        public void RemoveFriend(int id) => SendCommand(ServerCommands.RemoveFriend(id), id, PlayerRelationShip.Friend, true);
        public void RemoveFoe(int id) => SendCommand(ServerCommands.RemoveFoe(id), id, PlayerRelationShip.Foe, true);

        private void SendCommand(string command, int id, PlayerRelationShip relation, bool isRemoving = false)
        {
            SessionService.Send(command);
            switch (relation)
            {
                case PlayerRelationShip.Friend:
                    if (isRemoving)
                    {
                        relation = PlayerRelationShip.None;
                        Friends.Remove(id);
                        RemovedFriend?.Invoke(this, id);
                    }
                    else
                    {
                        Friends.Add(id);
                        AddedFriend?.Invoke(this, id);
                    }
                    break;
                case PlayerRelationShip.Foe:
                    if (isRemoving)
                    {
                        relation = PlayerRelationShip.None;
                        Foes.Remove(id);
                        RemovedFoe?.Invoke(this, id);
                    }
                    else
                    {
                        Foes.Add(id);
                        AddedFoe?.Invoke(this, id);
                    }
                    break;
            }
            PlayerdRelationshipChanged?.Invoke(this, new(id, relation));
        }

        public List<PlayerInfoMessage> GetFriends()
        {
            throw new NotImplementedException();
        }

        public List<PlayerInfoMessage> GetFoes()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
