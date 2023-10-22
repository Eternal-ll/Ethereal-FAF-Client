using Ethereal.FAF.API.Client.Models.Base;
using Ethereal.FAF.API.Client.Models.Clans;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public interface IFafClansService : IFafApi<ClanDto>
    {
        [Get("/data/clan")]
        Task<ApiResponse<ApiListResult<ClanDto>>> Get(
            [AliasAs("filter")]
            //[Query(Format = ";")]
            string filter = null,
            Sorting? sorting = null,
            Pagination? pagination = null,
            [AliasAs("include")]
            [Query(CollectionFormat.Csv)]
        string[] include = null,
            CancellationToken cancellationToken = default);
    }
}
