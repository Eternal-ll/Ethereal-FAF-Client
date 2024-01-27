using System.Net.Http.Headers;

namespace Ethereal.FAF.API.Client
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenProvider TokenProvider;

        public AuthHeaderHandler(ITokenProvider tenantProvider)
        {
            this.TokenProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await TokenProvider.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
