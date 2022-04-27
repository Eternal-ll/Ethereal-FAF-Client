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

        public PlayerInfoMessage[] CachedPlayers { get; }

        public PlayerInfoMessage[] Players { get; }
        public PlayerInfoMessage Self { get; }

        public PlayerInfoMessage GetPlayer(string idOrLogin);

        public bool TryGetPlayer(string login, out PlayerInfoMessage player);
        public bool TryGetPlayer(int id, out PlayerInfoMessage player);

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);
    }
}
