using beta.Models.Server;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGamesServices
    {
        public ObservableCollection<GameInfoMessage> IdleGames { get; }
        public ObservableCollection<GameInfoMessage> LiveGames { get; }
    }
}
