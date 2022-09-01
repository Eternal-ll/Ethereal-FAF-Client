using Ethereal.FAF.UI.Client.Properties;
using System.IdentityModel.Tokens.Jwt;
using static Ethereal.FAF.API.Client.BuilderExtensions;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public class TokenProvider : ITokenProvider
    {
        // /init init_faf.lua /gpgnet 127.0.0.1:5636 /mean 1500 /deviation 500 /numgaes /country RU /clan ZFG
        public TokenBearer TokenBearer { get; private set; }
        public JwtSecurityToken JwtSecurityToken { get; private set; }
        public TokenProvider()
        {
            var token = Settings.Default.Token;
            if (token is not null)
            {
                TokenBearer = token;
                ProcessJwtToken(token);
            }
        }

        private void ProcessJwtToken(TokenBearer token)
        {
            if (token.AccessToken is null)
            {
                TokenBearer = token;
                Settings.Default.Token = token;
                Settings.Default.Save();
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
            Settings.Default.Token = token;
            Settings.Default.Save();
        }

        public string GetToken() => TokenBearer.AccessToken;
    }
}
