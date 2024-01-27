using System.Web;

namespace Ethereal.FAF.API.Client
{
    public class VerifyHeaderHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            var verify = query["verify"];
            if (verify != null)
            {
                request.Headers.Add("Verify", HttpUtility.UrlEncode(verify));
                request.RequestUri = new Uri(request.RequestUri.Scheme + "://" + request.RequestUri.Host + request.RequestUri.LocalPath);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
