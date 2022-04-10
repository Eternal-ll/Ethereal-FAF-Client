using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public enum SessionState : byte
    {
        Disconnected,
        PendingConnection,
        Connected,
    }
    public interface ISessionService
    {
        public event EventHandler<SessionState> StateChanged;

        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<GameInfoMessage> GameReceived;
        public event EventHandler<GameInfoMessage[]> GamesReceived;

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

        /// <summary>
        /// JSON Format
        /// </summary>
        public void Send(string command);
    }
}
