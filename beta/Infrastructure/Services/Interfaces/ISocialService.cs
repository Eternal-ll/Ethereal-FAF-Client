using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<int> AddedFriend;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<int> AddedFoe;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<int> RemovedFriend;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<int> RemovedFoe;
        public event EventHandler<KeyValuePair<int, PlayerRelationShip>> PlayerdRelationshipChanged;

        public string[] Friends { get; }
        public string[] Foes { get; }

        public SocialData SocialMessage { get; set; }

        public void AddFriend(int id);
        public void AddFoe(int id);
        public void RemoveFriend(int id);
        public void RemoveFoe(int id);

        public List<PlayerInfoMessage> GetFriends();
        public List<PlayerInfoMessage> GetFoes();
    }
}
