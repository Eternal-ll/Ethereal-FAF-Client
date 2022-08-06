using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace beta.Infrastructure.Services
{
    internal class SelfService : ISelfService
    {
        public PlayerInfoMessage Self { get; private set; }

        public event EventHandler<PlayerInfoMessage> SelfUpdated;
        
        private readonly IPlayersService PlayersService;

        public SelfService(IPlayersService playersService)
        {
            PlayersService = playersService;
            playersService.SelfReceived += PlayersService_SelfReceived;
            playersService.PlayerUpdated += PlayersService_PlayerUpdated;
        }

        private void PlayersService_SelfReceived(object sender, PlayerInfoMessage e)
        {
            Self = e;
            SelfUpdated?.Invoke(this, e);
        }

        private void PlayersService_PlayerUpdated(object sender, PlayerInfoMessage e)
        {
            if (e.RelationShip != Models.Server.Enums.PlayerRelationShip.Me) return;
            Self = e;
            SelfUpdated?.Invoke(this, e);
        }
    }
}
