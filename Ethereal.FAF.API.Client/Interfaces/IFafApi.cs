using Ethereal.FAF.API.Client.Models.Base;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public interface IFafApi<T> where T : class
    {
        Task<ApiResponse<ApiListResult<T>>> Get(
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
