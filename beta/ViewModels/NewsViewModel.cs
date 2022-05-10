using beta.Infrastructure.Commands;
using beta.Models.API.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class NewsViewModel : ApiViewModel
    {
        private string Url = "https://direct.faforever.com/wp-json/wp/v2/posts?_embed=author,wp:featuredmedia&_fields[]=_links&_fields[]=title&_fields[]=date&_fields[]=content&_fields[]=newshub_externalLinkUrl";

        public NewsViewModel() => RunRequest();

        public Uri SidebarLeft { get; set; }
        public Uri SidebarMid { get; set; }
        public Uri SidebarRight { get; set; }

        public PostModel[] Posts { get; set; }

        #region CurrentPage
        private int _CurrentPage = 1;
        public int CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (Set(ref _CurrentPage, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        #region SelectedPost
        private IPostModel _SelectedPost = new PlugViewModel();
        public IPostModel SelectedPost
        {
            get => _SelectedPost;
            set => Set(ref _SelectedPost, value);
        }
        #endregion

        #region TotalPages
        private int _TotalPages;
        public int TotalPages
        {
            get => _TotalPages;
            set
            {
                if (Set(ref _TotalPages, value))
                {
                    OnPropertyChanged(nameof(Pages));
                }
            }
        }
        #endregion


        public int[] Pages => Enumerable.Range(1, TotalPages).ToArray();

        public int[] PageSizes => new int[] { 10, 15, 20, 25, 30, 35 };


        #region PerPage
        private int _PerPage = 20;
        public int PerPage
        {
            get => _PerPage;
            set
            {
                if (Set(ref _PerPage, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        private string BuildQuery()
        {
            StringBuilder sb = new();
            sb.Append($"&page={CurrentPage}");

            sb.Append($"&per_page={PerPage}");

            return sb.ToString();
        }

        protected override async Task RequestTask()
        {
            SidebarRight = null;
            SidebarLeft = null;
            SidebarMid = null;

            var query = BuildQuery();   
            WebRequest request = WebRequest.Create(Url + query);
            var response = await request.GetResponseAsync();
            var posts = await JsonSerializer.DeserializeAsync<List<PostModel>>(response.GetResponseStream());
           
            for (int i = 0; i < posts.Count; i++)
            {
                var post = posts[i];
                if (post.Title.Text.Contains("NewsHub", StringComparison.OrdinalIgnoreCase))
                {
                    if (post.Title.Text.Contains("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        SidebarRight = post.Embedded.Media[0].ImageUrl;
                        posts[i] = null;
                    }
                    else if (post.Title.Text.Contains("Left", StringComparison.OrdinalIgnoreCase))
                    {
                        SidebarLeft = post.Embedded.Media[0].ImageUrl; posts[i] = null;
                    }
                    else if (post.Title.Text.Contains("Mid", StringComparison.OrdinalIgnoreCase))
                    {
                        SidebarMid = post.Embedded.Media[0].ImageUrl; posts[i] = null;
                    }
                }
                post.Title.Text = WebUtility.HtmlDecode(post.Title.Text);
                post.Content.Text = WebUtility.HtmlDecode(post.Content.Text);
            }
            OnPropertyChanged(nameof(SidebarRight));
            OnPropertyChanged(nameof(SidebarMid));
            OnPropertyChanged(nameof(SidebarLeft));

            // X-Wp-Totalpages
            var data = response.Headers.GetValues("X-Wp-Totalpages");
            TotalPages = data.Length > 0 ? int.Parse(data[0]) : 0;

            Posts = posts.ToArray();
            OnPropertyChanged(nameof(Posts));
        }
        #region HideSelectedPostCommand
        private ICommand _HideSelectedPostCommand;
        public ICommand HideSelectedPostCommand => _HideSelectedPostCommand ??= new LambdaCommand(OnHideSelectedPostCommand, CanHideSelectedPostCommand);
        private bool CanHideSelectedPostCommand(object parameter) => SelectedPost is not null;
        private void OnHideSelectedPostCommand(object parameter) => SelectedPost = new PlugViewModel();
        #endregion
    }
}
