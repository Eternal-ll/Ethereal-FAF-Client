using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public abstract class Link
    {
        public string Title { get; set; }
        public Uri Url { get; set; }
    }
    public class ResourceLink : Link
    {

    }
    public class PlayerLink : Link
    {
        public long PlayerId { get; set; }
        public Player Player { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
    }
    public class LinksGroup
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, Uri> Links { get; set; }
        public Dictionary<string, Uri> LinksView { get; set; }
        public bool IsExpanded { get; set; }
        public void Filter(params string[] words)
        {
            LinksView = new(Links.Where(l =>
            {
                foreach (var word in words)
                {
                    if (l.Key.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }));
        }
        public void DropFilter()
        {
            LinksView = Links;
        }
    }
    public class LinksViewModel : Base.ViewModel
    {
        private readonly IHttpClientFactory HttpClientFactory;
        public LinksViewModel(IHttpClientFactory httpClientFactory)
        {
            LinksGroups = new();
            LinksGroupsSource = new();
            LinksGroupsSource.Source = LinksGroups.AsObservable;
            LinksGroupsSource.Filter += LinksGroupsSource_Filter;
            HttpClientFactory = httpClientFactory;
            Task.Run(async () =>
            {
                using var client = httpClientFactory.CreateClient();
                var groups = await client.GetFromJsonAsync<LinksGroup[]>("https://jsonblob.com/api/jsonBlob/1029400916503314432");
                LinksGroups.AddRange(groups);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var configuration = new ConfigurationBuilder().AddJsonFile("links.json").Build();
                    var groups = configuration.GetSection("Groups").Get<LinksGroup[]>();
                    LinksGroups.AddRange(groups);
                }
            });
        }
        private ConcurrentObservableCollection<LinksGroup> LinksGroups { get; set; }
        private readonly CollectionViewSource LinksGroupsSource;
        public ICollectionView LinksGroupsView => LinksGroupsSource.View;

        #region FilterText
        private string _FilterText;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    LinksGroupsView.Refresh();
                }
            }
        }
        #endregion

        private void LinksGroupsSource_Filter(object sender, FilterEventArgs e)
        {
            var group = (LinksGroup)e.Item;
            e.Accepted = false;
            var filter = FilterText;
            group.DropFilter();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var words = filter.Split();
                foreach (var word in words)
                {
                    if (!(group.Title.Contains(word, System.StringComparison.OrdinalIgnoreCase) ||
                        group.Description.Contains(word, System.StringComparison.OrdinalIgnoreCase) ||
                        group.Links.Any(l => l.Key.Contains(word, System.StringComparison.OrdinalIgnoreCase))))
                    {
                        return;
                    }
                }
                group.Filter(words);
            }
            e.Accepted = true;
        }
    }
}
