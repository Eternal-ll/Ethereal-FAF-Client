using beta.ViewModels.Base;

namespace beta.ViewModels
{

    public class HomeViewModel : ViewModel
    {       
        public HomeViewModel()
        {
            NewsViewModel = new();
            ForumViewModel = new();
        }

        public NewsViewModel NewsViewModel { get; set; }
        public ForumViewModel ForumViewModel { get; set; }
    }
}
