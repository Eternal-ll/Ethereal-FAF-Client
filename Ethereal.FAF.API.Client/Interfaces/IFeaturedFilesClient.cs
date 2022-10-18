using beta.Models.API;
using beta.Models.API.Base;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public static partial class BuilderExtensions
    {
        public interface IFafApiClient
        {
            [Get("/data/featuredMods/{featuredMod}/files/latest")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetLatestAsync(int featuredMod, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

            [Get("/data/featuredMods/{featuredMod}/files/{version}")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(int featuredMod, int version, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

            [Get("/data/coturnServer")]
            Task<ApiResponse<ApiUniversalResult<CoturnServer>>> GetCoturnServersAsync([Authorize("Bearer")] string token, CancellationToken cancellationToken = default);
        }
        public interface IFafContentClient
        {
            [Get("/{url}")]
            [QueryUriFormat(UriFormat.Unescaped)]
            Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);
        }
    } 
}
