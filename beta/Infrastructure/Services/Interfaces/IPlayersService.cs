using beta.Models.Server;
using beta.Models.Server.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IPlayersService : INotifyPropertyChanged
    {
        #region Properties
        
        public ObservableCollection<PlayerInfoMessage> Players { get; }

        #endregion

        #region Functions

        public IPlayer GetPlayer(string login);
        public PlayerInfoMessage GetPlayer(int id);

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);
        #endregion
    }
}
