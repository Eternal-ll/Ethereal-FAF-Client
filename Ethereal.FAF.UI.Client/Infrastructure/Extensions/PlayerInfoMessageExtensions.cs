using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    public static class PlayerInfoMessageExtensions
	{
		public static Player MapToViewModel(this PlayerInfoMessage x) => new Player()
		{
			Id = x.Id,
			Login = x.Login,
			Avatar = x.Avatar,
			Clan = x.Clan,
			Country = x.Country,
			Ratings = x.Ratings,
			State = x.State,
		};
	}
}
