using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISessionService
    {
        public event EventHandler<EventArgs<bool>> Authorized;
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayerReceived;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGameReceived;
        public event EventHandler<EventArgs<SocialMessage>> SocialDataReceived;
        public event EventHandler<EventArgs<WelcomeMessage>> WelcomeDataReceived;
        public event EventHandler<EventArgs<NoticeMessage>> NotificationReceived;
        //public event EventHandler<EventArgs<QueueData>> QueueDataReceived;
        public event EventHandler<EventArgs<MatchMakerData>> MatchMakerDataReceived;
        public event EventHandler<EventArgs<GameLaunchData>> GameLaunchDataReceived;

        public void Connect();
        public void Authorize();
        public void Ping();
        public string GenerateUID(string session);

        /// <summary>
        /// JSON Format
        /// </summary>
        public void Send(string command);
    }
}
