using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    /// <summary>
    /// Remove player from favourite list
    /// </summary>
    internal class RemoveFavouriteCommand : Base.Command
    {
        private readonly IFavouritesService FavouritesService;
        public RemoveFavouriteCommand() => FavouritesService = App.Services.GetService<IFavouritesService>();
        public override bool CanExecute(object parameter) => parameter is int;
        public override void Execute(object parameter) => FavouritesService.AddFavouritePlayer((int)parameter);
    }
    /// <summary>
    /// Add player to favourite list
    /// </summary>
    internal class AddFavouriteCommand : Base.Command
    {
        private readonly IFavouritesService FavouritesService;
        public AddFavouriteCommand() => FavouritesService = App.Services.GetService<IFavouritesService>();
        public override bool CanExecute(object parameter) => parameter is int;
        public override void Execute(object parameter) => FavouritesService.RemoveFavouritePlayer((int)parameter);
    }
}
