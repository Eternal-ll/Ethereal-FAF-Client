using Ethereal.FAF.API.Client.Models.Attributes;
using Ethereal.FAF.API.Client.Models.Base;
using FAF.Domain.LobbyServer;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
	public static class CoturnServerExtensions
	{
		public static IceCoturnServer MapToDomain(this ApiUniversalData<CoturnServerAttributes> x)
			=> new IceCoturnServer()
		{
			Active = x.Attributes.Active,
			Credential = x.Attributes.Credential,
			CredentialType = x.Attributes.CredentialType,
			Region = x.Attributes.Region,
			Urls = x.Attributes.Urls,
			Username = x.Attributes.Username,
		};
	}
}
