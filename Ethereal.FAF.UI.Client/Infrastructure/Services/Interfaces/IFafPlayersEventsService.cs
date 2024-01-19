using Ethereal.FAF.UI.Client.ViewModels;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// FAF lobby players events
    /// </summary>
    public interface IFafPlayersEventsService
    {
        public event EventHandler<Player[]> PlayersAdded;
        public event EventHandler<Player[]> PlayersUpdated;
        public event EventHandler<Player[]> PlayersRemoved;
        /// <summary>
        /// <see cref="Player"/> connected to lobby
        /// </summary>
        public event EventHandler<Player> PlayerConnected;
        /// <summary>
        /// <see cref="Player"/> disconnected from lobby
        /// </summary>
        public event EventHandler<Player> PlayerDisconnected;
    }
}
