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
}
