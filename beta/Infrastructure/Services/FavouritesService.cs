using beta.Infrastructure.Services.Interfaces;
using System;

namespace beta.Infrastructure.Services
{
    public class FavouritesService : IFavouritesService
    {
        public event EventHandler<int> FavouriteAdded;
        public event EventHandler<int> FavouriteRemoved;

        public void AddFavouritePlayer(int id)
        {
            FavouriteAdded?.Invoke(this, id);
        }

        public int[] GetFavouritePlayers() => Array.Empty<int>();

        public void RemoveFavouritePlayer(int id)
        {
            FavouriteRemoved?.Invoke(this, id);
        }
    }
}
