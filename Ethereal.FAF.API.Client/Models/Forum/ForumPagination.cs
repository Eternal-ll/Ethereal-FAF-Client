using System.Linq;

namespace beta.Models.API.Forum
{
    public class ForumPagination
    {
        public int currentPage { get; set; }
        public int pageCount { get; set; }
        public int[] Pages => Enumerable.Range(1, pageCount).ToArray();
    }
}
