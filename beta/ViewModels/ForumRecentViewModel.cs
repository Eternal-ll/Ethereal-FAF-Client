using beta.Models.API.Forum;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    public class ForumRecentViewModel : ApiViewModel
    {
        private static string UnreadURL = "https://forum.faforever.com/api/recent";
        public ForumRecentViewModel() => RunRequest();

        #region ApiForumRecentResult
        private ApiForumRecentResult _ApiForumRecentResult;
        public ApiForumRecentResult ApiForumRecentResult
        {
            get => _ApiForumRecentResult;
            set => Set(ref _ApiForumRecentResult, value);
        }
        #endregion

        #region SelectedTerm
        private string _SelectedTerm = "daily";
        public string SelectedTerm
        {
            get => _SelectedTerm;
            set
            {
                if (Set(ref _SelectedTerm, value))
                {
                    if (SelectedPage == 1) _SelectedPage = 0;
                    SelectedPage = 1;
                }
            }
        }
        #endregion

        #region SelectedPage
        private int _SelectedPage = 1;
        public int SelectedPage
        {
            get => _SelectedPage;
            set
            {
                if (Set(ref _SelectedPage, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        private string BuildQuery()
        {
            StringBuilder sb = new();
            sb.Append($"?page={SelectedPage}");

            if (!string.IsNullOrWhiteSpace(SelectedTerm))
            {
                sb.Append($"&term={SelectedTerm}");
            }

            return sb.ToString();
        }

        protected override async Task RequestTask()
        {
            var query = BuildQuery();

            WebRequest request = WebRequest.Create(UnreadURL + query);
            var result = await JsonSerializer.DeserializeAsync<ApiForumRecentResult>(request.GetResponse().GetResponseStream());
            ApiForumRecentResult = result;
        }
    }
}
