using Ethereal.FAF.UI.Client.Infrastructure.Queries.GetCoturnServers;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configurations;
using MediatR;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Queries.GetSelectedCoturnServers
{
	internal class GetSelectedCoturnServersQuery : IRequest<CoturnServer[]>
	{
		internal class GetSelectedCoturnServersQueryHandler : IRequestHandler<GetSelectedCoturnServersQuery, CoturnServer[]>
		{
			private readonly IMediator _mediator;
			private readonly IceAdapterConfiguration _options;

			public GetSelectedCoturnServersQueryHandler(IMediator mediator, IOptions<IceAdapterConfiguration> options)
			{
				_mediator = mediator;
				_options = options.Value;
			}

			public async Task<CoturnServer[]> Handle(GetSelectedCoturnServersQuery request, CancellationToken cancellationToken)
			{
				var servers = await _mediator.Send(new GetCoturnServersQuery(), cancellationToken);
				servers = servers
					.Where(x => x.Active && _options.SelectedCoturnServers.Contains(x.Id))
					.ToArray();
				return servers;
			}
		}
	}
}
