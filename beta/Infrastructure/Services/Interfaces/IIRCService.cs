using beta.Models.Enums;
using beta.Models.IRC;
using beta.Models.IRC.Enums;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IIrcService
    {
        public void Test();

        public IrcState State { get; set; }

        #region Events
        public event EventHandler<IrcState> StateChanged;

        /// <summary>
        /// User connected to IRC server
        /// </summary>
        public event EventHandler<string> UserConnected;
        /// <summary>
        /// User disconnected from IRC server
        /// </summary>
        public event EventHandler<string> UserDisconnected;

        /// <summary>
        /// User joined to specific channel
        /// </summary>
        public event EventHandler<IrcUserJoin> UserJoined;
        /// <summary>
        /// User left from specific channel
        /// </summary>
        public event EventHandler<IrcUserLeft> UserLeft;

        /// <summary>
        /// User changed his nickname
        /// </summary>
        public event EventHandler<IrcUserChangedName> UserChangedName;

        /// <summary>
        /// Private message received from specific user
        /// </summary>
        public event EventHandler<IrcPrivateMessage> PrivateMessageReceived;
        /// <summary>
        /// Channel message received from specific channel
        /// </summary>
        public event EventHandler<IrcChannelMessage> ChannedMessageReceived;

        /// <summary>
        /// Specific channel topic updated
        /// </summary>
        public event EventHandler<IrcChannelTopicUpdated> ChannelTopicUpdated;

        /// <summary>
        /// Specific channel topic changed by specific user
        /// </summary>
        public event EventHandler<IrcChannelTopicChangedBy> ChannelTopicChangedBy;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<IrcChannelUsers> ChannelUsersReceived;

        #endregion

        #region Methods
        public void Connect(string host, int port);
        public void Restart(string nickname, string password);

        #region IRC
        public void SendCommand(IrcUserCommand command, string writtenText, string channel = null);
        public void Authorize(string nickname, string password);

        public void Join(string channel, string key = null);
        public void SetTopic(string channel, string topic = null);

        public void SendMessage(string channelOrUser, string message);
        public void SendInvite(string user, string channel);

        public void Ping();

        public void Leave(string channel);
        public void Leave(string[] channels);
        public void LeaveAll();

        public void Quit(string reason = null);
        #endregion

        #endregion
    }
}
