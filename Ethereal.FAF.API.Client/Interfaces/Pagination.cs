using Refit;

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
}
