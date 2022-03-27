using beta.Models.Enums;
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

        public PlayerInfoMessage GetPlayer(string login);
        public PlayerInfoMessage GetPlayer(int id);

        public bool TryGetPlayer(string login, out PlayerInfoMessage player);
        public bool TryGetPlayer(int id, out PlayerInfoMessage player);

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);
        #endregion
    }
}
