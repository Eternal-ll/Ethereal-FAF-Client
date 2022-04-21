using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IPlayersService
    {
        public event EventHandler<PlayerInfoMessage> SelfReceived;

        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage> PlayerUpdated;
        public event EventHandler<PlayerInfoMessage> PlayerLeft;

        public event EventHandler<PlayerInfoMessage> FriendConnected;
        public event EventHandler<PlayerInfoMessage> FoeConnected;
        public event EventHandler<PlayerInfoMessage> ClanmateConnected;

        public event EventHandler<PlayerInfoMessage> FriendJoinedToGame;
        public event EventHandler<PlayerInfoMessage> FriendLeftFromGame;
        public event EventHandler<PlayerInfoMessage> FriendFinishedGame;

        public event EventHandler<PlayerInfoMessage> FoeJoinedToGame;
        public event EventHandler<PlayerInfoMessage> FoeLeftFromGame;
        public event EventHandler<PlayerInfoMessage> FoeFinishedGame;

        public event EventHandler<PlayerInfoMessage> ClanmateJoinedToGame;
        public event EventHandler<PlayerInfoMessage> ClanmateLeftFromGame;
        public event EventHandler<PlayerInfoMessage> ClanmateFinishedGame;

        public PlayerInfoMessage[] CachedPlayers { get; }

        public List<PlayerInfoMessage> Players { get; }
        public PlayerInfoMessage Self { get; }

        public PlayerInfoMessage GetPlayer(string idOrLogin);

        public bool TryGetPlayer(string login, out PlayerInfoMessage player);
        public bool TryGetPlayer(int id, out PlayerInfoMessage player);

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);
    }
}
