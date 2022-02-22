using beta.Models;
using beta.Models.Server;
using System;
using System.Net;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ISessionService
    {
        public event EventHandler<EventArgs<bool>> Authorized;

        public event EventHandler<EventArgs<SocialMessage>> SocialInfo;
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGame;

        public ManagedTcpClient TcpClient { get; }

        public void Connect(IPEndPoint ip);
        public void AskSession();
        public void Authorize();
        public string GenerateUID(string session);

        /// <summary>
        /// JSON Format
        /// </summary>
        public void Send(string command);
    }
}
