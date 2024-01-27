using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
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
