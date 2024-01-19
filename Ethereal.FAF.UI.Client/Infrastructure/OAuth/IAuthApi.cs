using Refit;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public interface IAuthApi
    {
        [Post("/oauth2/token")]
        public Task<TokenBearer> GetTokenByCode(
            [Body(BodySerializationMethod.UrlEncoded)] FetchTokenByCode model,
            CancellationToken cancellationToken = default);

        [Post("/oauth2/token")] 
        public Task<TokenBearer> GetTokenByRefreshToken(
            [Body(BodySerializationMethod.UrlEncoded)] FetchTokenByRefreshToken model,
            CancellationToken cancellationToken = default);

        public abstract class FetchToken
        {
            [AliasAs("grant_type")]
            public string GrantType { get; }
            [AliasAs("client_id")]
            public string ClientId { get; }

            protected FetchToken(string grantType, string clientId)
            {
                GrantType = grantType;
                ClientId = clientId;
            }
            public static FetchTokenByCode ByCode(string code, string clientId, string redirectUri) => new(code, clientId, redirectUri);
            public static FetchTokenByRefreshToken ByRefreshToken(string refreshToken, string clientId) => new(refreshToken, clientId);
        }
        public class FetchTokenByCode : FetchToken
        {
            [AliasAs("code")]
            public string Code { get; }
            [AliasAs("redirect_uri")]
            public string RedirectUri { get; set; }
            public FetchTokenByCode(string code, string clientId, string redirectUri) : base("authorization_code", clientId)
            {
                Code = code;
                RedirectUri = redirectUri;

            }
        }
        public class FetchTokenByRefreshToken : FetchToken
        {
            public FetchTokenByRefreshToken(string refreshToken, string clientId) : base("refresh_token", clientId)
            {
                RefreshToken = refreshToken;
            }

            [AliasAs("refresh_token")]
            public string RefreshToken { get;}
        }
    }
}
