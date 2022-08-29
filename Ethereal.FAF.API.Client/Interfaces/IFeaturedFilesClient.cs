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
            [Get("featuredMods/{featuredMod:int}/files/latest")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(FeaturedMod featuredMod, [Authorize("Bearer")] string token);

            [Get("featuredMods/{featuredMod:int}/files/{version:int}")]
            Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(FeaturedMod featuredMod, int version, [Authorize("Bearer")] string token);

        }
    }
}
