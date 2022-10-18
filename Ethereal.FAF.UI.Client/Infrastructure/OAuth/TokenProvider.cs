using Ethereal.FAF.UI.Client.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using static Ethereal.FAF.API.Client.BuilderExtensions;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public sealed class TokenProvider : ITokenProvider
    {
        public TokenBearer TokenBearer { get; 
            private set; }
        public JwtSecurityToken JwtSecurityToken { get; private set; }
        public TokenProvider(IConfiguration configuration)
        {
            var token = configuration.GetSection("TokenBearer").Get<TokenBearer>();
            if (token is not null)
            {
                TokenBearer = token;
                ProcessJwtToken(token);
            }
        }

        private void ProcessJwtToken(TokenBearer token)
        {
            if (token is null) return;
            if (token.AccessToken is null)
            {
                //    TokenBearer = null;
                //    AppSettings.Update("TokenBearer", token);
                return;
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadToken(token.AccessToken) as JwtSecurityToken;
            JwtSecurityToken = jwtSecurityToken;
        }

        public void Save(TokenBearer token)
        {
            ProcessJwtToken(token);
            TokenBearer = token;
            UserSettings.Update("TokenBearer", new Dictionary<string, object>()
            {
                { "AccessToken", token.AccessToken },
                { "ExpiresIn", token.ExpiresIn },
                { "IdToken", token.IdToken },
                { "RefreshToken", token.RefreshToken },
                { "TokenType", token.TokenType },
                { "Scope", token.Scope },
                { "Created", token.Created },
            });
        }

        public string GetToken() => TokenBearer.AccessToken;
    }
}
