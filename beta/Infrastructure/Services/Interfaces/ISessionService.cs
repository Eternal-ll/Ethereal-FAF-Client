using beta.Models.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public enum SessionState : byte
    {
        Disconnected,
        PendingConnection,
        Connected,
        AuthentificationFailed
    }
    /// <summary>
    /// Main service for communicating with lobby-server
    /// </summary>
    public interface ISessionService
    {
        public event EventHandler<SessionState> StateChanged;

        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<GameInfoMessage> GameReceived;
        public event EventHandler<GameInfoMessage[]> GamesReceived;

        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<WelcomeData> WelcomeDataReceived;
        public event EventHandler<NotificationData> NotificationReceived;
        //public event EventHandler<QueueData> QueueDataReceived;
        public event EventHandler<MatchMakerData> MatchMakerDataReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;
        public event EventHandler<IceServersData> IceServersDataReceived;
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;
        public event EventHandler<MatchCancelledData> MatchCancelledDataReceived;
        public event EventHandler<MatchFoundData> MatchFoundDataReceived;

        public bool IsAuthorized { get; }
        public Task AuthorizeAsync(string host, int port, string accessToken, CancellationToken token);
        public void Ping();

        /// <summary>
        /// JSON Format
        /// </summary>
        public void Send(string command);
        public Task SendAsync(string command);
    }
}
