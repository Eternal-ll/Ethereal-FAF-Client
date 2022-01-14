using beta.Models.Server;
using beta.Views;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ILobbySessionService
    {
        public event EventHandler<EventArgs<MainView.PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<MainView.PlayerInfoMessage>> UpdatePlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGameInfo;
        public event EventHandler<EventArgs<GameInfoMessage>> UpdateGameInfo;
        public event EventHandler<MainView.IServerMessage> MessageReceived;
        public string Session { get; }
        public void Connect(IPEndPoint ip);
        public void AskSession();
        public Task<string> GenerateUID(string session);
        public void Authorize();
        public event EventHandler<EventArgs<bool>> Authorization;
        public bool AuthorizationRequested { get; set; }
        public Dictionary<int, MainView.PlayerInfoMessage> Players { get; }
        public List<GameInfoMessage> Games { get; }
    }
}
