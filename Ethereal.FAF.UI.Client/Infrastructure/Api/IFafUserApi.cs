using Refit;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Api
{
    public interface IFafUserApi
    {
        [Get("/lobby/access")]
        public Task<LobbyAccess> GetLobbyAccess(CancellationToken cancellationToken = default);
        public class LobbyAccess
        {
            [JsonPropertyName("accessUrl")]
            public Uri AccessUrl { get; set; }
        }
    }
}
