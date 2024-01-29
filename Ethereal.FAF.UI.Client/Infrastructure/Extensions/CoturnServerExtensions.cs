using Ethereal.FAF.API.Client.Models.Attributes;
using Ethereal.FAF.API.Client.Models.Base;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    public static class CoturnServerExtensions
	{
		public static IceServer MapToDomain(this ApiUniversalData<CoturnServerAttributes> x)
			=> new IceServer()
		{
			Credential = x.Attributes.Credential,
			CredentialType = x.Attributes.CredentialType,
			Urls = x.Attributes.Urls,
			Username = x.Attributes.Username,
		};
	}
}
