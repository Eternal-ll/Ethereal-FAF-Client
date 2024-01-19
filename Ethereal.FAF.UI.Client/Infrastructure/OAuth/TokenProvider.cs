using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TokenProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _serviceProvider.GetService<IFafAuthService>().GetActualAccessToken(cancellationToken);
        }
    }
}
