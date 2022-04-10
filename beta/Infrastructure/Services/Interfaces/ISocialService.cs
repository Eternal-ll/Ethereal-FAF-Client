using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISocialService
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string> AddedFriend;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string> AddedFoe;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string> RemovedFriend;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string> RemovedFoe;

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
