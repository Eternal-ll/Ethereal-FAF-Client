using beta.Infrastructure.Commands;
using beta.Infrastructure.Utils;
using FAF.Domain.Direct.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class NewsViewModel : ApiViewModel
    {
        private readonly string Url = "https://direct.faforever.com/wp-json/wp/v2/posts?_embed=author,wp:featuredmedia";
        private readonly HttpClient HttpClient;

        public NewsViewModel()
        {
            HttpClient = new();
            Posts = new();
            PostsViewSource = new();
            PostsViewSource.SortDescriptions.Add(new SortDescription(nameof(Post.NewshubSortIndex), ListSortDirection.Descending));
            PostsViewSource.SortDescriptions.Add(new SortDescription(nameof(Post.Date), ListSortDirection.Descending));
            PostsViewSource.Filter += PostsViewSource_Filter;
            PostsViewSource.Source = Posts;
            RunRequest();
        }

        //public Uri SidebarLeft { get; set; }
        //public Uri SidebarMid { get; set; }
        //public Uri SidebarRight { get; set; }

        public ObservableCollection<Post> Posts { get; private set; }
        CollectionViewSource PostsViewSource;
        public ICollectionView PostsView => PostsViewSource.View;

        #region FilterText
        private string _FilterText;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    PostsView.Refresh();
                }
            }
        }
        #endregion

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
        private Post _SelectedPost;
        public Post SelectedPost
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

        public int[] PageSizes => new int[] { 10, 20, 30, 60, 120 };

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

        private void PostsViewSource_Filter(object sender, FilterEventArgs e)
        {
            var post = (Post)e.Item;
            var filter = FilterText;
            e.Accepted = true;
            if (string.IsNullOrWhiteSpace(filter)) return;
            e.Accepted = post.Title.Text.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                post.Excerpt.Text.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                post.Content.Text.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private string BuildQuery()
        {
            StringBuilder sb = new();
            sb.Append($"&page={CurrentPage}");
            sb.Append($"&per_page={PerPage}");
            return sb.ToString();
        }

        protected override async Task RequestTask()
        {
            //SidebarRight = null;
            //SidebarLeft = null;
            //SidebarMid = null;
            var viewPosts = Posts;
            DispatcherHelper.RunOnMainThread(() => viewPosts.Clear());
            var query = BuildQuery();
            var response = await HttpClient.GetAsync(Url + query);
            var stream = await response.Content.ReadAsStreamAsync();
            var posts = await JsonSerializer.DeserializeAsync<List<Post>>(stream);
           
            for (int i = 0; i < posts.Count; i++)
            {
                var post = posts[i];
                if (post.Title.Text.Contains("NewsHub", StringComparison.OrdinalIgnoreCase))
                {
                    if (post.Title.Text.Contains("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (post.Title.Text.Contains("Left", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (post.Title.Text.Contains("Mid", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                post.Title.Text = WebUtility.HtmlDecode(post.Title.Text);
                post.Content.Text = WebUtility.HtmlDecode(post.Content.Text);
                post.Excerpt.Text = WebUtility.HtmlDecode(post.Excerpt.Text);
                DispatcherHelper.RunOnMainThread(() => viewPosts.Add(post));
            }
            if (posts.Any())
            {
                SelectedPost = posts[0];
            }

            // X-Wp-Totalpages
            var values = response.Headers.GetValues("X-Wp-Totalpages");

            TotalPages = values.Any() ? int.Parse(values.First()) : 1;
        }

        #region HideSelectedPostCommand
        private ICommand _HideSelectedPostCommand;
        public ICommand HideSelectedPostCommand => _HideSelectedPostCommand ??= new LambdaCommand(OnHideSelectedPostCommand, CanHideSelectedPostCommand);
        private bool CanHideSelectedPostCommand(object parameter) => SelectedPost is not null;
        private void OnHideSelectedPostCommand(object parameter) => SelectedPost = null;
        #endregion

        protected override void Dispose(bool disposing)
        {
            Posts.Clear();
            Posts = null;
            PostsViewSource.Filter -= PostsViewSource_Filter;
            PostsViewSource.Source = null;
            _HideSelectedPostCommand = null;
        }
    }
}
