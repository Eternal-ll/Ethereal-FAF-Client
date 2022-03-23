using beta.Models;
using beta.Models.IRC;
using beta.Models.IRC.Enums;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IIrcService
    {
        public void Test();

        #region Events
        public event EventHandler<EventArgs<ManagedTcpClientState>> StateChanged;
        public event EventHandler<bool> IrcConnected;

        /// <summary>
        /// User connected to IRC server
        /// </summary>
        public event EventHandler<EventArgs<string>> UserConnected;
        /// <summary>
        /// User disconnected from IRC server
        /// </summary>
        public event EventHandler<EventArgs<string>> UserDisconnected;

        /// <summary>
        /// User joined to specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcUserJoin>> UserJoined;
        /// <summary>
        /// User left from specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcUserLeft>> UserLeft;

        /// <summary>
        /// Private message received from specific user
        /// </summary>
        public event EventHandler<EventArgs<IrcPrivateMessage>> PrivateMessageReceived;
        /// <summary>
        /// Channel message received from specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelMessage>> ChannedMessageReceived;

        /// <summary>
        /// Specific channel topic updated
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelTopicUpdated>> ChannelTopicUpdated;

        /// <summary>
        /// Specific channel topic changed by specific user
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelTopicChangedBy>> ChannelTopicChangedBy;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelUsers>> ChannelUsersReceived;

        #endregion

        #region Properties
        public bool IsIRCConnected { get; }
        public ManagedTcpClientState TCPConnectionState { get; }
        #endregion

        #region Methods
        public void Connect(string host, int port);

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
