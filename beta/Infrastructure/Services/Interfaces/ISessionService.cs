using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISessionService
    {
        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> NewPlayerReceived;
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<WelcomeData> WelcomeDataReceived;
        public event EventHandler<NotificationData> NotificationReceived;
        //public event EventHandler<QueueData> QueueDataReceived;
        public event EventHandler<MatchMakerData> MatchMakerDataReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;
        public event EventHandler<IceServersData> IceServersDataReceived;
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;

        public bool IsAuthorized { get; }

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
