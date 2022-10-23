using beta.Models.API;
using beta.Models.API.Base;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public interface IFafApiClient
    {
        [Get("/featuredMods/{featuredMod}/files/latest")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetLatestAsync(int featuredMod, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/featuredMods/{featuredMod}/files/{version}")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(int featuredMod, int version, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/data/coturnServer")]
        Task<ApiResponse<ApiUniversalResult<CoturnServer[]>>> GetCoturnServersAsync([Authorize("Bearer")] string token, CancellationToken cancellationToken = default);
    }
    public interface IFafContentClient
    {
        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);
    }
}
