using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    public static class GameInfoMessageExtensions
	{
		public static Game MapToViewModel(this GameInfoMessage x) => new Game()
		{
			//Command = x.Command,
			Uid = x.Uid,
			LaunchedAt = x.LaunchedAt,
			MaxPlayers = x.MaxPlayers,
			NumPlayers = x.NumPlayers,
			PasswordProtected = x.PasswordProtected,
			SimMods = x.SimMods.Count == 0 ? null : new(x.SimMods.Select(x => new SimMod()
            {
                Id = x.Key,
                Name = x.Value
            }).ToArray()),
			State = x.State,
			TeamsIds = x.TeamsIds,
			Teams = x.Teams,
			Visibility = x.Visibility,
			RatingMax = x.RatingMax,
			RatingMin = x.RatingMin,
			RatingType = x.RatingType,
			Title = x.Title,
			Host = x.Host,
			EnforceRatingRange = x.EnforceRatingRange,
			GameType = x.GameType,
			FeaturedMod = x.FeaturedMod,
			Map = MapGenerator.IsGeneratedMap(x.Mapname) ?
				new NeroxisGameMap(MapGenerator.Parse(x.Mapname)) : new CustomGameMap(GameMapHelper.Parse(x.Mapname))
		};
	}
}
