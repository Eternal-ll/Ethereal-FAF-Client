using beta.Models.API;
using beta.Models.API.Base;
using Refit;
using System.ComponentModel;

namespace Ethereal.FAF.API.Client
{
    public class Pagination
    {
        [AliasAs("page[totals]")]
        public bool PageTotals { get; set; } = true;
        [AliasAs("page[size]")]
        public int PageSize { get; set; }
        [AliasAs("page[number]")]
        public int PageNumber { get; set; }
    }
    public class Sorting
    {
        public ListSortDirection SortDirection { get; set; }
        private string _Sort;
        [AliasAs("sort")]
        public string Sort
        {
            get
            {
                return
                    SortDirection is ListSortDirection.Descending ?
                    "-" : string.Empty +
                    Parameter;
            }
            set
            {
                _Sort = value;
            }
        }
        public string Parameter { get; set; }
    }
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
        Task<ApiResponse<ApiUniversalResult<CoturnServer[]>>> GetCoturnServersAsync([Authorize("Bearer")] string token, CancellationToken cancellationToken = default);

        [Get("/data/map")]
        Task<ApiResponse<ApiMapsResult>> GetMapsAsync(
            [AliasAs("filter")]
            //[Query(Format = ";")]
            string filter = null,
            Sorting? sorting = null,
            Pagination? pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
            params string[] include);
    }
    public interface IFafContentClient
    {
        [Get("/{url}")]
        [QueryUriFormat(UriFormat.Unescaped)]
        Task<ApiResponse<Stream>> GetFileStreamAsync(string url, [Authorize("Bearer")] string token, [Header("Verify")] string verify, CancellationToken cancellationToken = default);
    }
}
