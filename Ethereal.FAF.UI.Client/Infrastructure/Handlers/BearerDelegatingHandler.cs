using Ethereal.FAF.API.Client;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Handlers
{
    internal class BearerDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenProvider TokenProvider;

        public BearerDelegatingHandler(ITokenProvider tokenProvider)
        {
            TokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("Authorization")) return await base.SendAsync(request, cancellationToken);
            
            var token = await TokenProvider.GetAccessTokenAsync(cancellationToken);
            request.Headers.Add("Authorization", "Bearer + " + token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
