using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.API.Client.Models.Attributes;
using Ethereal.FAF.API.Client.Models.Base;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public class Filtration
    {

    }
    public interface IFafApiClient
    {
        [Get("/featuredMods/{featuredMod}/files/latest")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetLatestAsync(int featuredMod, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/featuredMods/{featuredMod}/files/{version}")]
        Task<ApiResponse<ApiUniversalResult<FeaturedModFile[]>>> GetAsync(int featuredMod, int version, [Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/data/coturnServer")]
        Task<FafApiResult<Entity<CoturnServerAttributes>[]>> GetCoturnServersAsync(CancellationToken cancellationToken = default);

        [Get("/data/map")]
        Task<ApiResponse<ApiMapsResult>> GetMapsAsync(
            [AliasAs("filter")]
            //[Query(Format = ";")]
            string filter = null,
            Sorting? sorting = null,
            Pagination? pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);
        [Get("/data/mod")]
        Task<ApiResponse<ModsResult>> GetModsAsync(string filter = null, Sorting sorting = null, Pagination pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);

        #region ModerationReport
        /// <summary>
        /// Get all reports
        /// </summary>
        /// <returns></returns>
        [Get("/data/moderationReport")]
        Task<ApiResponse<Response<ModerationReport>>> GetModerationReportAsync();
        #endregion



        #region Games
        [Get("/data/game")]
        Task<ApiResponse<Response<Model<Game>[]>>> GetGamesAsync(string filter, Sorting sorting = default, Pagination pagination = default,
            [AliasAs("include")] [Query(CollectionFormat.Csv)]
            string[] include = null,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<Response<Model<Game>[]>>> GetPlayerGamesAsync(string login, Sorting sorting = default, Pagination pagination = default,
            string[] include = null, CancellationToken cancellationToken = default)
            => GetGamesAsync($"playerStats.player.login=={login}", sorting, pagination, include, cancellationToken);

        Task<ApiResponse<Response<Model<Game>[]>>> GetPlayerGamesAsync(long id, Sorting sorting = default, Pagination pagination = default,
            string[] include = null, CancellationToken cancellationToken = default)
            => GetGamesAsync($"playerStats.player.id=={id}", sorting, pagination, include, cancellationToken);

        #endregion
    }
    public interface IFafContentClient
    {
        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);

        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <example>https://content.faforever.com/maps/astro_crater_battles_4x4_rich_huge.v0004.zip</example>
        /// <param name="map">Map name</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Get("/maps/{map}.zip")]
        Task<ApiResponse<Stream>> GetMapStreamAsync(string map, CancellationToken cancellationToken = default);
    }
}
