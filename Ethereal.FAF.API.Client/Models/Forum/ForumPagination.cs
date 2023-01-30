using System.Linq;

namespace Ethereal.FAF.API.Client.Models.Forum
{
    public class ForumPagination
    {
        public int currentPage { get; set; }
        public int pageCount { get; set; }
        public int[] Pages => Enumerable.Range(1, pageCount).ToArray();
    }
}
