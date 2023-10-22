using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
	public static class GameInfoMessageExtensions
	{
		public static Game MapToViewModel(this GameInfoMessage x) => new Game()
		{
			Command = x.Command,
			Uid = x.Uid,
			LaunchedAt = x.LaunchedAt,
			MapFilePath = x.MapFilePath,
			MaxPlayers = x.MaxPlayers,
			NumPlayers = x.NumPlayers,
			PasswordProtected = x.PasswordProtected,
			SimMods = x.SimMods,
			State = x.State,
			TeamsIds = x.TeamsIds,
			Teams = x.Teams,
			Visibility = x.Visibility,
			RatingMax = x.RatingMax,
			RatingMin = x.RatingMin,
			RatingType = x.RatingType,
			Title = x.Title,
			Host = x.Host,
			Mapname = x.Mapname,
			EnforceRatingRange = x.EnforceRatingRange,
			GameType = x.GameType,
			FeaturedMod = x.FeaturedMod
		};
	}
	public static class PlayerInfoMessageExtensions
	{
		public static Player MapToViewModel(this PlayerInfoMessage x) => new Player()
		{
			Command = x.Command,
			Id = x.Id,
			Login = x.Login,
			Avatar = x.Avatar,
			Clan = x.Clan,
			Country = x.Country,
			NumberOfGames = x.NumberOfGames,
			Ratings = x.Ratings
		};
	}
	public static class WelcomeDataExtensions
	{
		public static Welcome MapToViewModel(this WelcomeData data) => new()
		{
			Command = data.Command,
			id = data.id,
			login = data.login,
			me = data.me.MapToViewModel()
		};
	}
}
