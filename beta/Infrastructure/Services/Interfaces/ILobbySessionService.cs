using beta.Models.Server;
using beta.Models.Server.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ILobbySessionService : INotifyPropertyChanged
    {
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<PlayerInfoMessage>> UpdatePlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGameInfo;
        public event EventHandler<EventArgs<GameInfoMessage>> UpdateGameInfo;
        public event EventHandler<IServerMessage> MessageReceived;
        public string Session { get; }
        public void Connect(IPEndPoint ip);
        public void AskSession();
        public Task<string> GenerateUID(string session);
        public void Authorize();
        public event EventHandler<EventArgs<bool>> Authorization;
        public bool AuthorizationRequested { get; set; }

        public ObservableCollection<PlayerInfoMessage> Players { get; }
        public PlayerInfoMessage GetPlayerInfo(int uid);
        public PlayerInfoMessage GetPlayerInfo(string login);

        public IEnumerable<string> GetPlayersLogins(string filter);
        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter);

        public ObservableCollection<GameInfoMessage> AvailableLobbies { get; }
    }
}
