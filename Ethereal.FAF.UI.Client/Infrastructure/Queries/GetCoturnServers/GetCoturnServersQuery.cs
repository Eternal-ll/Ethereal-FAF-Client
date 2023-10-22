using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Models;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Queries.GetCoturnServers
{
	internal class GetCoturnServersQuery : IRequest<CoturnServer[]>
	{
		internal class GetCoturnServersQueryHandler : IRequestHandler<GetCoturnServersQuery, CoturnServer[]>
		{
			private readonly IFafApiClient _fafApiClient;

			public GetCoturnServersQueryHandler(IFafApiClient fafApiClient)
			{
				_fafApiClient = fafApiClient;
			}

			public async Task<CoturnServer[]> Handle(GetCoturnServersQuery request, CancellationToken cancellationToken) => await _fafApiClient
				.GetCoturnServersAsync(cancellationToken)
				.ContinueWith(x =>
				{
					if (x.IsFaulted)
					{
						return Array.Empty<CoturnServer>();
					}
					return x.Result.Data
					.Select(x => new CoturnServer()
					{
						Id = x.Id,
						Active = x.Attributes.Active,
						Credential = x.Attributes.Credential,
						CredentialType = x.Attributes.CredentialType,
						Region = x.Attributes.Region,
						Urls = x.Attributes.Urls,
						Username = x.Attributes.Username
					})
					.ToArray();
				});
		}
	}
}
