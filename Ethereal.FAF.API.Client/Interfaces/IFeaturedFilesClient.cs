using beta.Models.API;
using beta.Models.API.Base;
using FAF.Domain.LobbyServer.Enums;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public static partial class BuilderExtensions
    {
        public interface IFeaturedFilesClient
        {
            [Get("/featuredMods/{featuredMod}/files/latest")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetLatestAsync(int featuredMod, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

            [Get("/featuredMods/{featuredMod}/files/{version}")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(int featuredMod, int version, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

            [Get("/{url}")]
            Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);
        }
    }
}
