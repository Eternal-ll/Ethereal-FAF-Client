using System;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Service for favourite players
    /// </summary>
    public interface IFavouritesService
    {
        public event EventHandler<int> FavouriteAdded;
        public event EventHandler<int> FavouriteRemoved;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int[] GetFavouritePlayers();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void AddFavouritePlayer(int id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void RemoveFavouritePlayer(int id);
    }
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
