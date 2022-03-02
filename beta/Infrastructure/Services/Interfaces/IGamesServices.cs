using beta.Models.Server;
using beta.ViewModels;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGamesServices
    {
        public ObservableCollection<GameVM> IdleGames { get; }
        public ObservableCollection<GameVM> LiveGames { get; }
    }
}
