using beta.ViewModels.Base;

namespace beta.ViewModels
{
    public class ForumViewModel : ViewModel
    {
        public ForumViewModel()
        {
            Recent = new();
        }
        public ForumRecentViewModel Recent { get; set; }
    }
}
